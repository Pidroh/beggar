using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ScrollManagementWindow : EditorWindow
{
    private int scrollSensitivity;
    private Color barBackgroundColor;
    private Color barHandleColor;

    // Define default colors
    private Color defaultBarBackgroundColor = Color.gray;
    private Color defaultBarHandleColor = Color.white;

    [MenuItem("Tools/Scroll Management")]
    public static void ShowWindow()
    {
        GetWindow<ScrollManagementWindow>("Scrolls");
    }

    private void OnEnable()
    {
        // Load settings or set default values
        scrollSensitivity = EditorPrefs.GetInt("ScrollSensitivity", 25);
        barBackgroundColor = GetColor("BarBackgroundColor", defaultBarBackgroundColor);
        barHandleColor = GetColor("BarHandleColor", defaultBarHandleColor);
    }

    private void OnGUI()
    {
        GUILayout.Label("Scroll Management", EditorStyles.boldLabel);

        scrollSensitivity = EditorGUILayout.IntSlider("Sensitivity", scrollSensitivity, 1, 100);

        barBackgroundColor = EditorGUILayout.ColorField("Background Color", barBackgroundColor);
        barHandleColor = EditorGUILayout.ColorField("Handle Color", barHandleColor);

        if (GUILayout.Button("Apply to All ScrollRects"))
        {
            ApplyScrollSettingsToAllScrollRects();
        }

        if (GUILayout.Button("Save Settings"))
        {
            // Save settings in the editor using EditorPrefs.
            EditorPrefs.SetInt("ScrollSensitivity", scrollSensitivity);
            SetColor("BarBackgroundColor", barBackgroundColor);
            SetColor("BarHandleColor", barHandleColor);
        }
    }

    private void ApplyScrollSettingsToAllScrollRects()
    {
        ScrollRect[] scrollRects = FindAllScrollRects();

        foreach (ScrollRect scrollRect in scrollRects)
        {
            scrollRect.scrollSensitivity = scrollSensitivity;
            UpdateScrollbarColors(scrollRect.verticalScrollbar, barBackgroundColor, barHandleColor);
            UpdateScrollbarColors(scrollRect.horizontalScrollbar, barBackgroundColor, barHandleColor);
        }
    }

    private void UpdateScrollbarColors(Scrollbar scrollbar, Color backgroundColor, Color handleColor)
    {
        if (scrollbar != null)
        {
            Image background = scrollbar.GetComponent<Image>();
            if (background != null)
            {
                background.color = backgroundColor;
            }

            Transform handle = scrollbar.transform.Find("Sliding Area/Handle");
            if (handle != null)
            {
                Image handleImage = handle.GetComponent<Image>();
                if (handleImage != null)
                {
                    handleImage.color = handleColor;
                }
            }
        }
    }

    private ScrollRect[] FindAllScrollRects()
    {
        // Find all ScrollRects, including those within inactive GameObjects.
        GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>();
        List<ScrollRect> allScrollRects = new List<ScrollRect>();

        foreach (GameObject go in allGameObjects)
        {
            ScrollRect[] scrollRectsInGameObject = go.GetComponentsInChildren<ScrollRect>(true);
            allScrollRects.AddRange(scrollRectsInGameObject);
        }

        return allScrollRects.ToArray();
    }

    // Custom method for storing color settings in EditorPrefs
    private void SetColor(string key, Color color)
    {
        EditorPrefs.SetFloat(key + "_r", color.r);
        EditorPrefs.SetFloat(key + "_g", color.g);
        EditorPrefs.SetFloat(key + "_b", color.b);
        EditorPrefs.SetFloat(key + "_a", color.a);
    }

    // Custom method for retrieving color settings from EditorPrefs
    private Color GetColor(string key, Color defaultValue)
    {
        float r = EditorPrefs.GetFloat(key + "_r", defaultValue.r);
        float g = EditorPrefs.GetFloat(key + "_g", defaultValue.g);
        float b = EditorPrefs.GetFloat(key + "_b", defaultValue.b);
        float a = EditorPrefs.GetFloat(key + "_a", defaultValue.a);

        return new Color(r, g, b, a);
    }
}
