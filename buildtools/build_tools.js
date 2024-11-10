const fs = require('fs');
const path = require('path');

const dirPath = path.join(__dirname, '../builds/site');
const templatePath = path.join(__dirname, 'redirector_template.html');
const outputPath = path.join(dirPath, 'index.html');

fs.readdir(dirPath, (err, files) => {
    if (err) throw err;

    // Find the folder with the "webNNNN" format with the highest number
    const webFolders = files
        .filter(file => fs.statSync(path.join(dirPath, file)).isDirectory())
        .filter(folder => /^web\d{4}$/.test(folder))
        .sort((a, b) => parseInt(b.slice(3), 10) - parseInt(a.slice(3), 10));

    const highestFolder = webFolders[0];

    // Read the template file
    fs.readFile(templatePath, 'utf-8', (err, data) => {
        if (err) throw err;

        // Replace &URL& with the highest webNNNN string
        const updatedContent = data.replace('%URL%', highestFolder);

        // Write to index.html in the builds/site directory
        fs.writeFile(outputPath, updatedContent, 'utf-8', (err) => {
            if (err) throw err;
            console.log('index.html created/updated successfully.');
        });
    });
});

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
            const buttonText = index === 0 ? 'Latest Version' : `Version ${versionNumber}`;
        
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