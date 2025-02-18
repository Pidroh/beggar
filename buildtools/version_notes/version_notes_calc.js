const fs = require('fs');
const path = require('path');

const folderPath = 'jsons_data';

function extractVersion(filename) {
    const match = filename.match(/_v(\d+)/);
    return match ? parseInt(match[1], 10) : null;
}

function getFilesSortedByVersion() {
    return fs.readdirSync(folderPath)
        .filter(file => file.endsWith('.json'))
        .map(file => ({ file, version: extractVersion(file) }))
        .filter(item => item.version !== null)
        .sort((a, b) => a.version - b.version);
}

function getIdMap(jsonData) {
    const idMap = {};
    jsonData.forEach(obj => {
        if (!obj.items || !obj.type) return;
        if (!idMap[obj.type]) idMap[obj.type] = new Set();
        obj.items.forEach(item => idMap[obj.type].add(item.id));
    });
    return idMap;
}

function compareVersions(prevData, currData, currVersion) {
    const prevIds = getIdMap(prevData);
    const currIds = getIdMap(currData);

    console.log(`Version ${currVersion}`);
    Object.keys(currIds).forEach(type => {
        const prevSet = prevIds[type] || new Set();
        const currSet = currIds[type];
        const newIds = [...currSet].filter(id => !prevSet.has(id));
        //console.log(newIds);
        console.log(`${newIds.length} new ${type.toLowerCase()}(s)`);
    });
    console.log('');
}

function main() {
    const files = getFilesSortedByVersion();
    if (files.length < 2) return console.log('Not enough versions to compare.');

    let prevData = JSON.parse(fs.readFileSync(path.join(folderPath, files[0].file), 'utf8'));

    for (let i = 1; i < files.length; i++) {
        const currData = JSON.parse(fs.readFileSync(path.join(folderPath, files[i].file), 'utf8'));
        compareVersions(prevData, currData, files[i].version);
        prevData = currData;
    }
}

main();
