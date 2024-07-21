using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class BuildConfigWindow : EditorWindow
{
    private List<FileBuildConfigurations> buildConfigs = new List<FileBuildConfigurations>();

    [MenuItem("Window/Build Configurations")]
    public static void ShowWindow()
    {
        GetWindow<BuildConfigWindow>("Build Configurations");
    }

    private void OnEnable()
    {
        // Fetch all FileBuildConfigurations assets in the project
        string[] configurationPaths = AssetDatabase.FindAssets("t:FileBuildConfigurations");
        foreach (var configurationPath in configurationPaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(configurationPath);
            var configuration = AssetDatabase.LoadAssetAtPath<FileBuildConfigurations>(path);
            if (configuration != null)
            {
                buildConfigs.Add(configuration);
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Build Configurations", EditorStyles.boldLabel);

        foreach (var config in buildConfigs)
        {
            foreach (var entry in config.entries)
            {
                if (GUILayout.Button(entry.tag))
                {
                    CustomBuild.BuildGameEntry(entry);
                }
            }
        }
    }
}
