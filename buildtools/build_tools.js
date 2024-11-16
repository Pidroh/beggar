const fs = require('fs');
const path = require('path');

const dirPath = path.join(__dirname, '../builds/site');

const { execFileSync } = require('child_process');

function Execute(scriptPath) {
  try {
    const output = execFileSync('node', [scriptPath], { encoding: 'utf-8' });
    return output;
  } catch (error) {
    console.error(`Error: ${error.message}`);
    return null;
  }
}



const menuTemplatePath = path.join(__dirname, 'menu_template.html');
const outputMenuPath = path.join(dirPath, 'menu.html');

fs.readdir(dirPath, (err, items) => {
    if (err) throw err;

    // Find all directories with the "webNNNN" format
    const webDirs = items
    .filter(item => fs.statSync(path.join(dirPath, item)).isDirectory())
    .filter(dir => /^web\d{4}$/.test(dir))
    .sort((a, b) => b.slice(3).localeCompare(a.slice(3))); // String comparison on the numeric part


    // Read the menu template
    fs.readFile(menuTemplatePath, 'utf-8', (err, templateContent) => {
        if (err) throw err;

        // Create a button for each webNNNN directory
        const buttonsHTML = webDirs.map((dir, index) => {
            const versionNumber = dir.slice(3);
            
            // Set the first button to "Latest Version"
            const buttonText = `Version ${versionNumber}`;
        
            return `<button onclick="window.location.href='./${dir}/index.html'">${buttonText}</button>`;
        }).join('\n');

        // Replace %MENU% with the buttons in the template
        const finalContent = templateContent.replace('%MENU%', buttonsHTML);

        // Save the final content as menu.html
        fs.writeFile(outputMenuPath, finalContent, 'utf-8', (err) => {
            if (err) throw err;
            console.log('menu.html created/updated successfully.');
        });
    });
});

const latestPath = path.join(dirPath, 'latest');
const latestBetaPath = path.join(dirPath, 'latest_beta');

// Helper to filter and find the highest NNNN folders
const findHighest = (folders, pattern) => {
    var filtered = folders
    .filter(name => pattern.test(name));
    console.log(filtered);
  return filtered
    .map(name => parseInt(name.match(/\d+/)[0], 10))
    .sort((a, b) => b - a)[0];
};

// Get folders in dirPath
const folders = fs.readdirSync(dirPath).filter(folder => fs.lstatSync(path.join(dirPath, folder)).isDirectory());
console.log(folders);

const highestNumber = findHighest(folders, /^web\d{4}$/);
console.log(highestNumber);
const highestBetaNumber = findHighest(folders, /^web\d{4}_beta$/);

const highestFolder = highestNumber ? `web${highestNumber.toLocaleString('en-US', {minimumIntegerDigits:4, useGrouping: false})}` : null;
const highestBetaFolder = highestBetaNumber ? `web${highestNumber.toLocaleString('en-US', {minimumIntegerDigits:4, useGrouping: false})}_beta` : null;


if (highestFolder) {
    console.log(highestFolder);
    //if (!fs.existsSync(latestPath)) fs.mkdirSync(latestPath, { recursive: true });
  fs.rmSync(latestPath, { recursive: true, force: true });
  fs.cpSync(path.join(dirPath, highestFolder), latestPath, { recursive: true });
}

if (highestBetaFolder) {
  //  if (!fs.existsSync(latestBetaPath)) fs.mkdirSync(latestBetaPath, { recursive: true });
  fs.rmSync(latestBetaPath, { recursive: true, force: true });
  fs.cpSync(path.join(dirPath, highestBetaFolder), latestBetaPath, { recursive: true });
}

Execute("add_menu.js");
Execute("modifier_builds.js");