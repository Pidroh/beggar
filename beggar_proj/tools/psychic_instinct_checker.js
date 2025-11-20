const fs = require('fs');
const path = require('path');
const { exec } = require('child_process');

const CONFIG_PATH = './config.json';
const TARGET_FILE = 'main_data_prestige.json';

async function main() {
    const folderPath = await getFolderPath();
    if (!folderPath) {
        console.error('No data folder selected.');
        return;
    }

    const targetPath = path.join(folderPath, TARGET_FILE);
    if (!fs.existsSync(targetPath)) {
        console.error(`Target file not found: ${targetPath}`);
        return;
    }

    const jsonData = JSON.parse(fs.readFileSync(targetPath, 'utf8'));

    const taskSection = jsonData.find(block => block.type === 'TASK');
    if (!taskSection || !Array.isArray(taskSection.items)) {
        console.log('No TASK section found.');
        return;
    }

    let totalGain = 0;
    let totalCost = 0;
    let maxPsychicNeeded = 0;

    for (const task of taskSection.items) {
        // 1. How much psychic_instinct you can generate in total
        const maxRuns = typeof task.max === 'number' && task.max > 0 ? task.max : 1;
        if (task.result && typeof task.result === 'object' && task.result !== null) {
            const gain = Number(task.result.psychic_instinct || 0);
            if (!Number.isNaN(gain)) {
                totalGain += gain * maxRuns;
            }
        }

        // 2. How much psychic_instinct can be spent in total
        if (task.cost && typeof task.cost === 'object' && task.cost !== null) {
            const cost = Number(task.cost.psychic_instinct || 0);
            if (!Number.isNaN(cost)) {
                totalCost += cost * maxRuns;
            }
        }

        // 3. Threshold-style psychic_instinct "need" in requirements (if any)
        // We interpret "need" as a threshold, so we track the maximum
        if (typeof task.need === 'string') {
            const needValue = parsePsychicNeed(task.need);
            if (needValue !== null && needValue > maxPsychicNeeded) {
                maxPsychicNeeded = needValue;
            }
        }
    }

    console.log('=== Psychic Instinct Summary ===');
    console.log(`Total psychic_instinct you can generate (with max runs): ${totalGain}`);
    console.log(`Total psychic_instinct you can spend (all costs * max):  ${totalCost}`);
    console.log(`Maximum psychic_instinct required by any task need:      ${maxPsychicNeeded}`);
}

function parsePsychicNeed(needStr) {
    // Examples handled:
    // psychic_instinct>=100
    // psychic_instinct >= 50
    // psychic_instinct>20
    // psychic_instinct==30
    const regex = /psychic_instinct\s*(?:>=|>|==|=)\s*([0-9]+(?:\.[0-9]+)?)/i;
    const match = needStr.match(regex);
    if (!match) return null;
    const value = Number(match[1]);
    return Number.isNaN(value) ? null : value;
}

async function getFolderPath() {
    if (fs.existsSync(CONFIG_PATH)) {
        try {
            const config = JSON.parse(fs.readFileSync(CONFIG_PATH, 'utf8'));
            if (config.folderPath) {
                return config.folderPath;
            }
        } catch (e) {
            console.error('Failed to read config.json, will ask for folder.');
        }
    }
    const folderPath = await askForFolder();
    if (folderPath) {
        fs.writeFileSync(CONFIG_PATH, JSON.stringify({ folderPath }, null, 2));
    }
    return folderPath;
}

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
            fs.unlinkSync(tempFile);
            if (err) return resolve(null);
            const selected = stdout.toString().trim();
            resolve(selected || null);
        });
    });
}

main();
