using UnityEditor;
using System.IO;
using UnityEngine;


public class FileCopyTool : EditorWindow
{
    [MenuItem("Tools/Copy Files")]
    public static void ShowWindow()
    {
        GetWindow(typeof(FileCopyTool));
    }

    private string sourceFolder;
    private string destinationFolder;
    private bool pixelPerfect;

    private const string SourceFolderKey = "LastUsedSourceFolder";
    private const string DestinationFolderKey = "LastUsedDestinationFolder";
    private const string PixelPerfecstring = "PixelPerfect";

    void OnEnable()
    {
        sourceFolder = EditorPrefs.GetString(SourceFolderKey, "SourceFolder");
        destinationFolder = EditorPrefs.GetString(DestinationFolderKey, "DestinationFolder");
        pixelPerfect = EditorPrefs.GetBool(PixelPerfecstring, false);
    }

    void OnGUI()
    {
        GUILayout.Label("File Copy Tool", EditorStyles.boldLabel);

        sourceFolder = EditorGUILayout.TextField("Source Folder (Relative to Assets)", sourceFolder);
        destinationFolder = EditorGUILayout.TextField("Destination Folder (Relative to Assets)", destinationFolder);

        pixelPerfect = EditorGUILayout.Toggle("Pixel Perfect", pixelPerfect);

        if (GUILayout.Button("Copy Files"))
        {
            CopyFiles(sourceFolder, destinationFolder, pixelPerfect);
        }
    }

    private void CopyFiles(string source, string destination, bool usePixelPerfect)
    {
        string sourcePath = Application.dataPath + "/" + source;
        string destPath = Application.dataPath + "/" + destination;

        Debug.Log("Source Path: " + sourcePath);
        Debug.Log("Destination Path: " + destPath);

        if (!Directory.Exists(sourcePath) || !Directory.Exists(destPath))
        {
            Debug.LogError("Source or destination folder does not exist.");
            return;
        }

        string[] files = Directory.GetFiles(sourcePath);

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string destFilePath = destPath + "/" + fileName;

            File.Copy(file, destFilePath, true);
        }

        if (usePixelPerfect)
        {
            // Check for texture files in the destination folder and change their filter mode to Point (no filter)
            string[] textureFiles = Directory.GetFiles(destPath, "*.png");
            foreach (string textureFile in textureFiles)
            {
                TextureImporter importer = AssetImporter.GetAtPath(textureFile) as TextureImporter;
                if (importer != null)
                {
                    importer.filterMode = FilterMode.Point;
                    AssetDatabase.ImportAsset(textureFile);
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Files copied successfully!");

        // Save the last used source and destination folders and the "pixel perfect" setting
        EditorPrefs.SetString(SourceFolderKey, source);
        EditorPrefs.SetString(DestinationFolderKey, destination);
        EditorPrefs.SetBool(PixelPerfecstring, usePixelPerfect);
    }
}
