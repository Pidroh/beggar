using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using HeartUnity;

public class FileReplaceWindow : EditorWindow
{
    private Dictionary<string, List<FileReplaceConfiguration.Entry>> tagToEntries;

    [MenuItem("Window/File Replace Window")]
    public static void ShowWindow()
    {
        GetWindow<FileReplaceWindow>("File Replace");
    }

    private void OnEnable()
    {
        // Search for all FileReplaceConfiguration assets in the project
        string[] configurationPaths = AssetDatabase.FindAssets("t:FileReplaceConfiguration");
        tagToEntries = new Dictionary<string, List<FileReplaceConfiguration.Entry>>();

        foreach (var configurationPath in configurationPaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(configurationPath);
            FileReplaceConfiguration configuration = AssetDatabase.LoadAssetAtPath<FileReplaceConfiguration>(path);

            // Build dictionary for quick access based on tag
            foreach (var entry in configuration.entries)
            {
                if (!tagToEntries.ContainsKey(entry.tag))
                {
                    tagToEntries[entry.tag] = new List<FileReplaceConfiguration.Entry>();
                }
                tagToEntries[entry.tag].Add(entry);
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("File Replace Configuration", EditorStyles.boldLabel);

        // Display buttons for each unique tag across all configurations
        foreach (var tag in tagToEntries.Keys)
        {
            if (GUILayout.Button("Replace files for tag: " + tag))
            {
                ReplaceFilesForTag(tag);
            }
        }
    }

    private void ReplaceFilesForTag(string tag)
    {
        if (tagToEntries.ContainsKey(tag))
        {
            foreach (var motherEntry in tagToEntries[tag])
            {
                foreach (var entry in motherEntry.subEntries)
                {
                    // Get full paths for source and destination
                    string sourcePath = Path.Combine(Application.dataPath, entry.source);
                    string destinationPath = Path.Combine(Application.dataPath, entry.destination);

                    // Perform file copy operation
                    try
                    {
                        File.Copy(sourcePath, destinationPath, true);
                        Debug.Log("File replaced: " + entry.destination);
                        AssetDatabase.Refresh();
                        // If the destination file is a .asset file, ensure the main object name matches the asset filename
                        if (entry.destination.EndsWith(".asset"))
                        {
                            EnsureMainObjectNameMatchesFilename(destinationPath);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to replace file: " + entry.destination + "\n" + e.Message);
                    }
                }
                
            }
        }
        else
        {
            Debug.LogWarning("No entries found for tag: " + tag);
        }
    }

    private void EnsureMainObjectNameMatchesFilename(string assetFilePath)
    {
        string assetObjectName = Path.GetFileNameWithoutExtension(assetFilePath);
        string assetRelativePath = "Assets" + assetFilePath.Substring(Application.dataPath.Length);
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetRelativePath);

        if (asset != null && asset.name != assetObjectName)
        {
            asset.name = assetObjectName;
            EditorUtility.SetDirty(asset);
        }
    }
}
