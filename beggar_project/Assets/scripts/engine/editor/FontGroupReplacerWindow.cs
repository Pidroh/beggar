using UnityEngine;
using UnityEditor;
using TMPro;
using HeartUnity.View;

public class FontGroupReplacerWindow : EditorWindow
{
    private FontGroup targetFontGroup; // Field to store the target FontGroup
    public int fontSizeForScaling = 16;

    [MenuItem("Window/Font Group Replacer")]
    public static void ShowWindow()
    {
        GetWindow<FontGroupReplacerWindow>("Font Group Replacer");
    }

    private void OnGUI()
    {
        targetFontGroup = (FontGroup)EditorGUILayout.ObjectField("Target Font Group", targetFontGroup, typeof(FontGroup), false);
        fontSizeForScaling = EditorGUILayout.IntField(fontSizeForScaling);

        if (GUILayout.Button("Replace Font Group"))
        {
            if (targetFontGroup != null)
            {
                ReplaceFontGroup();
            }
            else
            {
                Debug.LogError("Please assign a target Font Group.");
            }
        }
    }

    private void ReplaceFontGroup()
    {
        UIUnit[] uiUnits = Resources.FindObjectsOfTypeAll<UIUnit>();

        foreach (var uiUnit in uiUnits)
        {
            if (uiUnit.text != null || uiUnit.GetComponent<TextMeshProUGUI>() != null)
            {
                if(uiUnit.fontGroup == null){
                    uiUnit.fontGroup = targetFontGroup;
                    EditorUtility.SetDirty(uiUnit);
                    var fontSize = uiUnit.text == null ? uiUnit.GetComponent<TextMeshProUGUI>().fontSize : uiUnit.text.fontSize;
                    uiUnit.fontSizeScale = fontSize / fontSizeForScaling;
                }
                    
            }
        }

        Debug.Log("Font Group Replaced for eligible UIUnits.");
    }
}
