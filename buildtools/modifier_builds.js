const fs = require('fs');
const path = require('path');

const dirPath = path.join(__dirname, '../builds/site');
const styleFilePath = path.join(__dirname, 'style.css'); // Replace with the actual path to your style.css file

function copyStyleToTemplateData(dirPath) {
  fs.readdir(dirPath, (err, files) => {
    if (err) {
      console.error('Error reading directory:', err);
      return;
    }

    files.forEach(file => {
      const filePath = path.join(dirPath, file);

      fs.stat(filePath, (err, stats) => {
        if (err) {
          console.error('Error getting file stats:', err);
          return;
        }

        if (stats.isDirectory() && file !== '.git') { // Ignore .git folders
          const templateDataPath = path.join(filePath, 'TemplateData');
          fs.mkdir(templateDataPath, { recursive: true }, (err) => {
            if (err) {
              console.error('Error creating TemplateData directory:', err);
              return;
            }

            const destinationPath = path.join(templateDataPath, 'style.css');
            fs.copyFile(styleFilePath, destinationPath, (err) => {
              if (err) {
                console.error('Error copying style.css:', err);
              } else {
                console.log('style.css copied to:', destinationPath);
              }
            });
          });
        }
      });
    });
  });
}

copyStyleToTemplateData(dirPath);