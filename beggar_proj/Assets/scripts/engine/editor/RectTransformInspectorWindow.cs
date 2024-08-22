using UnityEngine;
using UnityEditor;

public class RectTransformInspectorWindow : EditorWindow
{
    [MenuItem("Window/RectTransform Inspector")]
    public static void ShowWindow()
    {
        GetWindow<RectTransformInspectorWindow>("RectTransform Inspector");
    }

    private void OnGUI()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject != null)
        {
            RectTransform rectTransform = selectedObject.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                EditorGUILayout.LabelField("Selected GameObject: " + selectedObject.name);

                EditorGUILayout.Space();

                // Display position
                EditorGUILayout.LabelField("Position:");
                rectTransform.position = EditorGUILayout.Vector2Field("", rectTransform.position);

                EditorGUILayout.Space();

                // Display local position
                EditorGUILayout.LabelField("Local Position:");
                rectTransform.localPosition = EditorGUILayout.Vector2Field("", rectTransform.localPosition);

                EditorGUILayout.Space();

                // Display size delta
                EditorGUILayout.LabelField("Size Delta:");
                rectTransform.sizeDelta = EditorGUILayout.Vector2Field("", rectTransform.sizeDelta);

                EditorGUILayout.Space();

                
                EditorGUILayout.LabelField("Anchor Min:");
                rectTransform.anchorMin = EditorGUILayout.Vector2Field("", rectTransform.anchorMin);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Anchor Max:");
                rectTransform.anchorMax = EditorGUILayout.Vector2Field("", rectTransform.anchorMax);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Offset min:");
                rectTransform.offsetMin = EditorGUILayout.Vector2Field("", rectTransform.offsetMin);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Offset max:");
                rectTransform.offsetMax = EditorGUILayout.Vector2Field("", rectTransform.offsetMax);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Anchored position:");
                rectTransform.anchoredPosition = EditorGUILayout.Vector2Field("", rectTransform.anchoredPosition);

                EditorGUILayout.Space();

                // Display RectTransform rect information
                EditorGUILayout.LabelField("RectTransform Rect Information:");
                EditorGUILayout.LabelField("Position: " + rectTransform.rect.position);
                EditorGUILayout.LabelField("Size: " + rectTransform.rect.size);
            }
            else
            {
                EditorGUILayout.LabelField("Selected GameObject does not have a RectTransform component.");
            }
        }
        else
        {
            EditorGUILayout.LabelField("No GameObject selected.");
        }
    }
}
