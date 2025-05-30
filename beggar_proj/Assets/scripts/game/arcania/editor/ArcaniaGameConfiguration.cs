using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
[CreateAssetMenu(fileName = "ArcaniaGameConfiguration", menuName = "Arcania/Arcania Game Configuration", order = 1)]
public class ArcaniaGameConfiguration : ScriptableObject
{
    public ArcaniaGameConfigurationUnit configurationReference;
    public List<Entry> entries;

    [Serializable]
    public class Entry
    {
        public string id;
        public List<TextAsset> jsonDatas;
        public string buildConfigId;
        public int majorVersionOverride;
        public int versionOverride;
        public int patchOverride;
    }
}
#endif