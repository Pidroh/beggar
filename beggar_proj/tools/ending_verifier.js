const fs = require('fs');
const path = require('path');
const readline = require('readline');

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

            const type = (block.type || '').toString().toUpperCase();
            if (type === 'DIALOG') {
                // Dialog entries can also use ending_game tag; ignore for endings list.
                continue;
            }

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

    const idsMatch = text.match(/endingUnitIds\s*=\s*new\s+string\[[^\]]*]\s*{\s*([\s\S]*?)}/);
    if (!idsMatch) {
        throw new Error('Could not find endingUnitIds array in C# file.');
    }

    const idsBody = idsMatch[1];
    const strRegex = /"([^"]+)"/g;
    const ids = [];
    let m;
    while ((m = strRegex.exec(idsBody)) !== null) {
        ids.push(m[1]);
    }

    const prefixMatch = text.match(/endingPrefix\s*=\s*new\s+string\[[^\]]*]\s*{\s*([\s\S]*?)}/);
    const prefixes = [];
    if (prefixMatch) {
        const body = prefixMatch[1];
        let pm;
        while ((pm = strRegex.exec(body)) !== null) {
            prefixes.push(pm[1]);
        }
    }

    const snippetMatch = text.match(/endingMessageSnippet\s*=\s*new\s+string\[[^\]]*]\s*{\s*([\s\S]*?)}/);
    const snippets = [];
    if (snippetMatch) {
        const body = snippetMatch[1];
        let sm;
        while ((sm = strRegex.exec(body)) !== null) {
            snippets.push(sm[1]);
        }
    }

    return { endingCount, ids, prefixes, snippets };
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

    return { ok, onlyInJson, onlyInCs };
}

function updateCSharpAddEnding(id, prefix, snippet) {
    if (!fs.existsSync(ENDING_CSHARP_PATH)) {
        throw new Error(`C# file not found at ${ENDING_CSHARP_PATH}`);
    }
    let text = fs.readFileSync(ENDING_CSHARP_PATH, 'utf8');

    const info = parseCSharpEndings();
    const newCount = info.endingCount + 1;

    // Update ENDING_COUNT
    text = text.replace(
        /public\s+const\s+int\s+ENDING_COUNT\s*=\s*\d+\s*;/,
        `public const int ENDING_COUNT = ${newCount};`
    );

    // Helper to append a string entry to a string[] initializer
    function appendStringToArray(code, arrayName, value) {
        const re = new RegExp(
            arrayName + '\\s*=\\s*new\\s+string\\[[^\\]]*]\\s*{([\\s\\S]*?)}'
        );
        const match = re.exec(code);
        if (!match) return code;
        const body = match[1];
        const trimmed = body.replace(/\s+$/, '');
        const needsComma = trimmed.trim().length > 0;
        const indentMatch = body.match(/(\s*)$/);
        const indent = indentMatch ? indentMatch[1] : ' ';
        const addition = (needsComma ? ',' : '') + ` "${value}"`;
        const newBody = trimmed + addition + indent;
        return code.replace(re, `${arrayName} = new string[ENDING_COUNT] {${newBody}}`);
    }

    // endingUnitIds, endingPrefix, endingMessageSnippet
    text = appendStringToArray(text, 'endingUnitIds', id);
    text = appendStringToArray(text, 'endingPrefix', prefix);
    text = appendStringToArray(text, 'endingMessageSnippet', snippet);

    // Update JEndingGameData.runtimeUnits initializer: add one more null
    const ruRe = /runtimeUnits\s*=\s*new\s+RuntimeUnit\[[^\]]*]\s*{([\s\S]*?)}/;
    const ruMatch = ruRe.exec(text);
    if (ruMatch) {
        const body = ruMatch[1];
        const trimmed = body.replace(/\s+$/, '');
        const needsComma = trimmed.trim().length > 0;
        const indentMatch = body.match(/(\s*)$/);
        const indent = indentMatch ? indentMatch[1] : ' ';
        const addition = (needsComma ? ',' : '') + ' null';
        const newBody = trimmed + addition + indent;
        text = text.replace(ruRe, `runtimeUnits = new RuntimeUnit[JGameControlExecuterEnding.ENDING_COUNT] {${newBody}}`);
    }

    fs.writeFileSync(ENDING_CSHARP_PATH, text, 'utf8');
    console.log(`Added ending "${id}" to JGameControlExecuterEnding.cs (new count: ${newCount}).`);
}

async function promptAddMissingEndings(missingIds, jsonEntries) {
    if (!missingIds || missingIds.length === 0) return;

    const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout,
    });

    const ask = (q) =>
        new Promise((resolve) => rl.question(q, (answer) => resolve(answer.trim())));

    console.log('\n--- Interactive ending adder ---');

    for (const id of missingIds) {
        const answer = (await ask(`Add missing ending "${id}" to JGameControlExecuterEnding? (y/N): `)).toLowerCase();
        if (answer !== 'y' && answer !== 'yes') {
            continue;
        }

        const info = parseCSharpEndings();
        console.log('\nJSON entry for this ending (for context):');
        const entries = jsonEntries[id] || [];
        if (entries.length > 0) {
            console.log(JSON.stringify(entries[0].item, null, 4));
        } else {
            console.log('  (No JSON entry details available)');
        }

        const examplePrefix = info.prefixes[0] || '';
        if (info.prefixes.length > 0) {
            console.log('\nExample prefixes:');
            info.prefixes.forEach((p, i) => console.log(`  [${i}] ${p}`));
        }
        const prefixPrompt = examplePrefix
            ? `\nEnter prefix text for this ending (e.g. "${examplePrefix}"): `
            : '\nEnter prefix text for this ending: ';
        const prefix = await ask(prefixPrompt);

        const exampleSnippet = info.snippets[0] || '';
        if (info.snippets.length > 0) {
            console.log('\nExample message snippets:');
            info.snippets.forEach((s, i) => console.log(`  [${i}] ${s}`));
        }
        const snippetPrompt = exampleSnippet
            ? 'Enter message snippet text for this ending (e.g. "' + exampleSnippet + '"): '
            : 'Enter message snippet text for this ending: ';
        const snippet = await ask(snippetPrompt);

        try {
            updateCSharpAddEnding(id, prefix, snippet);
        } catch (err) {
            console.error('Failed to update C# file for', id, ':', err.message);
        }
    }

    rl.close();
}

async function main() {
    try {
        const folderPath = loadFolderPath();
        const { ids: jsonIds, sources: jsonSources, entries: jsonEntries } = findEndingIds(folderPath);
        const csInfo = parseCSharpEndings();
        const result = compareEndings(jsonIds, jsonSources, jsonEntries, csInfo);

        if (!result.ok && result.onlyInJson.length > 0) {
            await promptAddMissingEndings(result.onlyInJson, jsonEntries);
        }
    } catch (err) {
        console.error('Error in ending_verifier:', err.message);
    }
}

main();
