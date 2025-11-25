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
                    // Resolve JSON data based on jsonKey
                    config.configurationReference.jsonDatas.Clear();
                    if (config.jsonEntries != null && !string.IsNullOrWhiteSpace(entry.jsonKey))
                    {
                        for (int k = 0; k < config.jsonEntries.entries.Count; k++)
                        {
                            var jsonUnit = config.jsonEntries.entries[k];
                            if (jsonUnit == null || jsonUnit.jsons == null)
                            {
                                continue;
                            }

                            if (jsonUnit.key == entry.jsonKey)
                            {
                                config.configurationReference.jsonDatas.AddRange(jsonUnit.jsons);
                            }
                        }

                    }

                    // Resolve misc info based on miscKey
                    ArcaniaGameConfiguration.EntryMiscInfo miscInfo = null;
                    if (config.entryMiscInfos != null && !string.IsNullOrWhiteSpace(entry.miscKey))
                    {
                        for (int j = 0; j < config.entryMiscInfos.Count; j++)
                        {
                            var candidate = config.entryMiscInfos[j];
                            if (candidate != null && candidate.key == entry.miscKey)
                            {
                                miscInfo = candidate;
                                break;
                            }
                        }
                    }

                    if (miscInfo != null)
                    {
                        if (!string.IsNullOrWhiteSpace(miscInfo.SubtitleOverride))
                        {
                            config.configurationReference.gameSubTitleText = miscInfo.SubtitleOverride;
                        }
                        if (miscInfo.majorVersionOverride >= 0)
                        {
                            gameConfigEngine.majorVersion = miscInfo.majorVersionOverride;
                        }
                        // needs to allow 0 because 1.0.0 is a valid version (major.version.patch)
                        if (miscInfo.versionOverride >= 0)
                        {
                            gameConfigEngine.versionNumber = miscInfo.versionOverride;
                            EditorUtility.SetDirty(gameConfigEngine);
                        }
                        if (miscInfo.majorVersionOverride >= 0)
                        {
                            gameConfigEngine.patchVersion = miscInfo.patchOverride;
                        }
                    }

                    gameConfigEngine.patreonBuild = entry.patreonBuild;

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

