using UnityEditor;
using System.IO;
using UnityEngine;
using HeartUnity;
using HeartUnity.View;

public class ExportTextListForEngineFontCreation
{
    [MenuItem("Tools/Localization/Export Text List for Engine Font Creation")]
    private static void CreateTextFileWithContent()
    {
        string folderPath = "Assets/Reusable";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "autogen_engine_default_text.txt");
        var locali = HeartGame.GetConfig().localizationData.text;
        foreach (var item in ReusableSettingMenu.languageLabels)
        {
            locali += item.Value;
        }
        File.WriteAllText(filePath, locali);
        AssetDatabase.Refresh();

        Object createdAsset = AssetDatabase.LoadAssetAtPath<Object>(filePath);
        Selection.activeObject = createdAsset;
    }
    private static bool ValidateCreateTextFileWithContent()
    {
        return Selection.activeObject != null && AssetDatabase.Contains(Selection.activeObject);
    }
}
