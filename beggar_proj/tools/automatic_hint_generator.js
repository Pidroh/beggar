const fs = require('fs');
const path = require('path');

const BLACKLIST_PATH = path.resolve(__dirname, 'automatic_hint_blacklist.json');

async function main() {
    // Hard-coded folder path relative to repo root
    const folderPath = path.resolve(__dirname, '../Assets/data');
    if (!fs.existsSync(folderPath)) {
        console.error(`Data folder not found: ${folderPath}`);
        return;
    }

    // Load blacklist (create if missing)
    const blacklist = loadBlacklist();

    // Process matching JSON files
    const files = fs.readdirSync(folderPath).filter(f => f.startsWith('main_data_') && f.endsWith('.json'));

    for (const file of files) {
        const fullPath = path.join(folderPath, file);
        const jsonData = JSON.parse(fs.readFileSync(fullPath, 'utf8'));

        // Collect classes and existing hints
        const classBlocks = jsonData.filter(e => e.type === 'CLASS');
        const hintBlock = jsonData.find(e => e.type === 'HINT');
        let hintItems = hintBlock && Array.isArray(hintBlock.items) ? hintBlock.items : [];

        // Ensure we have a hint section
        let targetHintBlock = hintBlock;
        if (!targetHintBlock) {
            targetHintBlock = { type: 'HINT', items: [] };
            jsonData.push(targetHintBlock);
            hintItems = targetHintBlock.items;
        }

        // Remove hints for blacklisted classes
        let removedCount = 0;
        if (Array.isArray(hintItems) && blacklist.size > 0) {
            const before = hintItems.length;
            targetHintBlock.items = hintItems.filter(h => {
                const tgt = h && h.target_id;
                if (!tgt) return true;
                return !blacklist.has(tgt);
            });
            removedCount = before - targetHintBlock.items.length;
            hintItems = targetHintBlock.items;
        }

        // Build current hint indices after removals
        const existingHintIds = new Set(hintItems.map(h => h.id));
        const existingHintTargets = new Set(hintItems.map(h => h.target_id));

        let addedCount = 0;

        for (const block of classBlocks) {
            const items = Array.isArray(block.items) ? block.items : [];
            for (const item of items) {
                const classId = item.id;
                if (!classId) continue;

                // Skip blacklisted classes entirely
                if (blacklist.has(classId)) continue;

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

        if (addedCount > 0 || removedCount > 0) {
            fs.writeFileSync(fullPath, JSON.stringify(jsonData, null, 4));
            console.log(`Updated: ${file} (+${addedCount} hints, -${removedCount} removed)`);
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

function loadBlacklist() {
    try {
        if (!fs.existsSync(BLACKLIST_PATH)) {
            fs.writeFileSync(BLACKLIST_PATH, JSON.stringify([], null, 4));
            return new Set();
        }
        const raw = fs.readFileSync(BLACKLIST_PATH, 'utf8');
        const list = JSON.parse(raw);
        if (Array.isArray(list)) {
            return new Set(list.filter(v => typeof v === 'string' && v.trim().length > 0));
        }
        console.warn('Blacklist file is not an array; ignoring.');
        return new Set();
    } catch (err) {
        console.error('Failed to load blacklist, proceeding with empty set:', err.message);
        return new Set();
    }
}

main();
