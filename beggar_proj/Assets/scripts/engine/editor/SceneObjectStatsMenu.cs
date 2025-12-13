using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Adds a menu entry to quickly inspect scene object counts.
/// </summary>
public static class SceneObjectStatsMenu
{
    [MenuItem("Tools/Heart Engine/Scene Object Stats")]
    public static void ShowSceneObjectStats()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog("Scene Object Stats", "No active scene is loaded.", "OK");
            return;
        }

        int totalObjects = CountAllObjects(scene);
        int canvasObjects = CountObjectsUnderCanvases(scene);
        int textComponents = CountComponentsInScene<TMP_Text>(scene);
        int imageComponents = CountComponentsInScene<Image>(scene);

        string message =
            $"Scene: {scene.name}\n" +
            $"Total objects (recursive): {totalObjects}\n" +
            $"Objects under a Canvas (recursive): {canvasObjects}\n" +
            $"Objects with TMP_Text: {textComponents}\n" +
            $"Objects with Image: {imageComponents}";

        EditorUtility.DisplayDialog("Scene Object Stats", message, "OK");
    }

    private static int CountAllObjects(Scene scene)
    {
        int count = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            // Transforms include the root itself, giving us a full recursive count.
            count += root.GetComponentsInChildren<Transform>(true).Length;
        }
        return count;
    }

    private static int CountObjectsUnderCanvases(Scene scene)
    {
        var seen = new HashSet<int>();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
                {
                    seen.Add(t.gameObject.GetInstanceID());
                }
            }
        }
        return seen.Count;
    }

    private static int CountComponentsInScene<T>(Scene scene) where T : Component
    {
        int count = 0;
        foreach (var component in Object.FindObjectsOfType<T>(true))
        {
            if (component.gameObject.scene == scene)
            {
                count++;
            }
        }
        return count;
    }
}
