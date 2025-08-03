#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using HeartUnity;

namespace BeggarEditor
{
    public static class ScreenshotScriptMenu
    {
        public static ScreenshotScriptRunner currentRunner;

        [MenuItem("Tools/Beggar/Execute Screenshot Scripts")]
        public static void ExecuteScreenshotScripts()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[ScreenshotScript] This command only works in Play Mode!");
                return;
            }

            if (currentRunner != null)
            {
                Debug.LogWarning("[ScreenshotScript] Screenshot scripts are already running!");
                return;
            }

            // Create a runner GameObject in the scene
            var runnerObject = new GameObject("ScreenshotScriptRunner");
            currentRunner = runnerObject.AddComponent<ScreenshotScriptRunner>();
            currentRunner.StartExecution();
        }

    }

    // MonoBehaviour class to handle coroutines in play mode
    public class ScreenshotScriptRunner : MonoBehaviour
    {
        private GameObject executorObject;

        public void StartExecution()
        {
            StartCoroutine(ExecuteAllScripts());
        }

        private IEnumerator ExecuteAllScripts()
        {
            Debug.Log("[ScreenshotScript] Starting screenshot script execution for all languages...");

            // Load the script file
            string scriptPath = Path.Combine(Application.dataPath, "editor/data/screenshot_script.txt");
            if (!File.Exists(scriptPath))
            {
                Debug.LogError($"[ScreenshotScript] Script file not found: {scriptPath}");
                Cleanup();
                yield break;
            }

            string scriptText = File.ReadAllText(scriptPath);
            var script = ScreenshotScript.Parse(scriptText);

            if (script.Commands.Count == 0)
            {
                Debug.LogError("[ScreenshotScript] No valid commands found in script!");
                Cleanup();
                yield break;
            }

            // Get all available languages
            var languages = GetAvailableLanguages();
            if (languages.Count == 0)
            {
                Debug.LogError("[ScreenshotScript] No languages available!");
                Cleanup();
                yield break;
            }

            Debug.Log($"[ScreenshotScript] Found {languages.Count} languages: {string.Join(", ", languages)}");

            // Create executor object
            executorObject = new GameObject("ScreenshotScriptExecutor");
            var executor = executorObject.AddComponent<ScreenshotScriptExecutor>();

            // Execute script for each language
            foreach (var language in languages)
            {
                Debug.Log($"[ScreenshotScript] Processing language: {language}");

                // Change language
                yield return ChangeLanguage(language);

                // Wait for scene to stabilize
                yield return new WaitForSeconds(2f);

                // Execute the script
                executor.ExecuteScript(script, language);

                // Wait for script execution to complete
                while (IsExecutorBusy(executor))
                {
                    yield return new WaitForSeconds(0.5f);
                }

                Debug.Log($"[ScreenshotScript] Completed language: {language}");
                yield return new WaitForSeconds(1f);
            }

            Debug.Log("[ScreenshotScript] All screenshot scripts completed!");
            Cleanup();
        }

        private void Cleanup()
        {
            // Cleanup executor
            if (executorObject != null)
            {
                Destroy(executorObject);
            }

            // Clear static reference
            ScreenshotScriptMenu.currentRunner = null;

            // Destroy this runner
            Destroy(gameObject);
        }

        private List<string> GetAvailableLanguages()
        {
            var languages = new List<string>();

            // Check if Local instance exists and has languages
            if (Local.Instance != null && Local.Instance.languages != null)
            {
                var config = HeartGame.GetConfig();
                foreach (var lang in Local.Instance.languages)
                {
                    if (config != null && config.blacklistedLanguages != null && 
                        config.blacklistedLanguages.Contains(lang.languageName))
                    {
                        continue;
                    }
                    languages.Add(lang.languageName);
                }
            }

            // If no languages found, try to read from localization data
            if (languages.Count == 0)
            {
                var config = HeartGame.GetConfig();
                if (config != null && config.localizationData != null)
                {
                    // Parse localization data to find languages
                    var lines = config.localizationData.text.Split('\n');
                    if (lines.Length > 0)
                    {
                        var headers = lines[0].Split('$');
                        for (int i = 0; i < headers.Length; i++)
                        {
                            var header = headers[i].Trim();
                            if (header.ToLower() != "key" && header.ToLower() != "description")
                            {
                                languages.Add(header);
                            }
                        }
                    }
                }
            }

            return languages;
        }

        private IEnumerator ChangeLanguage(string language)
        {
            Debug.Log($"[ScreenshotScript] Changing language to: {language}");

            // Change the language
            Local.ChangeLanguage(language);

            // Find MainGameControl and check if we need to reload
            var mainGameControl = GameObject.FindObjectOfType<MainGameControl>();
            if (mainGameControl != null)
            {
                // Reload the scene to apply language change
                var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
                
                // Wait for scene to load
                yield return new WaitForSeconds(2f);
            }
        }

        private bool IsExecutorBusy(ScreenshotScriptExecutor executor)
        {
            if (executor == null) return false;

            // Use reflection to check if executor is still running
            var field = executor.GetType().GetField("isExecuting", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return (bool)field.GetValue(executor);
            }

            return false;
        }
    }
}
#endif