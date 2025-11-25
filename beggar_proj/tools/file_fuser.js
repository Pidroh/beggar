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

    // 2. Repeatedly ask user for source/target pair
    const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout,
    });

    function ask() {
        rl.question(
            'Enter source and target (e.g. "27_2 28", empty to quit): ',
            (answer) => {
                const trimmed = answer.trim();
                if (!trimmed) {
                    rl.close();
                    console.log('Exiting.');
                    return;
                }

                const parts = trimmed.split(/\s+/).filter(Boolean);
                if (parts.length !== 2) {
                    console.log('Please provide exactly two values, e.g. "27_2 28".');
                    ask();
                    return;
                }

                const [sourceSuffix, targetSuffix] = parts;
                try {
                    fuseFiles(folderPath, sourceSuffix, targetSuffix);
                } catch (err) {
                    console.error('Error while fusing files:', err.message);
                }
                ask();
            }
        );
    }

    ask();
}

function fuseFiles(folderPath, sourceSuffix, targetSuffix) {
    if (sourceSuffix === targetSuffix) {
        console.log('Source and target are the same; nothing to do.');
        return;
    }

    const normalizedSource = sourceSuffix.startsWith('v')
        ? sourceSuffix
        : 'v' + sourceSuffix;
    const normalizedTarget = targetSuffix.startsWith('v')
        ? targetSuffix
        : 'v' + targetSuffix;

    const sourceFile = `main_data_${normalizedSource}.json`;
    const targetFile = `main_data_${normalizedTarget}.json`;

    const sourcePath = path.join(folderPath, sourceFile);
    const targetPath = path.join(folderPath, targetFile);

    if (!fs.existsSync(sourcePath)) {
        console.log('Source file not found:', sourceFile);
        return;
    }
    if (!fs.existsSync(targetPath)) {
        console.log('Target file not found:', targetFile);
        return;
    }

    let sourceDataRaw;
    let targetDataRaw;

    try {
        sourceDataRaw = JSON.parse(fs.readFileSync(sourcePath, 'utf8'));
    } catch (err) {
        console.error('Failed to parse source JSON:', sourceFile, err.message);
        return;
    }

    try {
        targetDataRaw = JSON.parse(fs.readFileSync(targetPath, 'utf8'));
    } catch (err) {
        console.error('Failed to parse target JSON:', targetFile, err.message);
        return;
    }

    if (!Array.isArray(sourceDataRaw)) {
        console.log('Source JSON is not an array:', sourceFile);
        return;
    }
    if (!Array.isArray(targetDataRaw)) {
        console.log('Target JSON is not an array:', targetFile);
        return;
    }

    // First, check for duplicate IDs between source and target.
    const duplicates = findDuplicateIds(targetDataRaw, sourceDataRaw);
    if (duplicates.length > 0) {
        console.log('Duplicate IDs found between source and target. Aborting without changes.');
        for (const dup of duplicates) {
            console.log(`  Type: ${dup.type}, ID: ${dup.id}`);
        }
        return;
    }

    // Merge by type and id, so we don't end up with
    // multiple RESOURCE/TASK/etc blocks.
    const merged = mergeBlocks(targetDataRaw, sourceDataRaw);

    const originalBlockCount = targetDataRaw.length;
    const originalItemCount = countItems(targetDataRaw);
    const newBlockCount = merged.length;
    const newItemCount = countItems(merged);

    // Optional backup
    const backupPath = `${targetPath}.bak`;
    try {
        fs.copyFileSync(targetPath, backupPath);
        console.log('Backup created:', path.basename(backupPath));
    } catch (err) {
        console.warn('Could not create backup of target file:', err.message);
    }

    fs.writeFileSync(targetPath, JSON.stringify(merged, null, 4));

    console.log('----------------------------------------');
    console.log('Source file :', sourceFile);
    console.log('Target file :', targetFile);
    console.log('Blocks      :', originalBlockCount, '->', newBlockCount);
    console.log('Items       :', originalItemCount, '->', newItemCount);
    console.log('Fusion complete.');
}

// Merge arrays of blocks from target and source.
// - Blocks are grouped by "type"
// - Within each type, items are merged by "id"
function mergeBlocks(targetBlocks, sourceBlocks) {
    // Process target first, then source so source wins on conflicts.
    const all = [...targetBlocks, ...sourceBlocks];
    const typeMap = new Map();

    for (const block of all) {
        if (!block || typeof block !== 'object') continue;
        const type = block.type || 'UNKNOWN';

        if (!typeMap.has(type)) {
            const clone = {};
            for (const key of Object.keys(block)) {
                if (key !== 'items') {
                    clone[key] = block[key];
                }
            }
            clone.type = type;
            clone.items = [];
            clone._idIndex = new Map();
            typeMap.set(type, clone);
        }

        const entry = typeMap.get(type);
        const items = Array.isArray(block.items) ? block.items : [];

        for (const item of items) {
            const id = item && item.id;
            if (id && entry._idIndex.has(id)) {
                const idx = entry._idIndex.get(id);
                entry.items[idx] = item;
            } else {
                if (id) {
                    entry._idIndex.set(id, entry.items.length);
                }
                entry.items.push(item);
            }
        }
    }

    const result = [];
    for (const entry of typeMap.values()) {
        const { _idIndex, ...clean } = entry;
        result.push(clean);
    }
    return result;
}

function countItems(blocks) {
    let total = 0;
    for (const block of blocks) {
        if (Array.isArray(block.items)) {
            total += block.items.length;
        }
    }
    return total;
}

// Find duplicate IDs between target and source, grouped by type.
// Returns an array of { type, id }.
function findDuplicateIds(targetBlocks, sourceBlocks) {
    const typeToIds = new Map();

    // Record all ids in target by type
    for (const block of targetBlocks) {
        if (!block || typeof block !== 'object') continue;
        const type = block.type || 'UNKNOWN';
        if (!typeToIds.has(type)) {
            typeToIds.set(type, new Set());
        }
        const idSet = typeToIds.get(type);
        const items = Array.isArray(block.items) ? block.items : [];
        for (const item of items) {
            if (item && typeof item.id === 'string') {
                idSet.add(item.id);
            }
        }
    }

    const duplicates = [];
    const seen = new Set(); // avoid listing the same (type,id) twice

    // Check each source id against the target sets
    for (const block of sourceBlocks) {
        if (!block || typeof block !== 'object') continue;
        const type = block.type || 'UNKNOWN';
        const idSet = typeToIds.get(type);
        if (!idSet) continue;

        const items = Array.isArray(block.items) ? block.items : [];
        for (const item of items) {
            if (item && typeof item.id === 'string') {
                const key = `${type}::${item.id}`;
                if (idSet.has(item.id) && !seen.has(key)) {
                    seen.add(key);
                    duplicates.push({ type, id: item.id });
                }
            }
        }
    }

    return duplicates;
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
