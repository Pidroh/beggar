const fs = require('fs');
const path = require('path');
const { exec } = require('child_process');

const CONFIG_PATH = './config.json';

async function main() {
    // 1. Load or ask for folder
    let folderPath;
    if (fs.existsSync(CONFIG_PATH)) {
        const config = JSON.parse(fs.readFileSync(CONFIG_PATH, 'utf8'));
        folderPath = config.folderPath;
    } else {
        folderPath = await askForFolder();
        if (!folderPath) return;
        fs.writeFileSync(CONFIG_PATH, JSON.stringify({ folderPath }, null, 2));
    }

    // 2. Process matching JSON files
    const files = fs.readdirSync(folderPath).filter(f => f.startsWith('main_data_') && f.endsWith('.json'));

    for (const file of files) {
        const fullPath = path.join(folderPath, file);
        const jsonData = JSON.parse(fs.readFileSync(fullPath, 'utf8'));

        let modified = false;

        for (const obj of jsonData) {
            if (obj.type === 'TASK' && Array.isArray(obj.items)) {
                for (const item of obj.items) {
                    const hasMax = Object.prototype.hasOwnProperty.call(item, 'max');
                    const hasDuration = Object.prototype.hasOwnProperty.call(item, 'duration');

                    let tagToAdd = null;
                    if (hasMax) {
                        tagToAdd = 't_power_up';
                    } else if (!hasMax && hasDuration) {
                        tagToAdd = 't_repeatable_task';
                    }

                    if (!tagToAdd) continue;

                    // Prefer modifying whichever field exists (tags or tag). If neither exists, create 'tags'.
                    const existingTags = [];
                    const sources = [];
                    if (typeof item.tags === 'string' && item.tags.trim().length > 0) {
                        sources.push('tags');
                        existingTags.push(...item.tags.split(',').map(t => t.trim()).filter(Boolean));
                    }
                    if (typeof item.tag === 'string' && item.tag.trim().length > 0) {
                        sources.push('tag');
                        existingTags.push(...item.tag.split(',').map(t => t.trim()).filter(Boolean));
                    }

                    const hasAlready = existingTags.includes(tagToAdd);
                    if (hasAlready) continue;

                    if (sources.includes('tags')) {
                        // Append to tags
                        item.tags = (item.tags && item.tags.trim().length > 0)
                            ? (item.tags + ',' + tagToAdd)
                            : tagToAdd;
                        modified = true;
                    } else if (sources.includes('tag')) {
                        // Append to tag
                        item.tag = (item.tag && item.tag.trim().length > 0)
                            ? (item.tag + ',' + tagToAdd)
                            : tagToAdd;
                        modified = true;
                    } else {
                        // Neither present; create tags
                        item.tags = tagToAdd;
                        modified = true;
                    }
                }
            }
        }

        if (modified) {
            fs.writeFileSync(fullPath, JSON.stringify(jsonData, null, 4));
            console.log(`Updated: ${file}`);
        } else {
            console.log(`No changes: ${file}`);
        }
    }
}

// Electron setup just for folder selection (same pattern as church_data_autofix.js)
function askForFolder() {
    return new Promise((resolve) => {
        const electronCode = `
            const { dialog, app } = require('electron');
            app.whenReady().then(async () => {
                const result = await dialog.showOpenDialog({ properties: ['openDirectory'] });
                if (!result.canceled && result.filePaths.length > 0) {
                    console.log(result.filePaths[0]);
                }
                app.exit();
            });
        `;
        const tempFile = './__temp-electron.js';
        fs.writeFileSync(tempFile, electronCode);
        exec('npx electron ' + tempFile, (err, stdout) => {
            fs.unlinkSync(tempFile);
            if (err) return resolve(null);
            const selected = stdout.toString().trim();
            resolve(selected || null);
        });
    });
}

main();

