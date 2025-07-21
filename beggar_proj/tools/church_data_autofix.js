const fs = require('fs');
const path = require('path');
const { dialog, app } = require('electron');
const { exec } = require('child_process');

const CONFIG_PATH = './config.json';
const WHITELIST = new Set(['churchporter', 'acolyte', 'lector', 'priest', 'goodpriest']);

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
        const tagToAdd = 'not_church_class_automated';
        for (const obj of jsonData) {
            if (obj.type === 'CLASS' && Array.isArray(obj.items)) {
                for (const item of obj.items) {
                    if (!WHITELIST.has(item.id)) {
                        console.log(item.id);
                        console.log(item.tags);
                        if (typeof item.tags !== 'string' || item.tags.trim() === '') {
                            item.tags = tagToAdd;
                            modified = true;
                        } else if (!item.tags.split(',').map(t => t.trim()).includes(tagToAdd)) {
                            item.tags += ',' + tagToAdd;
                            modified = true;
                        }
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

// Electron setup just for folder selection
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
