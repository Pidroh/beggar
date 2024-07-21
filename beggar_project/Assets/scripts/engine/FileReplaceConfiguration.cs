using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeartUnity
{
    // Define your ScriptableObject class
    [CreateAssetMenu(fileName = "FileReplacerConfiguration", menuName = "Custom/File Replacer Configuration", order = 1)]
    public class FileReplaceConfiguration : ScriptableObject
    {
        public List<Entry> entries;

        [Serializable]
        public class Entry
        {
            public string tag;
            public List<SubEntry> subEntries;
        }

        [Serializable]
        public class SubEntry 
        {
            public string source;
            public string destination;
        }
    }

}