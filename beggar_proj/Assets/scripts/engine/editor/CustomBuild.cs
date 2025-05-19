
using HeartUnity;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;


public class CustomBuild
{
    public static void BuildGameCommandLine()
    {
        // Access command-line arguments
        string[] args = System.Environment.GetCommandLineArgs();


        string buildConfigTag = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-buildConfig" && i + 1 < args.Length)
            {
                buildConfigTag = args[i + 1];
            }
        }

        BuildWithTag(buildConfigTag);

    }

    public static void BuildWithTag(string buildConfigTag)
    {
        string[] configurationPaths = AssetDatabase.FindAssets("t:FileBuildConfigurations");
        foreach (var configurationPath in configurationPaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(configurationPath);
            var configuration = AssetDatabase.LoadAssetAtPath<FileBuildConfigurations>(path);
            foreach (var entry in configuration.entries)
            {
                if (entry.tag != buildConfigTag) continue;
                BuildGameEntry(entry);
                return;
            }
        }
    }

    public static void BuildGameEntry(FileBuildConfigurations.Entry entry)
    {
        var copyFileConfig = entry.copyFileTag;
        var outputPath = entry.outputPath;
        if (outputPath.Contains("%V%")) 
        {
            var config = HeartGame.GetConfig();
            outputPath = outputPath.Replace("%V%", config.versionNumber.ToString("D4"));
        }
        if (outputPath.Contains("%BETA%"))
        {
            var config = HeartGame.GetConfig();
            outputPath = outputPath.Replace("%BETA%", config.betaVersion ? "_beta" : "");
            
            
        }

        if (!string.IsNullOrWhiteSpace(copyFileConfig))
        {
            
            var result = FileReplaceWindow.ReplaceFilesForTag(copyFileConfig);
            Debug.Log(result ? "Successful replace!" : "Failed to replace");
        }
        else
        {
            Debug.Log("COPY FILE CONFIG IS NULL");
        }
        // Define the build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainScene.unity", "Assets/Reusable/SettingsMenu.unity" }, // Adjust the scenes array as necessary
            locationPathName = outputPath,
            target = entry.buildTarget,
            options = BuildOptions.None
        };

        // Perform the build
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            if (entry.buildTarget != BuildTarget.WebGL)
            {
                ExecuteCommand($"7z a -r -tzip \"../{entry.tag}.zip\" ./{Path.GetDirectoryName(outputPath)}/*");
            }

        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }

    // Method to execute a command string
    public static void ExecuteCommand(string command)
    {
        System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
        processStartInfo.FileName = "cmd.exe";
        processStartInfo.Arguments = $"/c {command}";
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardError = true;
        processStartInfo.UseShellExecute = false;
        processStartInfo.CreateNoWindow = true;

        using (System.Diagnostics.Process process = new System.Diagnostics.Process())
        {
            process.StartInfo = processStartInfo;
            process.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log(args.Data);
            process.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError(args.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }
}
