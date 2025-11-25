const fs = require('fs');
const path = require('path');

const CONFIG_PATH = './config.json';
const ENDING_TAG = 'ending_game';
const ENDING_CSHARP_PATH = path.join('..', 'Assets', 'scripts', 'game', 'JGameControlExecuterEnding.cs');

function loadFolderPath() {
    if (!fs.existsSync(CONFIG_PATH)) {
        throw new Error('config.json not found. Run another tool first to create it, or add folderPath.');
    }
    const config = JSON.parse(fs.readFileSync(CONFIG_PATH, 'utf8'));
    if (!config.folderPath) {
        throw new Error('folderPath missing in config.json.');
    }
    return config.folderPath;
}

function findEndingIds(folderPath) {
    const files = fs
        .readdirSync(folderPath)
        .filter(f => f.startsWith('main_data_') && f.endsWith('.json'));

    const endingMap = Object.create(null); // id -> { files: Set, entries: [{ file, item }] }

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
            if (!block || !Array.isArray(block.items)) continue;
            for (const item of block.items) {
                if (!item || typeof item !== 'object') continue;
                const id = item.id;
                if (typeof id !== 'string' || !id.trim()) continue;

                const tag = item.tag || item.tags || '';
                if (typeof tag !== 'string' || !tag.trim()) continue;

                const tags = tag.split(',').map(t => t.trim());
                if (tags.includes(ENDING_TAG)) {
                    if (!endingMap[id]) {
                        endingMap[id] = {
                            files: new Set(),
                            entries: []
                        };
                    }
                    endingMap[id].files.add(file);
                    endingMap[id].entries.push({ file, item });
                }
            }
        }
    }

    const ids = Object.keys(endingMap).sort();
    const sources = {};
    const entries = {};
    for (const id of ids) {
        sources[id] = Array.from(endingMap[id].files).sort();
        entries[id] = endingMap[id].entries.slice();
    }
    return { ids, sources, entries };
}

function parseCSharpEndings() {
    if (!fs.existsSync(ENDING_CSHARP_PATH)) {
        throw new Error(`C# file not found at ${ENDING_CSHARP_PATH}`);
    }
    const text = fs.readFileSync(ENDING_CSHARP_PATH, 'utf8');

    const countMatch = text.match(/public\s+const\s+int\s+ENDING_COUNT\s*=\s*(\d+)\s*;/);
    if (!countMatch) {
        throw new Error('Could not find ENDING_COUNT in C# file.');
    }
    const endingCount = parseInt(countMatch[1], 10);

    const idsMatch = text.match(/endingUnitIds\s*=\s*new\s+string\[[^\]]*]\s*{\s*([^}]*)}/);
    if (!idsMatch) {
        throw new Error('Could not find endingUnitIds array in C# file.');
    }

    const idsBody = idsMatch[1];
    const idRegex = /"([^"]+)"/g;
    const ids = [];
    let m;
    while ((m = idRegex.exec(idsBody)) !== null) {
        ids.push(m[1]);
    }

    return { endingCount, ids };
}

function compareEndings(jsonIds, jsonSources, jsonEntries, csInfo) {
    const { endingCount, ids: csharpIds } = csInfo;

    const jsonSet = new Set(jsonIds);
    const csSet = new Set(csharpIds);

    const onlyInJson = jsonIds.filter(id => !csSet.has(id));
    const onlyInCs = csharpIds.filter(id => !jsonSet.has(id));

    let ok = true;

    console.log('--- Ending verifier ---');
    console.log('JSON endings count  :', jsonIds.length);
    console.log('C# ENDING_COUNT     :', endingCount);
    console.log('C# endingUnitIds len:', csharpIds.length);

    if (endingCount !== jsonIds.length) {
        ok = false;
        console.log('MISMATCH: ENDING_COUNT does not match number of endings in JSON.');
    }

    if (csharpIds.length !== endingCount) {
        ok = false;
        console.log('MISMATCH: endingUnitIds length does not match ENDING_COUNT.');
    }

    if (onlyInJson.length > 0) {
        ok = false;
        console.log('IDs present in JSON endings but missing in endingUnitIds:');
        for (const id of onlyInJson) {
            const files = jsonSources[id] || [];
            if (files.length > 0) {
                console.log('  ', id, ' (from:', files.join(', '), ')');
            } else {
                console.log('  ', id);
            }

            const entries = jsonEntries[id] || [];
            for (const entry of entries) {
                console.log('    From file:', entry.file);
                try {
                    console.log(JSON.stringify(entry.item, null, 4));
                } catch {
                    console.log('      (Could not stringify entry)');
                }
            }
        }
    }

    if (onlyInCs.length > 0) {
        ok = false;
        console.log('IDs present in endingUnitIds but not tagged as endings in JSON:');
        for (const id of onlyInCs) {
            console.log('  ', id);
        }
    }

    if (ok) {
        console.log('All good: ENDING_COUNT and endingUnitIds match JSON endings (tag ending_game).');
    }
}

function main() {
    try {
        const folderPath = loadFolderPath();
        const { ids: jsonIds, sources: jsonSources, entries: jsonEntries } = findEndingIds(folderPath);
        const csInfo = parseCSharpEndings();
        compareEndings(jsonIds, jsonSources, jsonEntries, csInfo);
    } catch (err) {
        console.error('Error in ending_verifier:', err.message);
    }
}

main();
