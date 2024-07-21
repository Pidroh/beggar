using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
[CustomEditor(typeof(LocalizationHelper))]
public class LocalizationHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LocalizationHelper myScript = (LocalizationHelper)target;

        if (GUILayout.Button("Generate Copies"))
        {
            myScript.GenerateCopies();
            EditorUtility.SetDirty(myScript);
        }
        if (GUILayout.Button("Export to Localized Asset"))
        {
            myScript.ExportToLocalizedAsset();
            EditorUtility.SetDirty(myScript);
        }
    }
}
#endif