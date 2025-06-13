using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace HeartUnity
{
    [CreateAssetMenu(fileName = "LocalizedTextAsset", menuName = "Localization/LocalizedTextAsset")]
    public class LocalizedTextAsset : ScriptableObject
    {
        [System.Serializable]
        public class TextAssetHolder
        {
            public TextAsset textAsset;
            public string languageName;
        }

        public List<TextAssetHolder> textAssetHolders = new List<TextAssetHolder>();

        public string Text => GetText();

        private string GetText()
        {
            foreach (var tah in textAssetHolders)
            {
                if (tah.languageName == Local.Instance.Lang.languageName) {
                    return tah.textAsset.text;
                }
            }
            Debug.LogError("File for language not found in"+this.name);
            return null;
        }

        public bool TryGetText(out string content)
        {
            content = null;
            foreach (var tah in textAssetHolders)
            {
                if (tah.languageName == Local.Instance.Lang.languageName)
                {
                    content = tah.textAsset.text;
                    return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR
        // Static method to create a LocalizedTextAsset based on a TextAsset
        [MenuItem("Tools/Localization/LocalizedTextAsset From Selected TextAsset", false, 101)]
        public static void CreateLocalizedTextAssetFromSelected()
        {
            TextAsset selectedTextAsset = Selection.activeObject as TextAsset;
            if (selectedTextAsset == null)
            {
                Debug.LogError("Please select a TextAsset to create a LocalizedTextAsset from.");
                return;
            }

            LocalizedTextAsset localizedTextAsset = CreateInstance<LocalizedTextAsset>();
            localizedTextAsset.textAssetHolders = new List<TextAssetHolder>();

            HeartGame.ReadLocalizationData();
            foreach (var language in Local.Instance.languages)
            {
                if (language.languageName == "English") {
                    localizedTextAsset.textAssetHolders.Add(new TextAssetHolder
                    {
                        textAsset = selectedTextAsset,
                        languageName = language.languageName
                    });
                    continue;
                }
                TextAsset copyTextAsset = CreateCopyOfTextAsset(selectedTextAsset, language.languageName, "");
                localizedTextAsset.textAssetHolders.Add(new TextAssetHolder
                {
                    textAsset = copyTextAsset,
                    languageName = language.languageName
                });
            }

            // Save the LocalizedTextAsset asset
            string assetPath = AssetDatabase.GetAssetPath(selectedTextAsset);
            assetPath = assetPath.Replace(".txt", "_LocalizedTextAsset.asset");
            AssetDatabase.CreateAsset(localizedTextAsset, assetPath);
            AssetDatabase.SaveAssets();

            // Refresh the project window
            AssetDatabase.Refresh();

            Debug.Log("LocalizedTextAsset created at: " + assetPath);
        }

        public string GetConcatenatedText()
        {
            var text = "";
            foreach (var asset in textAssetHolders)
            {
                text += asset.textAsset.text;
            }
            return text;
        }

        public static TextAsset CreateCopyOfTextAsset(TextAsset original, string languageName, string header)
        {
            string originalPath = AssetDatabase.GetAssetPath(original);
            string directory = Path.GetDirectoryName(originalPath);

            string newPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, original.name + "_" + languageName + ".txt"));
            //var ta = new TextAsset("Translate this to "+languageName + "\n\n" + original.text);
            File.WriteAllText(newPath, $"{header}{original.text}", System.Text.Encoding.UTF8);
            //AssetDatabase.CreateAsset(ta, newPath);

            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<TextAsset>(newPath);
        }
#endif
    }
}
