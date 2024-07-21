using UnityEditor;
using System.IO;
using HeartUnity;
using static HeartUnity.Local;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace HeartUnity.Tools
{
    public static class LocalizationTools
    {
        [MenuItem("Tools/Localization/Export current language file")]
        static void ExportLanguageFile()
        {
            Local instance = Local.Instance;
            ExportLanguageFile(instance, "local.txt");
        }

        private static void ExportLanguageFile(Local instance, string fileN)
        {
            var header = "key$description$";
            var data = "";

            for (int i = 0; i < instance.languages.Count; i++)
            {
                header += instance.languages[i].languageName;
                if (i == instance.languages.Count - 1)
                {
                    header += "\n";
                }
                else
                {
                    header += "$";
                }
            }
            for (int i = 0; i < instance.keys.Count; i++)
            {
                string key = instance.keys[i];
                var desc = "";
                if (instance.descriptions.Count > 0)
                {
                    desc = instance.descriptions[i];
                }
                data += $"{key}${desc}$";
                for (int i1 = 0; i1 < instance.languages.Count; i1++)
                {
                    Local.LanguageSet lang = instance.languages[i1];
                    if (i1 == instance.languages.Count - 1)
                        data += $"{lang.textSet[key]}";
                    else
                        data += $"{lang.textSet[key]}$";
                }
                if (i != instance.keys.Count - 1)
                {
                    data += "\n";
                }
            }
            string exportFolderPath = "Assets/Export";
            if (!AssetDatabase.IsValidFolder(exportFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Export");
            }
            string filename = fileN;
            ExportFile(header + data, filename);
        }

        [MenuItem("Tools/Localization/Export prompt files")]
        static void ExportPromptFiles()
        {
            LanguageSet englishLang = null;
            if (Local.Instance.languages.Count == 0) {
                HeartGame.ReadLocalizationData();
            }
            foreach (var lang in Local.Instance.languages)
            {
                if (lang.languageName.ToLower() == "english") {
                    englishLang = lang;
                    break;
                }
            }
            foreach (var lang in Local.Instance.languages)
            {
                if (lang != englishLang)
                {
                    ExportPromptFile(englishLang, lang);
                }
            }
            
        }

        [MenuItem("Tools/Localization/Fuse prompt files")]
        static void FusePromptFiles() {
            // Folder path
            string exportFolderPath = Path.Combine(Application.dataPath, "Export");

            // Get all files in the Export folder
            string[] files = Directory.GetFiles(exportFolderPath);

            // Lists to store data
            List<string> languages = new List<string>();
            List<string> datas = new List<string>();

            // Iterate through each file
            foreach (string filePath in files)
            {
                // Ignore files with a .meta extension
                if (Path.GetExtension(filePath).ToLower() == ".meta")
                {
                    continue;
                }
                // Check if the file name contains "prompt_"
                if (Path.GetFileName(filePath).Contains("prompt_"))
                {
                    // Extract language name by removing "prompt_" from the file name
                    string language = Path.GetFileNameWithoutExtension(filePath).Replace("prompt_", "");

                    // Read the content of the file
                    string data = File.ReadAllText(filePath);

                    // Add language and data to the lists
                    languages.Add(language);
                    datas.Add(data);
                }
            }

            var local = new Local();
            foreach (var data in datas)
            {
                local.AddLanguages(data, true);
            }
            int count = -1;
            foreach (var lang in local.languages)
            {
                if (count == -1) {
                    count = lang.textSet.Count;
                }
                if (count != lang.textSet.Count) {
                    Debug.LogError("One of the languages has a different number of elements!!!");
                    return;
                }
            }
            ExportLanguageFile(local, "local.txt");
        }

        private static void ExportPromptFile(LanguageSet englishLang, LanguageSet lang)
        {
            var header = $"key$description${englishLang.languageName}${lang.languageName}\n";
            var data = "";
            for (int i = 0; i < Local.Instance.keys.Count; i++)
            {
                string key = Local.Instance.keys[i];
                var desc = "";
                if (Local.Instance.descriptions.Count > 0)
                {
                    desc = Local.Instance.descriptions[i];
                }
                data += $"{key}${desc}${englishLang.textSet[key]}${lang.textSet[key]}";
                if (i != Local.Instance.keys.Count - 1)
                {
                    data += "\n";
                }
            }
            string filename = $"prompt_{lang.languageName}.txt";
            ExportFile($"This is a csv file where $ is the separator. I'm missing the translation of {lang.languageName}. Add all missing translations to the file. Make it easy to copy. Don't add a $ mark in the end. I'm serious, do not add $ to the end of ANY lines\n\n\n" + header + data, filename);
        }

        private static void ExportFile(string data, string filename)
        {
            string exportFolderPath = "Assets/Export";
            if (!AssetDatabase.IsValidFolder(exportFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Export");

            }

            string filePath = Path.Combine(exportFolderPath, filename);
            File.WriteAllText(filePath, data, Encoding.UTF8);
            AssetDatabase.Refresh();
        }
    }
}
