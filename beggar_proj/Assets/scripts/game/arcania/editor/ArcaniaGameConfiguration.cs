using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
[CreateAssetMenu(fileName = "ArcaniaGameConfiguration", menuName = "Arcania/Arcania Game Configuration", order = 1)]
public class ArcaniaGameConfiguration : ScriptableObject
{
    public ArcaniaGameConfigurationUnit configurationReference;
    public List<Entry> entries;
    public List<JsonEntries> jsonEntries;
    public List<EntryMiscInfo> entryMiscInfos;

    [Serializable]
    public class Entry
    {
        public string id;
        public string jsonKey;
        public string miscKey;
        public List<TextAsset> jsonDatas;
        public string buildConfigId;
        public int majorVersionOverride;
        public int versionOverride;
        public int patchOverride;
        public string SubtitleOverride;
        public bool patreonBuild;
    }

    [Serializable]
    public class EntryMiscInfo
    {
        public string key;
        public int majorVersionOverride;
        public int versionOverride;
        public int patchOverride;
        public string SubtitleOverride;
    }
}
#endif
