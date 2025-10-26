const fs = require('fs');
const path = require('path');

const CONFIG_PATH = './config.json';

function ProcessJsonTags(jsonFileContent, currentResults) {
    for (const entry of jsonFileContent) {
        const type = entry.type;
        const items = entry.items;
        if (!items) continue;

        for (const item of items) {
            if (item.tag) {
                const tag = item.tag;

                if (!currentResults[tag]) {
                    currentResults[tag] = {
                        RESOURCE: 0,
                        TASK: 0,
                        SKILL: 0
                    };
                }

                if (type === 'RESOURCE' || type === 'TASK' || type === 'SKILL') {
                    currentResults[tag][type]++;
                }
            }
        }
    }
    return currentResults;
}

function DisplayTagResults(results) {
    const tags = Object.keys(results).sort();

    console.log('Tag Analysis');
    console.log('='.repeat(80));
    console.log();

    for (const tag of tags) {
        const counts = results[tag];
        const total = counts.RESOURCE + counts.TASK + counts.SKILL;

        console.log(`Tag: ${tag}`);
        console.log(`  Resources: ${counts.RESOURCE}`);
        console.log(`  Tasks:     ${counts.TASK}`);
        console.log(`  Skills:    ${counts.SKILL}`);
        console.log(`  Total:     ${total}`);
        console.log();
    }

    console.log('='.repeat(80));
    console.log(`Total unique tags: ${tags.length}`);
}

function main() {
    // Load folder from config
    let folderPath;
    if (fs.existsSync(CONFIG_PATH)) {
        const config = JSON.parse(fs.readFileSync(CONFIG_PATH, 'utf8'));
        folderPath = config.folderPath;
    } else {
        console.error('Config file not found. Please run church_data_autofix.js first or create config.json');
        return;
    }

    // Process matching JSON files
    const files = fs.readdirSync(folderPath).filter(f => f.startsWith('main_data_') && f.endsWith('.json'));

    let currentResults = {};
    for (const file of files) {
        const fullPath = path.join(folderPath, file);
        const jsonData = JSON.parse(fs.readFileSync(fullPath, 'utf8'));
        currentResults = ProcessJsonTags(jsonData, currentResults);
    }

    DisplayTagResults(currentResults);
}

main();
