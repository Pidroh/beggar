using System.IO;
using UnityEditor;
using UnityEngine;

public class SpriteGeneratorWindow : EditorWindow
{
    private Texture2D baseSprite;
    private Texture2D overlaySprite;

    [MenuItem("Window/Sprite Generator Window")]
    public static void ShowWindow()
    {
        GetWindow<SpriteGeneratorWindow>("Sprite Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite Generator", EditorStyles.boldLabel);

        baseSprite = EditorGUILayout.ObjectField("Base Sprite", baseSprite, typeof(Texture2D), false) as Texture2D;
        overlaySprite = EditorGUILayout.ObjectField("Overlay Sprite", overlaySprite, typeof(Texture2D), false) as Texture2D;

        if (GUILayout.Button("Generate"))
        {
            GenerateSprite();
        }
    }

    private void GenerateSprite()
    {
        if (baseSprite == null || overlaySprite == null)
        {
            Debug.LogError("Base Sprite or Overlay Sprite is missing.");
            return;
        }

        int width = Mathf.Max(baseSprite.width, overlaySprite.width);
        int height = Mathf.Max(baseSprite.height, overlaySprite.height);

        Texture2D newSprite = new Texture2D(width, height);
        var offX = 0;

        for (int i = 0; i < 200; i++)
        {
            if (offX + overlaySprite.width > baseSprite.width)
            {
                break;
            }
            var offY = 0;
            for (int j = 0; j < 200; j++)
            {
                DrawOn(offX, offY, baseSprite, overlaySprite, newSprite);
                offY += overlaySprite.height;
                if (offY + overlaySprite.height > baseSprite.height)
                {

                    break;
                }
            }
            offX += overlaySprite.width;
        }

        void DrawOn(int offX, int offY, Texture2D baseSprite, Texture2D overlaySprite, Texture2D newSprite)
        {
            {
                var empty = true;
                for (int yOverlay = 0; yOverlay < overlaySprite.height; yOverlay++)
                {
                    for (int xOverlay = 0; xOverlay < overlaySprite.width; xOverlay++)
                    {
                        if (baseSprite.GetPixel(xOverlay + offX, offY + yOverlay).a > 0)
                        {
                            empty = false;
                            break;
                        }
                    }
                    if (!empty) break;
                }
                if (empty) {
                    for (int yOverlay = 0; yOverlay < overlaySprite.height; yOverlay++)
                    {
                        for (int xOverlay = 0; xOverlay < overlaySprite.width; xOverlay++)
                        {
                            newSprite.SetPixel(offX + xOverlay, offY +yOverlay, new Color(0,0,0,0));
                        }
                    }
                }
                if (empty) return;
            }

            for (int yOverlay = 0; yOverlay < overlaySprite.height; yOverlay++)
            {
                for (int xOverlay = 0; xOverlay < overlaySprite.width; xOverlay++)
                {
                    Color overlayColor = overlaySprite.GetPixel(xOverlay, yOverlay);
                    if (overlayColor.a > 0)
                    {
                        newSprite.SetPixel(xOverlay + offX, yOverlay + offY, overlayColor);
                    }
                    else {
                        newSprite.SetPixel(xOverlay + offX, yOverlay + offY, baseSprite.GetPixel(xOverlay + offX, yOverlay + offY));
                    }
                }
            }
        }

        newSprite.Apply();
        // Encode the generated sprite to a PNG file
        byte[] pngBytes = newSprite.EncodeToPNG();
        if (pngBytes != null)
        {
            string filePath = EditorUtility.SaveFilePanel("Save Generated Sprite", "", "GeneratedSprite", "png");
            if (!string.IsNullOrEmpty(filePath))
            {
                File.WriteAllBytes(filePath, pngBytes);
                Debug.Log("Generated sprite saved at: " + filePath);
            }
        }
        else
        {
            Debug.LogError("Failed to generate PNG bytes.");
        }
    }
}
