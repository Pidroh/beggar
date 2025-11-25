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

    const sourceFile = `main_data_${sourceSuffix}.json`;
    const targetFile = `main_data_${targetSuffix}.json`;

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

    let sourceData;
    let targetData;

    try {
        sourceData = JSON.parse(fs.readFileSync(sourcePath, 'utf8'));
    } catch (err) {
        console.error('Failed to parse source JSON:', sourceFile, err.message);
        return;
    }

    try {
        targetData = JSON.parse(fs.readFileSync(targetPath, 'utf8'));
    } catch (err) {
        console.error('Failed to parse target JSON:', targetFile, err.message);
        return;
    }

    if (!Array.isArray(sourceData)) {
        console.log('Source JSON is not an array:', sourceFile);
        return;
    }
    if (!Array.isArray(targetData)) {
        console.log('Target JSON is not an array:', targetFile);
        return;
    }

    const originalTargetLength = targetData.length;
    targetData = targetData.concat(sourceData);

    // Optional backup
    const backupPath = `${targetPath}.bak`;
    try {
        fs.copyFileSync(targetPath, backupPath);
        console.log('Backup created:', path.basename(backupPath));
    } catch (err) {
        console.warn('Could not create backup of target file:', err.message);
    }

    fs.writeFileSync(targetPath, JSON.stringify(targetData, null, 4));

    console.log('----------------------------------------');
    console.log('Source file :', sourceFile);
    console.log('Target file :', targetFile);
    console.log('Target size :', originalTargetLength, '->', targetData.length);
    console.log('Fusion complete.');
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

