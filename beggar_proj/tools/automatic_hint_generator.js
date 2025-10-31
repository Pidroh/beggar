const fs = require('fs');
const path = require('path');

async function main() {
    // Hard-coded folder path relative to repo root
    const folderPath = path.resolve(__dirname, '../Assets/data');
    if (!fs.existsSync(folderPath)) {
        console.error(`Data folder not found: ${folderPath}`);
        return;
    }

    // Process matching JSON files
    const files = fs.readdirSync(folderPath).filter(f => f.startsWith('main_data_') && f.endsWith('.json'));

    for (const file of files) {
        const fullPath = path.join(folderPath, file);
        const jsonData = JSON.parse(fs.readFileSync(fullPath, 'utf8'));

        // Collect classes and existing hints
        const classBlocks = jsonData.filter(e => e.type === 'CLASS');
        const hintBlock = jsonData.find(e => e.type === 'HINT');
        const hintItems = hintBlock && Array.isArray(hintBlock.items) ? hintBlock.items : [];

        const existingHintIds = new Set(hintItems.map(h => h.id));
        const existingHintTargets = new Set(hintItems.map(h => h.target_id));

        let addedCount = 0;

        // Ensure we have a hint section
        let targetHintBlock = hintBlock;
        if (!targetHintBlock) {
            targetHintBlock = { type: 'HINT', items: [] };
            jsonData.push(targetHintBlock);
        }

        for (const block of classBlocks) {
            const items = Array.isArray(block.items) ? block.items : [];
            for (const item of items) {
                const classId = item.id;
                if (!classId) continue;

                const hintId = `${classId}_hint`;
                if (existingHintIds.has(hintId) || existingHintTargets.has(classId)) {
                    continue; // Hint already exists for this class
                }

                const tag = pickRelevantTag(item);
                if (!tag) {
                    // No suitable tag found; skip creating a hint
                    continue;
                }

                const newHint = {
                    id: hintId,
                    name: 'Class hint',
                    target_id: classId,
                    require: `hint_${tag}`
                };

                targetHintBlock.items.push(newHint);
                existingHintIds.add(hintId);
                existingHintTargets.add(classId);
                addedCount++;
            }
        }

        if (addedCount > 0) {
            fs.writeFileSync(fullPath, JSON.stringify(jsonData, null, 4));
            console.log(`Updated: ${file} (+${addedCount} hints)`);
        } else {
            console.log(`No changes: ${file}`);
        }
    }
}

function pickRelevantTag(item) {
    // Tags can be under 'tags' or 'tag' and may be comma-separated
    const raw = item.tags || item.tag;
    if (!raw || typeof raw !== 'string') return null;
    const tags = raw.split(',').map(t => t.trim()).filter(Boolean);

    // Prefer t_job if present
    if (tags.includes('t_job')) return 't_job';

    // Otherwise pick the first t_tierN tag
    const tier = tags.find(t => /^t_tier\d+$/.test(t));
    if (tier) return tier;

    return null;
}

main();
