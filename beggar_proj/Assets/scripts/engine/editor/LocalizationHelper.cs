using UnityEngine;

using System.Collections.Generic;
using HeartUnity;
using static HeartUnity.LocalizedTextAsset;

[CreateAssetMenu(fileName = "TextAssetCopier", menuName = "ScriptableObjects/TextAssetCopier", order = 1)]
public class LocalizationHelper : ScriptableObject
{
    [SerializeField]
    private TextAsset originalTextAsset;

    public string exportHeader;

    public SerializableStringDictionary exportHeaderOverwrite;
    public List<TextAssetHolder> textAssetHolders = new List<TextAssetHolder>();

    public LocalizedTextAsset output;

    internal void ExportToLocalizedAsset()
    {
        output.textAssetHolders.Clear();
        foreach (var tah in textAssetHolders)
        {
            output.textAssetHolders.Add(new TextAssetHolder() { languageName = tah.languageName, textAsset = tah.textAsset });
        }
        UnityEditor.EditorUtility.SetDirty(output);
        UnityEditor.AssetDatabase.SaveAssets();

    }

    public void GenerateCopies()
    {
        CreateAndCollectCopies();
    }

    private void CreateAndCollectCopies()
    {
        if (originalTextAsset == null)
        {
            Debug.LogError("Original TextAsset is not assigned!");
            return;
        }
        HeartGame.ReadLocalizationData();
        textAssetHolders.Clear();
        foreach (var language in Local.Instance.languages)
        {
            if (language.languageName == "English")
            {
                textAssetHolders.Add(new LocalizedTextAsset.TextAssetHolder
                {
                    textAsset = originalTextAsset,
                    languageName = language.languageName
                });
                continue;
            }

            var header = exportHeaderOverwrite.Contains(language.languageName) ? exportHeaderOverwrite.Get(language.languageName) : this.exportHeader;
            header = header.Replace("{0}", language.languageName).Replace("<br>", "\n");
            TextAsset copyTextAsset = LocalizedTextAsset.CreateCopyOfTextAsset(originalTextAsset, language.languageName, header);
            textAssetHolders.Add(new LocalizedTextAsset.TextAssetHolder
            {
                textAsset = copyTextAsset,
                languageName = language.languageName
            });
        }
    }
}
