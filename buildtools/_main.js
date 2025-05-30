const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

Execute("version_note_for_menu.js");
const dirPath = path.join(__dirname, '../builds/site');


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

  const dirs = items.filter(item => fs.statSync(path.join(dirPath, item)).isDirectory());

  const newFormatDirs = dirs
      .filter(dir => /^web\d_\d{2}_\d{2}$/.test(dir))
      .sort((a, b) => b.slice(3).localeCompare(a.slice(3))); // Sort descending

  const oldFormatDirs = dirs
      .filter(dir => /^web\d{4}$/.test(dir))
      .sort((a, b) => b.slice(3).localeCompare(a.slice(3))); // Sort descending

  fs.readFile(menuTemplatePath, 'utf-8', (err, templateContent) => {
      if (err) throw err;

      const buttonsNew = newFormatDirs.map(dir => {
          const version = dir.slice(3).replace(/_/g, '.');
          return `<button onclick="window.location.href='./${dir}/index.html'">Version ${version}</button>`;
      });

      const buttonsOld = oldFormatDirs.map(dir => {
          const padded = `0.${dir.slice(3)}.0`.replace(/\.?0+$/, ''); // Remove trailing ".0" if present
          return `<button onclick="window.location.href='./${dir}/index.html'">Version ${padded}</button>`;
      });

      const buttonsHTML = [...buttonsNew, ...buttonsOld].join('\n');
      const releaseNotesIns = fs.readFileSync('menu_release_notes_for_insertion.txt', 'utf8');
      const finalContent = templateContent
          .replace('%MENU%', buttonsHTML)
          .replace('%RELEASE_NOTES%', releaseNotesIns);

      fs.writeFile(outputMenuPath, finalContent, 'utf-8', err => {
          if (err) throw err;
          console.log('menu.html created/updated successfully.');
      });
  });
});


const latestPath = path.join(dirPath, 'latest');
const latestBetaPath = path.join(dirPath, 'latest_beta');

const parseVersion = (name) => {
    if (/^web\d{4}$/.test(name)) {
        const n = parseInt(name.slice(3), 10);
        return [0, n, 0];
    } else if (/^web\d_\d{2}_\d{2}$/.test(name)) {
        return name.slice(3).split('_').map(s => parseInt(s, 10));
    }
    return null;
};

const folders = fs.readdirSync(dirPath).filter(folder => fs.lstatSync(path.join(dirPath, folder)).isDirectory());

const nonBetaFolders = folders.filter(f => !f.includes('_beta'));
const betaFolders = folders.filter(f => f.includes('_beta'));

const compareVersions = (a, b) => {
    const va = parseVersion(a);
    const vb = parseVersion(b);
    for (let i = 0; i < 3; i++) {
        if (va[i] !== vb[i]) return vb[i] - va[i];
    }
    return 0;
};

const latestFolder = nonBetaFolders.filter(f => parseVersion(f)).sort(compareVersions)[0];
const latestBetaFolder = betaFolders.filter(f => parseVersion(f.replace('_beta', ''))).sort((a, b) =>
    compareVersions(a.replace('_beta', ''), b.replace('_beta', ''))
)[0];

if (latestFolder) {
    console.log("Latest folder: " + latestFolder);
    fs.rmSync(latestPath, { recursive: true, force: true });
    fs.cpSync(path.join(dirPath, latestFolder), latestPath, { recursive: true });
}

if (latestBetaFolder) {
    console.log("Latest beta folder: " + latestBetaFolder);
    fs.rmSync(latestBetaPath, { recursive: true, force: true });
    fs.cpSync(path.join(dirPath, latestBetaFolder), latestBetaPath, { recursive: true });
}

Execute("add_menu.js");
Execute("modifier_builds.js");
