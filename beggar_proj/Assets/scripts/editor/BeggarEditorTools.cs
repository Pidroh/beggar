using UnityEngine;
using UnityEditor;
using HeartUnity;

public class BeggarEditorTools
{
    [MenuItem("Tools/Beggar/Localize ping for editor tools")]
    public static void LocalizePingForEditorTools()
    {
        Debug.Log("Localize ping for editor tools executed!");
        
        // Find all ArcaniaGameConfiguration assets in the project
        string[] guids = AssetDatabase.FindAssets("t:ArcaniaGameConfiguration");
        
        if (guids.Length == 0)
        {
            Debug.Log("No ArcaniaGameConfiguration assets found in the project.");
            return;
        }
        
        Debug.Log($"Found {guids.Length} ArcaniaGameConfiguration asset(s):");
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ArcaniaGameConfiguration config = AssetDatabase.LoadAssetAtPath<ArcaniaGameConfiguration>(assetPath);
            
            if (config != null)
            {
                foreach (var entry in config.entryMiscInfos)
                {
                    Local.GetText(entry.SubtitleOverride);
                }
                
                Debug.Log($"Loaded: {config.name} at path: {assetPath}");
                
                // You can now work with the loaded configuration
                // Add your processing logic here
            }
        }
    }
}