using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "FileBuildConfiguration", menuName = "Custom/File Build Configuration", order = 1)]
public class FileBuildConfigurations : ScriptableObject
{
    public List<Entry> entries;

    [Serializable]
    public class Entry
    {
        public string tag;
        public string copyFileTag;
        public string outputPath;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    }
}
