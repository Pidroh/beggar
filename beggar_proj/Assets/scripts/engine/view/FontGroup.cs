using UnityEngine;
using TMPro;
using System;

[CreateAssetMenu(fileName = "FontSettings", menuName = "Custom/Font Settings")]
public class FontGroup : ScriptableObject
{
    public FontHolder[] fontHolders;

    [System.Serializable]
    public class FontHolder
    {
        public TMP_FontAsset fontAsset;
        public int targetSize;
        public string language;
    }

    public FontHolder GetFont(string languageName)
    {
        foreach (var fh in fontHolders)
        {
            if (fh.language == languageName)
            {
                return fh;
            }
        }
        return fontHolders[0];
    }
}
