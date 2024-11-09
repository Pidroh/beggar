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
