const fs = require('fs');
const path = require('path');

const dirPath = path.join(__dirname, '../builds/site');
const specialTextPath = path.join(__dirname, 'menu_content.txt');
const specialText = fs.readFileSync(specialTextPath, 'utf8');

function processHTMLFile(filePath) {
  let content = fs.readFileSync(filePath, 'utf8');
  
  const autoContentStart = '<!-- AUTO CONTENT -->';
  const autoContentEnd = '<!-- AUTO CONTENT END -->';
  const bodyTagRegex = /<body\b[^>]*>/i;

  if (content.includes(autoContentStart)) {
    const startIndex = content.indexOf(autoContentStart) + autoContentStart.length;
    const endIndex = content.indexOf(autoContentEnd);
    content = content.slice(0, startIndex) + specialText + content.slice(endIndex);
  } else {
    const match = content.match(bodyTagRegex);
    if (match) {
      const bodyTagIndex = match.index + match[0].length;
      content = content.slice(0, bodyTagIndex) +
                `\n${autoContentStart}${specialText}${autoContentEnd}\n` +
                content.slice(bodyTagIndex);
    }
  }

  fs.writeFileSync(filePath, content, 'utf8');
}

function findAndProcessIndexHtml(dir) {
  const entries = fs.readdirSync(dir, { withFileTypes: true });

  for (const entry of entries) {
    const fullPath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      findAndProcessIndexHtml(fullPath);
    } else if (entry.isFile() && entry.name === 'index.html') {
      processHTMLFile(fullPath);
    }
  }
}

findAndProcessIndexHtml(dirPath);
