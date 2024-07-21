using UnityEngine;
using UnityEditor;
using System.IO;

public partial class PersistentDataMenu
{
    [MenuItem("Tools/Open Persistent Data Path")]
    public static void OpenPersistentDataPath()
    {
        string dataPath = Application.persistentDataPath;
        EditorUtility.OpenWithDefaultApp(dataPath); // or EditorUtility.OpenInFileViewer(dataPath) on macOS
    }

    [MenuItem("Tools/Clear Persistent Data Path & Reload #d")]
    public static void ClearDataAndReloadScene()
    {
        // Clear all files in the persistent data path
        string dataPath = Application.persistentDataPath;
        string[] files = Directory.GetFiles(dataPath);
        foreach (string file in files)
        {
            File.Delete(file);
        }
        PlayerPrefs.DeleteAll();

        // Reload the current scene
        UnityEditor.SceneManagement.EditorSceneManager.LoadScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name);
    }

    [MenuItem("Tools/Reload #r")]
    public static void ReloadScene()
    {
        // Reload the current scene
        UnityEditor.SceneManagement.EditorSceneManager.LoadScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name);
    }

}