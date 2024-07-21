using UnityEngine;
using UnityEditor;
using UnityEngine.TextCore.LowLevel;
using System;
using HeartUnity;
using HeartUnity.View;

[CreateAssetMenu(fileName = "FontGeneratorData", menuName = "Custom/Font Generator Data")]
public class FontGeneratorData : ScriptableObject
{
    public Font font;
    public bool useTextVariable = false;
    public string text = "qwertyuiiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBM";
    public int sampleSize = 16;
    public int atlasPad = 1;
    public GlyphRenderMode renderMode = GlyphRenderMode.RASTER;
    public int atlasW = 2048;
    public int atlasH = 2048;
    public string fontName = "fontName";
    public LocalizedTextAsset[] localizedTextAssets;
    public bool copyPreviousMaterial = true;
    public FilterMode filterMode = FilterMode.Point;

    public bool colorFont = false;
    public Color faceColor = Color.white;
    public Color outlineColor = Color.clear;

    public void GenerateFont()
    {
        // Call your font generation logic here
        var fnt = FontGenerator.GenerateFont(font, (useTextVariable ? text : CreateTextFromLocalization()), sampleSize, atlasPad, renderMode, atlasW, atlasH, fontName, copyPreviousMaterial, filterMode);
        if (colorFont) {
            ModifyTexture(fnt.atlasTexture, faceColor, outlineColor);
        }

    }

    private string CreateTextFromLocalization()
    {
        var text = HeartGame.GetConfig().localizationData.text;
        foreach (var lta in localizedTextAssets)
        {
            text += lta.GetConcatenatedText();
        }
        return text + " \"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" + ReusableSettingMenu.GetAllLanguageNamesConcatenated();
    }

    // Method to modify the texture
    public static void ModifyTexture(Texture2D texture, Color mainColor, Color outlineColor)
    {
        // Get the pixels from the original texture
        Color[] pixels = texture.GetPixels();
        var newPixels = new Color[pixels.Length];

        // Loop through each pixel
        for (int i = 0; i < pixels.Length; i++)
        {
            newPixels[i] = pixels[i];
            // Check if the pixel is not fully transparent
            if (pixels[i].a != 0f)
            {
                // Change the color of the pixel to the main color
                newPixels[i] = mainColor;
                newPixels[i].a = pixels[i].a;

                // Add an outline by checking the neighboring pixels
                if (i % texture.width != 0 && pixels[i - 1].a == 0f)
                    newPixels[i - 1] = outlineColor; // Left pixel
                if (i % texture.width != texture.width - 1 && pixels[i + 1].a == 0f)
                    newPixels[i + 1] = outlineColor; // Right pixel
                if (i >= texture.width && pixels[i - texture.width].a == 0f)
                    newPixels[i - texture.width] = outlineColor; // Top pixel
                if (i < pixels.Length - texture.width && pixels[i + texture.width].a == 0f)
                    newPixels[i + texture.width] = outlineColor; // Bottom pixel
            }
        }

        // Apply the modified pixels back to the texture
        texture.SetPixels(newPixels);

        // Apply changes to the original asset
        texture.Apply();
    }
}

[CustomEditor(typeof(FontGeneratorData))]
public class FontGeneratorDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        FontGeneratorData fontGeneratorData = (FontGeneratorData)target;

        if (GUILayout.Button("Generate Font"))
        {
            fontGeneratorData.GenerateFont();
        }
    }
}
