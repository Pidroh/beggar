const fs = require('fs');
const path = require('path');
const { exec } = require('child_process');

const CONFIG_PATH = './config.json';
const SEED_PATH = path.join(__dirname, 'prestige_resource_gain_seed_data.json');

async function main() {
    // 1. Load or ask for folder
    const folderPath = await getFolderPath();
    if (!folderPath) {
        console.error('No data folder selected.');
        return;
    }

    // 2. Load seed data
    if (!fs.existsSync(SEED_PATH)) {
        console.error(`Seed file not found: ${SEED_PATH}`);
        return;
    }

    const seed = JSON.parse(fs.readFileSync(SEED_PATH, 'utf8'));
    const fileName = seed.file_name;
    const taskIdPrefix = seed.task_id_prefix || seed.taskIdPrefix || '';
    const taskNamePrefix = seed.tas_name_prefix || seed.task_name_prefix || seed.taskNamePrefix || '';
    const desc = seed.desc || '';
    const needId = seed.need_id;
    const needValues = Array.isArray(seed.need_values) ? seed.need_values : [];
    const gainId = seed.gain_id;
    const gainValues = Array.isArray(seed.gain_values) ? seed.gain_values : [];

    if (!fileName || !taskIdPrefix || !taskNamePrefix || !needId || !gainId) {
        console.error('Seed data missing required fields.');
        return;
    }

    const count = Math.min(needValues.length, gainValues.length);
    if (count === 0) {
        console.error('Seed arrays are empty or mismatched.');
        return;
    }

    const targetPath = path.join(folderPath, fileName);
    if (!fs.existsSync(targetPath)) {
        console.error(`Target data file not found: ${targetPath}`);
        return;
    }

    // 3. Load target JSON
    const jsonData = JSON.parse(fs.readFileSync(targetPath, 'utf8'));

    // Find or create TASK section
    let taskSection = jsonData.find(block => block.type === 'TASK');
    if (!taskSection) {
        taskSection = { type: 'TASK', items: [] };
        jsonData.push(taskSection);
    }
    if (!Array.isArray(taskSection.items)) {
        taskSection.items = [];
    }

    // 4. Generate / update tasks
    let modified = false;

    for (let i = 0; i < count; i++) {
        const id = `${taskIdPrefix}${i + 1}`;
        const needValue = needValues[i];
        const gainValue = gainValues[i];

        const newTask = {
            id,
            name: `${taskNamePrefix} ${i + 1}`,
            desc: desc,
            result: {
                [gainId]: gainValue
            },
            duration: 5,
            need: `${needId}>=${needValue}`,
            max: 1
        };

        const existingIndex = taskSection.items.findIndex(item => item.id === id);
        if (existingIndex >= 0) {
            // Update existing task on ID conflict
            taskSection.items[existingIndex] = newTask;
            console.log(`Updated existing task: ${id}`);
        } else {
            taskSection.items.push(newTask);
            console.log(`Added new task: ${id}`);
        }
        modified = true;
    }

    // 5. Save if modified
    if (modified) {
        fs.writeFileSync(targetPath, JSON.stringify(jsonData, null, 4));
        console.log(`Saved changes to ${targetPath}`);
    } else {
        console.log('No changes made.');
    }
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

// Electron helper (same pattern as church_data_autofix.js)
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

