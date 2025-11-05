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
                    const hasEffect = Object.prototype.hasOwnProperty.call(item, 'effect');

                    if (!(hasDuration || hasEffect)) {
                        // No duration/effect -> do nothing
                        continue;
                    }

                    const tagToAdd = hasMax ? 't_power_up' : 't_repeatable_task';

                    // Parse existing tags from both fields to avoid duplicates, but only write to one field
                    const existing = new Set();
                    const parse = (val) => {
                        if (typeof val === 'string' && val.trim().length > 0) {
                            val.split(',').map(t => t.trim()).filter(Boolean).forEach(t => existing.add(t));
                        }
                    };
                    parse(item.tags);
                    parse(item.tag);

                    if (existing.has(tagToAdd)) {
                        continue; // already tagged somewhere
                    }

                    // Choose a single target field to write
                    let targetField;
                    if (typeof item.tags === 'string') targetField = 'tags';
                    else if (typeof item.tag === 'string') targetField = 'tag';
                    else targetField = 'tags'; // create 'tags' if neither exists

                    const current = (typeof item[targetField] === 'string') ? item[targetField] : '';
                    const list = current ? current.split(',').map(t => t.trim()).filter(Boolean) : [];
                    list.push(tagToAdd);
                    item[targetField] = list.join(',');
                    modified = true;
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
