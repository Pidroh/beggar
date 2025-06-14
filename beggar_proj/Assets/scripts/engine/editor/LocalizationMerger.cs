namespace HeartUnity.Tools
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;

    public class LocalizationMerger : EditorWindow
    {
        TextAsset mainFile;
        TextAsset fileToAdd;

        [MenuItem("Tools/Localization Merger")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationMerger>("Localization Merger");
        }

        void OnGUI()
        {
            mainFile = (TextAsset)EditorGUILayout.ObjectField("Main File", mainFile, typeof(TextAsset), false);
            fileToAdd = (TextAsset)EditorGUILayout.ObjectField("File to Add", fileToAdd, typeof(TextAsset), false);

            if (GUILayout.Button("Merge Files"))
            {
                if (mainFile == null || fileToAdd == null)
                {
                    Debug.LogError("Please assign both files.");
                    return;
                }

                Merge();
            }
        }

        void Merge()
        {
            string[] mainLines = mainFile.text.Split('\n');
            string[] addLines = fileToAdd.text.Split('\n');

            if (mainLines.Length == 0 || addLines.Length == 0)
            {
                Debug.LogError("One of the files is empty.");
                return;
            }

            string[] mainHeader = mainLines[0].Trim().Split('$');
            string[] addHeader = addLines[0].Trim().Split('$');

            var langIndexMap = new Dictionary<string, int>();
            for (int i = 2; i < mainHeader.Length; i++)
                langIndexMap[mainHeader[i]] = i;

            var mainDict = new Dictionary<string, string[]>();
            for (int i = 1; i < mainLines.Length; i++)
            {
                var line = mainLines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var split = line.Split('$');
                mainDict[split[0]] = split;
            }

            for (int i = 1; i < addLines.Length; i++)
            {
                var line = addLines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var addSplit = line.Split('$');
                string key = addSplit[0];

                if (!mainDict.ContainsKey(key))
                {
                    var newLine = new string[mainHeader.Length];
                    newLine[0] = key;
                    newLine[1] = addSplit.Length > 1 ? addSplit[1] : "";

                    for (int j = 2; j < mainHeader.Length; j++)
                    {
                        int addIndex = System.Array.IndexOf(addHeader, mainHeader[j]);
                        newLine[j] = (addIndex != -1 && addIndex < addSplit.Length) ? addSplit[addIndex] : "";
                    }

                    mainDict[key] = newLine;
                }
                else
                {
                    var existing = mainDict[key];
                    for (int j = 2; j < mainHeader.Length; j++)
                    {
                        if (j >= existing.Length) continue;
                        if (string.IsNullOrWhiteSpace(existing[j]))
                        {
                            int addIndex = System.Array.IndexOf(addHeader, mainHeader[j]);
                            if (addIndex != -1 && addIndex < addSplit.Length)
                            {
                                existing[j] = addSplit[addIndex];
                            }
                        }
                    }
                }
            }

            var output = new List<string> { string.Join("$", mainHeader) };
            foreach (var kvp in mainDict)
            {
                var line = new string[mainHeader.Length];
                var data = kvp.Value;
                for (int j = 0; j < mainHeader.Length; j++)
                    line[j] = (j < data.Length) ? data[j] : "";
                output.Add(string.Join("$", line));
            }

            var path = AssetDatabase.GetAssetPath(mainFile);
            File.WriteAllText(path, string.Join("\n", output));
            AssetDatabase.Refresh();
            Debug.Log("Localization merge complete.");
        }
    }
}
