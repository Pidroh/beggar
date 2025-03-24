using UnityEngine;
using UnityEditor;
using System.IO;

public class ArcaniaAutoImageEditorUtility : EditorWindow
{
    [MenuItem("Tools/Arcania/Update Game Configuration with Images")]
    public static void UpdateGameConfiguration()
    {
        // Find the ScriptableObject of type ArcaniaGameConfigurationUnit in the project
        string[] assetPaths = AssetDatabase.FindAssets("t:ArcaniaGameConfigurationUnit");

        if (assetPaths.Length == 0)
        {
            Debug.LogError("No ArcaniaGameConfigurationUnit found in the assets.");
            return;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(assetPaths[0]);
        ArcaniaGameConfigurationUnit configUnit = AssetDatabase.LoadAssetAtPath<ArcaniaGameConfigurationUnit>(assetPath);

        if (configUnit == null)
        {
            Debug.LogError("Failed to load ArcaniaGameConfigurationUnit.");
            return;
        }

        // Get all sprites in the folder "Assets/view/images"
        string folderPath = "Assets/view/images";
        string[] spritePaths = Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories);

        foreach (var spritePath in spritePaths)
        {
            string relativePath = "Assets" + spritePath.Substring(Application.dataPath.Length);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);

            if (sprite != null)
            {
                // Call Add() method of the ScriptableObject with the sprite
                configUnit.spritesForLayout.Add(sprite);
            }
            else
            {
                Debug.LogWarning("Failed to load sprite: " + spritePath);
            }
        }

        // Mark the ScriptableObject as dirty to ensure changes are saved
        EditorUtility.SetDirty(configUnit);

        // Save the asset
        AssetDatabase.SaveAssets();

        Debug.Log("Game Configuration updated successfully.");
    }
}
