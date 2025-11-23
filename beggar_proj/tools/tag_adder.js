const fs = require('fs');
const path = require('path');
const readline = require('readline');
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

    // 2. Repeatedly ask user for ID
    const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout,
    });

    function askForId() {
        rl.question('Enter ID to tag (empty to quit): ', (answer) => {
            const rawId = answer.trim();
            if (!rawId) {
                rl.close();
                console.log('Exiting.');
                return;
            }

            const { matches, suggestions } = findItemsById(folderPath, rawId);

            if (suggestions.length > 0) {
                console.log('IDs containing that text:');
                for (const s of suggestions) {
                    console.log(`  ${s.id}  (file: ${s.file}, type: ${s.type || 'UNKNOWN'})`);
                }
                console.log('---');
            }

            if (matches.length === 0) {
                console.log(`No exact match found for ID "${rawId}".`);
                if (suggestions.length === 0) {
                    console.log('No IDs containing that text were found.');
                    console.log('---');
                }
                askForId();
                return;
            }

            console.log(`Found ${matches.length} item(s) with ID "${rawId}":`);
            for (const match of matches) {
                console.log('----------------------------------------');
                console.log('File :', match.file);
                console.log('Type :', match.type || 'UNKNOWN');
                console.log('ID   :', match.item.id || '');
                console.log('Name :', match.item.name || '');
                console.log('Desc :', match.item.desc || '');
                const tagStr = match.item.tag || match.item.tags || '';
                console.log('Tag  :', tagStr);
            }
            console.log('----------------------------------------');

            askForTag(rl, matches, rawId, askForId);
        });
    }

    askForId();
}

function findItemsById(folderPath, idInput) {
    const targetId = idInput.trim();
    const lowerId = targetId.toLowerCase();

    const files = fs
        .readdirSync(folderPath)
        .filter(f => f.startsWith('main_data_') && f.endsWith('.json'));

    if (files.length === 0) {
        console.log('No main_data_*.json files found in folder:', folderPath);
        return { matches: [], suggestions: [] };
    }

    const matches = [];
    const suggestionsMap = new Map(); // id -> { id, file, type }

    for (const file of files) {
        const fullPath = path.join(folderPath, file);
        let jsonData;
        try {
            jsonData = JSON.parse(fs.readFileSync(fullPath, 'utf8'));
        } catch (err) {
            console.error('Failed to parse JSON for file:', file, err.message);
            continue;
        }

        if (!Array.isArray(jsonData)) continue;

        for (const block of jsonData) {
            const type = block.type || 'UNKNOWN';
            if (!Array.isArray(block.items)) continue;

            for (const item of block.items) {
                const itemId = typeof item.id === 'string' ? item.id : '';
                if (!itemId) continue;

                if (itemId === targetId) {
                    matches.push({ file, fullPath, type, item, jsonData });
                } else if (itemId.toLowerCase().includes(lowerId)) {
                    if (!suggestionsMap.has(itemId)) {
                        suggestionsMap.set(itemId, { id: itemId, file, type });
                    }
                }
            }
        }
    }

    const suggestions = Array.from(suggestionsMap.values()).sort((a, b) =>
        a.id.localeCompare(b.id)
    );

    return { matches, suggestions };
}

function askForTag(rl, matches, idInput, doneCallback) {
    rl.question(`Enter tag to add to ID "${idInput}" (empty to cancel): `, (tagAnswer) => {
        const tag = tagAnswer.trim();
        if (!tag) {
            console.log('No tag entered. No changes made.');
            console.log('---');
            doneCallback();
            return;
        }

        const tagLower = tag.toLowerCase();
        let updatedItems = 0;
        const filesToWrite = new Map(); // fullPath -> { jsonData, file }

        for (const match of matches) {
            const { file, fullPath, item, jsonData } = match;

            let fieldName = 'tag';
            let existing = '';
            if (typeof item.tag === 'string') {
                fieldName = 'tag';
                existing = item.tag;
            } else if (typeof item.tags === 'string') {
                fieldName = 'tags';
                existing = item.tags;
            }

            const existingTags = existing
                ? existing
                    .split(',')
                    .map(t => t.trim())
                    .filter(t => t.length > 0)
                : [];

            const alreadyHas = existingTags.some(t => t.toLowerCase() === tagLower);
            if (alreadyHas) {
                console.log(`File ${file}: tag "${tag}" already present, skipping.`);
                continue;
            }

            existingTags.push(tag);
            const newTagString = existingTags.join(',');

            if (fieldName === 'tag') {
                item.tag = newTagString;
            } else {
                item.tags = newTagString;
            }

            updatedItems++;
            filesToWrite.set(fullPath, { jsonData, file });
        }

        for (const [fullPath, info] of filesToWrite.entries()) {
            fs.writeFileSync(fullPath, JSON.stringify(info.jsonData, null, 4));
            console.log(`Updated file: ${info.file}`);
        }

        if (updatedItems === 0) {
            console.log('No items were updated.');
        } else {
            console.log(`Total items updated: ${updatedItems}`);
        }
        console.log('---');
        doneCallback();
    });
}

// Electron-style folder picker, same pattern as main_data_search.js
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
