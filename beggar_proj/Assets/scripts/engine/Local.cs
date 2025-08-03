//using UnityEngine.U2D;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace HeartUnity
{
    public class Local
    {
        public int currentLangIndex = 0;
        public LanguageSet Lang => languages[currentLangIndex];
        public LanguageSet FirstLang => languages[0];

        public static bool WantToChooseLanguage => !Instance.languageChosen && Instance.languages.Count > 1;

        public static bool HasMoreThaOneLanguage => Instance.languages.Count > 1;
        public static bool IsFirstLanguage => Instance.currentLangIndex == 0;

        public List<LanguageSet> languages = new List<LanguageSet>();
        public static Local Instance = new Local();
        public List<String> descriptions = new List<string>();
        public List<String> keys = new List<string>();
        public bool languageChosen;

        public Local(List<LanguageSet> languages)
        {
            this.languages = languages;
        }

        public Local()
        {
        }

        public void Init(string localiData)
        {
            // if (languages.Count > 0) return;
            languages.Clear();
            keys.Clear();
            descriptions.Clear();
            AddLanguages(localiData, true);
            int count = -1;
            foreach (var lang in languages)
            {
                if (count == -1)
                {
                    count = lang.textSet.Count;
                }
                if (count != lang.textSet.Count)
                {
                    Debug.LogError("ERROR: localization data missing entries");
                }
            }
        }

        

        public void AddLanguages(string localiData, bool replaceSpaceWithUnderscoreInKey)
        {
            var firstLanguageAddition = languages.Count == 0;
            var lines = localiData.Split('\n');
            var headerLine = lines[0];
            var headers = headerLine.Split("$");
            int keyIndex = -1;
            int descriptionIndex = -1;
            for (int i = 0; i < headers.Length; i++)
            {
                string head = headers[i];
                var header = head.Trim();
                headers[i] = header;
                if (header.ToLower() == "key")
                {
                    keyIndex = i;
                    continue;
                }
                if (header.ToLower() == "description")
                {
                    descriptionIndex = i;
                    continue;
                }
                var ls = new LanguageSet();
                ls.languageName = header;
                bool replaced = false;
                for (int lIndex = 0; lIndex < languages.Count; lIndex++)
                {
                    if (languages[lIndex].languageName == header)
                    {
                        languages[lIndex] = ls;
                        replaced = true;
                        break;
                    }
                }
                if (!replaced)
                {
                    languages.Add(ls);
                }
            }

            // actual data
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var lineElements = lines[i].Split("$");
                ProcessLineElements(lineElements, keyIndex, descriptionIndex);
            }

            void ProcessLineElements(string[] lineEles, int key, int desc)
            {
                if (key < 0 || lineEles[key] == null)
                {
                    Debug.LogError("something is wrong");
                }
                var keyEle = lineEles[key].Trim();
                if (replaceSpaceWithUnderscoreInKey) keyEle = keyEle.Replace(' ', '_');
                if (firstLanguageAddition)
                {
                    keys.Add(keyEle);
                    if (desc >= 0)
                        descriptions.Add(lineEles[desc]);
                }
                for (int i = 0; i < lineEles.Length; i++)
                {
                    if (i == key || i == desc) continue;
                    var le = lineEles[i].Trim();
                    foreach (var lang in languages)
                    {
                        if (lang.languageName == headers[i])
                        {
                            lang.textSet[keyEle] = le;
                        }
                    }
                }
            }
        }

        // replaceSpaceWithUnderscoreInKe HAS to be true or it bugs I think. Maybe remove the argument and force the replacement
        public void AppendLocalizationData(string localiData, bool replaceSpaceWithUnderscoreInKey)
        {
            var lines = localiData.Split('\n');
            var headerLine = lines[0];
            var headers = headerLine.Split("$");
            int keyIndex = -1;
            int descriptionIndex = -1;
            using var _1 = DictionaryPool<int, int>.Get(out var indexRedirector);
            for (int i = 0; i < headers.Length; i++)
            {
                string head = headers[i];
                var header = head.Trim();
                headers[i] = header;
                if (header.ToLower() == "key")
                {
                    keyIndex = i;
                    continue;
                }
                if (header.ToLower() == "description")
                {
                    descriptionIndex = i;
                    continue;
                }
                LanguageSet ls = null;
                bool found = false;
                for (int lIndex = 0; lIndex < languages.Count; lIndex++)
                {
                    if (languages[lIndex].languageName == header)
                    {
                        ls = languages[lIndex];
                        found = true;
                        indexRedirector[i] = lIndex;
                        break;
                    }
                }
                if (!found)
                {
                    Debug.LogError("Trying to append but language not found "+header);
                }
            }
            // actual data
            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var lineElements = lines[lineIndex].Split("$");
                if (keyIndex < 0 || lineElements[keyIndex] == null)
                {
                    Debug.LogError("something is wrong");
                }
                var keyEle = lineElements[keyIndex].Trim();
                if (replaceSpaceWithUnderscoreInKey) keyEle = keyEle.Replace(' ', '_');
                keys.Add(keyEle);
                if (descriptionIndex >= 0)
                    descriptions.Add(lineElements[descriptionIndex]);
                else if(descriptions.Count > 0)
                    descriptions.Add("");
                for (int col = 0; col < lineElements.Length; col++) 
                {
                    if (col == keyIndex) continue;
                    if (col == descriptionIndex) continue;
                    var langIndex = indexRedirector[col];
                    var lang = languages[langIndex];
                    var le = lineElements[col].Trim();
                    lang.textSet[keyEle] = le;
                }                
            }
        }

        public static void ChangeLanguage(string languageName)
        {
            Instance.languageChosen = true;
            for (int i = 0; i < Instance.languages.Count; i++)
            {
                LanguageSet lang = Instance.languages[i];
                if (lang.languageName == languageName)
                {
                    ChangeLanguage(i);
                    return;
                }
            }
            Instance.languageChosen = false;
        }

        internal static void ChangeLanguage(int rtInt)
        {
            Instance.currentLangIndex = rtInt;
        }

        public static string GetText(string key)
        {
            return Instance.GetTextInstance(key);
        }

        public static string GetText(string key, string desc)
        {
            return Instance.GetTextInstance(key, desc);
        }

        public string GetTextInstance(string key, string desc = null)
        {

            if (key.Contains(' '))
            {
                key = key.Replace(" ", "_");
            }
            if (languages.Count == 0 || !Lang.textSet.TryGetValue(key, out string value))
            {
                return FallBack(key, desc);
            }
            if (value == "")
            {
                return "err_" + key;
            }
            return value;
        }

        private string FallBack(string key, string desc)
        {
            var value = key.Replace('_', ' '); //.Replace("\n", "<br>");
            if (languages.Count == 0) return value;
            FirstLang.textSet[key] = value;
            foreach (var lang in languages)
            {
                if (FirstLang != lang) lang.textSet[key] = "";
            }
            keys.Add(key);
            descriptions.Add(desc ?? "");
            return value;
        }

        public class LanguageSet
        {
            public string languageName;
            public Dictionary<string, string> textSet = new Dictionary<string, string>();
        }
    }


}