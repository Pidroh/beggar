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

function extractRequirementDetails(requireString, targetId) {
    if (!requireString) return null;
    
    // Check for different patterns and normalize to minimum required value
    const patterns = [
        { regex: new RegExp(`${targetId}>=([0-9]+)`), adjust: 0 },  // >= stays the same
        { regex: new RegExp(`${targetId}>([0-9]+)`), adjust: 1 },   // > needs +1
        { regex: new RegExp(`${targetId}==([0-9]+)`), adjust: 0 },  // == stays the same
        { regex: new RegExp(`${targetId}<=([0-9]+)`), adjust: 0 },  // <= stays the same (max requirement)
        { regex: new RegExp(`${targetId}<([0-9]+)`), adjust: -1 }   // < needs -1 (max requirement)
    ];
    
    for (const pattern of patterns) {
        const match = requireString.match(pattern.regex);
        if (match) {
            return parseInt(match[1]) + pattern.adjust;
        }
    }
    
    // Check if just the id is mentioned (implicitly means >0, so required is 1)
    if (requireString.includes(targetId)) {
        return 1;
    }
    
    return null;
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
                    const requiredValue = extractRequirementDetails(otherItem.require, id);
                    if (requiredValue !== null) {
                        idCount[id].push({
                            id: otherItem.id,
                            requiredValue: requiredValue
                        });
                    }
                }
            }
        }
    }

    // Append the count to each item in currentResults
    for (const type in currentResults) {
        for (const item of currentResults[type]) {
            item.requireCount = idCount[item.id] || [];
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
            if (item.requireCount.length > 0) {
                // Sort requirements by requiredValue
                item.requireCount.sort((a, b) => a.requiredValue - b.requiredValue);
                
                console.log(` ${item.id}: ${item.requireCount.length} requirements`);
                for (const req of item.requireCount) {
                    console.log(`   ${req.requiredValue}: ${req.id}`);
                }
                console.log('');
            }
        }
    }
}

// Example usage: ../Assets/data/
const fileNames = ['main_data_v15.json', 'main_data_v20.json', 'main_data_v25.json', 'main_data_v27.json', 'main_data_v28.json'];
const finalResults = processJsonFiles(fileNames);
DisplayResults(finalResults);
//console.log(finalResults);