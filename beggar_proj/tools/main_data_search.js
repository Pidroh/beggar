const fs = require('fs');
const path = require('path');
const readline = require('readline');
const { exec } = require('child_process');

const CONFIG_PATH = './config.json';
const REQUIRE_PREFIX = 'require:';

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

    // 2. Repeatedly ask user for search term
    const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout,
    });

    function ask() {
        rl.question('Enter search term (empty to quit): ', (answer) => {
            const raw = answer.trim();
            if (!raw) {
                rl.close();
                console.log('Exiting.');
                return;
            }
            let mode = 'default';
            let searchTerm = raw;
            const lowerRaw = raw.toLowerCase();
            if (lowerRaw.startsWith(REQUIRE_PREFIX)) {
                mode = 'require';
                searchTerm = raw.slice(REQUIRE_PREFIX.length).trim();
                if (!searchTerm) {
                    console.log('No term provided after "require:". Try again.');
                    ask();
                    return;
                }
            }
            runSearch(folderPath, searchTerm, mode);
            ask();
        });
    }

    ask();
}

function runSearch(folderPath, searchTerm, mode = 'default') {
    const termLower = searchTerm.toLowerCase();

    // 2. Process matching JSON files
    const files = fs
        .readdirSync(folderPath)
        .filter(f => f.startsWith('main_data_') && f.endsWith('.json'));

    if (files.length === 0) {
        console.log('No main_data_*.json files found in folder:', folderPath);
        return;
    }

    let hits = 0;

    for (const file of files) {
        const fullPath = path.join(folderPath, file);
        let jsonData;
        try {
            jsonData = JSON.parse(fs.readFileSync(fullPath, 'utf8'));
        } catch (err) {
            console.error('Failed to parse JSON for file:', file, err.message);
            continue;
        }

        if (!Array.isArray(jsonData)) {
            continue;
        }

        for (const block of jsonData) {
            const type = block.type || 'UNKNOWN';
            if (!Array.isArray(block.items)) continue;

            for (const item of block.items) {
                const id = item.id || '';
                const name = item.name || '';
                const desc = item.desc || '';
                const requireStr = item.require || '';
                const tagStr = item.tag || item.tags || '';

                let match = false;

                if (mode === 'require') {
                    match =
                        typeof requireStr === 'string' &&
                        requireStr.toLowerCase().includes(termLower);
                } else {
                    const idMatch =
                        typeof id === 'string' && id.toLowerCase().includes(termLower);
                    const nameMatch =
                        typeof name === 'string' && name.toLowerCase().includes(termLower);
                    const descMatch =
                        typeof desc === 'string' && desc.toLowerCase().includes(termLower);
                    const requireMatch =
                        typeof requireStr === 'string' &&
                        requireStr.toLowerCase().includes(termLower);
                    const tagMatch =
                        typeof tagStr === 'string' &&
                        tagStr.toLowerCase().includes(termLower);
                    match = idMatch || nameMatch || descMatch || requireMatch || tagMatch;
                }

                if (!match) continue;

                hits++;
                console.log('----------------------------------------');
                console.log('File    :', file);
                console.log('Type    :', type);
                console.log('ID      :', id);
                console.log('Name    :', name);
                console.log('Desc    :', desc);
                console.log('Tag     :', tagStr);
                if (mode === 'require') {
                    console.log('Require :', requireStr);
                }
            }
        }
    }

    if (hits === 0) {
        console.log('No matches found for:', searchTerm);
    } else {
        console.log('----------------------------------------');
        console.log('Total matches:', hits, 'for search term:', `"${searchTerm}"`);
    }
}

// Electron-style folder picker, same pattern as church_data_autofix.js
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
            try {
                fs.unlinkSync(tempFile);
            } catch {
                // ignore
            }
            if (err) return resolve(null);
            const selected = stdout.toString().trim();
            resolve(selected || null);
        });
    });
}

main();
