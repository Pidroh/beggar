const fs = require('fs');

function ProcessJson(jsonFileContent, currentResults) {
    for (const entry of jsonFileContent) {
        const type = entry.type;
        const items = entry.items;

        // if (!items) continue;
        if (items === undefined) continue;

        if (!currentResults[type]) {
            currentResults[type] = [];
        }

        for (const item of items) {
            currentResults[type].push({
                id: item.id,
                require: item.require+item.need || null
            });
        }
    }

    return currentResults;
}

function PostProcessResults(currentResults) {
    const idCount = {};

    // Count occurrences of each id in requires across all types
    for (const type in currentResults) {
        for (const item of currentResults[type]) {
            const id = item.id;
            if (!idCount[id]) idCount[id] = [];

            for (const otherType in currentResults) {
                for (const otherItem of currentResults[otherType]) {
                    if (otherItem.require && otherItem.require.includes(id)) {
                        idCount[id].push(otherItem.id);
                    }
                }
            }
        }
    }

    // Append the count to each item in currentResults
    for (const type in currentResults) {
        for (const item of currentResults[type]) {
            item.requireCount = idCount[item.id] || 0;
        }
    }

    return currentResults;
}

function processJsonFiles(fileNames) {
    let currentResults = {};
    for (const fileName of fileNames) {
        const fileContent = fs.readFileSync("../Assets/data/"+fileName, 'utf-8');
        // console.log(fileContent);
        const jsonData = JSON.parse(fileContent);
        currentResults = ProcessJson(jsonData, currentResults);
    }
    // console.log(currentResults);
    currentResults = PostProcessResults(currentResults);
    return currentResults;
}

function DisplayResults(currentResults) {
    for (const type in currentResults) {
        if (type != "SKILL") continue;
        console.log(type);
        for (const item of currentResults[type]) {
            console.log(` ${item.id}: ${item.requireCount.length} \n   ${item.requireCount}\n`);
        }
    }
}

// Example usage: ../Assets/data/
const fileNames = ['main_data_v15.json', 'main_data_v20.json'];
const finalResults = processJsonFiles(fileNames);
DisplayResults(finalResults);
//console.log(finalResults);