const fs = require('fs');

// Read the version notes from the file
fs.readFile('version_notes.txt', 'utf8', (err, data) => {
  if (err) throw err;

  const versions = data.split('\n\n'); // Split the data into version blocks
  let htmlContent = '';

  versions.forEach(version => {
    const lines = version.split('\n');
    let versionHeader = lines[0].replace('Version ', '').trim();
    
    // Ensure only the number is extracted, ignoring the - part
    versionHeader = versionHeader.replace(/[^0-9.]/g, ''); // Remove anything that's not a number or dot

    // Format the version as 0.x if it's an integer
    if (!versionHeader.includes('.')) {
      versionHeader = `0.${versionHeader}`;
    }

    let devNotes = '';
    let patreon = false;

    // Check for Patreon and Dev notes
    lines.forEach(line => {
      if (line.toLowerCase().includes('patreon: true')) {
        patreon = true;
      } else if (line.toLowerCase().startsWith('dev notes:')) {
        devNotes = line.replace('Dev notes:', '').trim();
      }
    });

    // Add (Patreon Early Access) if Patreon is true
    if (patreon) {
      versionHeader += ' (Patreon Early Access)';
    }

    const changesStartIndex = lines.indexOf('# CHANGES') + 1;
    const changes = lines.slice(changesStartIndex).join('<br>');

    // Format each version as HTML
    htmlContent += `
      <div class="version">
        <h2>Version ${versionHeader}</h2>
        <p>${devNotes}</p>
        <p>${changes}</p>
      </div>
    `;
  });

  // Write the formatted HTML content to the output file
  fs.writeFile('menu_release_notes_for_insertion.txt', htmlContent, (err) => {
    if (err) throw err;
    console.log('Release notes HTML saved!');
  });
});
