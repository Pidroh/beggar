using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArcaniaGameConfiguration))]
public class ArcaniaGameConfigurationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get the target object
        ArcaniaGameConfiguration config = (ArcaniaGameConfiguration)target;

        // Iterate through entries and create buttons
        if (config.entries != null)
        {
            for (int i = 0; i < config.entries.Count; i++)
            {
                var entry = config.entries[i];
                GUILayout.BeginHorizontal();
                var apply = false;
                var build = false;
                if (GUILayout.Button($"Apply {config.entries[i].id}"))
                {
                    apply = true;
                }
                if (GUILayout.Button($"Build {config.entries[i].id}"))
                {
                    apply = true;
                    build = true;
                }
                var gameConfigEngine = (apply || build) ? HeartUnity.HeartGame.GetConfig() : null;
                if (apply) 
                {
                    config.configurationReference.jsonDatas.Clear();
                    config.configurationReference.jsonDatas.AddRange(entry.jsonDatas);
                    if (entry.versionOverride > 0) 
                    {
                        gameConfigEngine.versionNumber = entry.versionOverride;
                        EditorUtility.SetDirty(gameConfigEngine);
                    }
                    EditorUtility.SetDirty(config.configurationReference); // Marks the object as dirty
                    AssetDatabase.SaveAssets(); // Saves the asset to disk
                    AssetDatabase.Refresh();
                }
                if (build) 
                {
                    CustomBuild.BuildWithTag(entry.buildConfigId);
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}
