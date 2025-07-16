const fs = require('fs');

function processJsonFilesMods(fileNames, id) {
    const targetSuffix = `${id}.max`;
    let currentResults = {};

    for (const fileName of fileNames) {
        const fileContent = fs.readFileSync("../Assets/data/" + fileName, 'utf-8');
        const jsonData = JSON.parse(fileContent);

        for (const entry of jsonData) {
            const type = entry.type;
            const items = entry.items;
            if (!items) continue;

            if (!currentResults[type]) {
                currentResults[type] = [];
            }

            for (const item of items) {
                if (!item.mod) continue;

                for (const key in item.mod) {
                    if (key === targetSuffix) {
                        currentResults[type].push({
                            id: item.id,
                            name: item.name,
                            value: item.mod[key]
                        });
                    }
                }
            }
        }
    }

    return currentResults;
}

function displayModResults(results, id) {
    for (const type in results) {
        
        if (results[type].length === 0) {
            continue;
        }
        console.log(type);
        results[type].sort((a, b) => b.value - a.value);
        for (const item of results[type]) {
            const namePart = item.name ? ` (${item.name})` : '';
            const label = (item.id + namePart).padEnd(50);
            console.log(`  - ${label}: ${item.value}`);
        }
        console.log();
    }
}

// Example usage
const fileNames = ['main_data_v15.json', 'main_data_v20.json', 'main_data_v25.json', 'main_data_v27.json'];
const targetModId = 'luxury'; // change to whatever mod you're tracking
const results = processJsonFilesMods(fileNames, targetModId);
displayModResults(results, targetModId);
