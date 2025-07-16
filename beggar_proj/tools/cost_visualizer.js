const fs = require('fs');

function ProcessJsonCosts(jsonFileContent, currentResults, costKey) {
    for (const entry of jsonFileContent) {
        const type = entry.type;
        const items = entry.items;
        if (!items) continue;

        if (!currentResults[type]) {
            currentResults[type] = [];
        }

        for (const item of items) {
            if (item.cost && item.cost[costKey] !== undefined) {
                currentResults[type].push({
                    id: item.id,
                    cost: item.cost[costKey]
                });
            }
            if (item.buy && item.buy[costKey] !== undefined) {
                currentResults[type].push({
                    id: item.id,
                    cost: item.buy[costKey]
                });
            }
        }
    }
    return currentResults;
}

function processJsonFilesCosts(fileNames, costKey) {
    let currentResults = {};
    for (const fileName of fileNames) {
        const fileContent = fs.readFileSync("../Assets/data/" + fileName, 'utf-8');
        const jsonData = JSON.parse(fileContent);
        currentResults = ProcessJsonCosts(jsonData, currentResults, costKey);
    }
    return currentResults;
}

function DisplayCostResults(results, costKey) {
    for (const type in results) {
        
        if (results[type].length === 0) {
            continue;
        }
        console.log(type);
        results[type].sort((a, b) => b.cost - a.cost);
        for (const item of results[type]) {
            const namePart = item.name ? ` (${item.name})` : '';
            const label = (item.id + namePart).padEnd(30);
            console.log(`  - ${label}: ${item.cost}`);
        }
        console.log();
    }
}


// Usage example
const fileNames = ['main_data_v15.json', 'main_data_v20.json', 'main_data_v25.json', 'main_data_v27.json'];
const costKey = 'coin'; // or 'luxury', etc
const results = processJsonFilesCosts(fileNames, costKey);
DisplayCostResults(results, costKey);
