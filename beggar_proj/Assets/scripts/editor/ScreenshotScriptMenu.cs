#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using HeartUnity;
using System.Linq;
using JLayout;

namespace BeggarEditor
{
    public static class ScreenshotScriptMenu
    {
        private static ScreenshotScriptStateMachine stateMachine;

        [MenuItem("Tools/Beggar/Execute Screenshot Scripts")]
        public static void ExecuteScreenshotScripts()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[ScreenshotScript] This command only works in Play Mode!");
                return;
            }

            if (stateMachine != null)
            {
                Debug.LogWarning("[ScreenshotScript] Screenshot scripts are already running!");
                return;
            }

            // Initialize and start the state machine
            stateMachine = new ScreenshotScriptStateMachine();
            stateMachine.Start();
            
            // Register update callback
            EditorApplication.update += UpdateStateMachine;
        }

        [MenuItem("Tools/Beggar/Stop Screenshot Scripts")]
        public static void StopScreenshotScripts()
        {
            if (stateMachine != null)
            {
                EditorApplication.update -= UpdateStateMachine;
                stateMachine = null;
                Debug.Log("[ScreenshotScript] Screenshot script manually stopped");
            }
            else
            {
                Debug.Log("[ScreenshotScript] No screenshot script is currently running");
            }
        }

        private static void UpdateStateMachine()
        {
            // Stop if we exit play mode
            if (!Application.isPlaying)
            {
                EditorApplication.update -= UpdateStateMachine;
                stateMachine = null;
                Debug.Log("[ScreenshotScript] Screenshot script stopped - Play Mode ended");
                return;
            }

            if (stateMachine == null)
            {
                EditorApplication.update -= UpdateStateMachine;
                return;
            }

            stateMachine.Update();

            if (stateMachine.IsComplete)
            {
                EditorApplication.update -= UpdateStateMachine;
                stateMachine = null;
                Debug.Log("[ScreenshotScript] All screenshot scripts completed!");
            }
        }
    }

    // State machine to handle screenshot script execution without coroutines
    public class ScreenshotScriptStateMachine
    {
        private enum State
        {
            Init,
            LoadingScript,
            ChangingLanguage,
            WaitingForSceneLoad,
            ExecutingCommand,
            WaitingForCommand,
            Complete
        }

        private State currentState = State.Init;
        private float waitTimer = 0f;
        private ScreenshotScript script;
        private List<string> languages;
        private int currentLanguageIndex = 0;
        private Queue<ScreenshotCommand> commandQueue;
        private ScreenshotCommand currentCommand;
        private string currentLanguage;
        private MainGameControl mainGameControl;

        public bool IsComplete => currentState == State.Complete;

        public void Start()
        {
            currentState = State.Init;
        }

        public void Update()
        {
            // Handle wait timers
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
                return;
            }

            switch (currentState)
            {
                case State.Init:
                    Initialize();
                    break;

                case State.LoadingScript:
                    LoadScript();
                    break;

                case State.ChangingLanguage:
                    ChangeLanguage();
                    break;

                case State.WaitingForSceneLoad:
                    CheckSceneLoaded();
                    break;

                case State.ExecutingCommand:
                    ExecuteCurrentCommand();
                    break;

                case State.WaitingForCommand:
                    CheckCommandComplete();
                    break;
            }
        }

        private void Initialize()
        {
            Debug.Log("[ScreenshotScript] Starting screenshot script execution for all languages...");

            // Load the script file
            string scriptPath = Path.Combine(Application.dataPath, "editor/data/screenshot_script.txt");
            if (!File.Exists(scriptPath))
            {
                Debug.LogError($"[ScreenshotScript] Script file not found: {scriptPath}");
                currentState = State.Complete;
                return;
            }

            string scriptText = File.ReadAllText(scriptPath);
            script = ScreenshotScript.Parse(scriptText);

            if (script.Commands.Count == 0)
            {
                Debug.LogError("[ScreenshotScript] No valid commands found in script!");
                currentState = State.Complete;
                return;
            }

            // Get all available languages
            languages = GetAvailableLanguages();
            if (languages.Count == 0)
            {
                Debug.LogError("[ScreenshotScript] No languages available!");
                currentState = State.Complete;
                return;
            }

            Debug.Log($"[ScreenshotScript] Found {languages.Count} languages: {string.Join(", ", languages)}");
            
            currentLanguageIndex = 0;
            currentState = State.ChangingLanguage;
        }

        private void LoadScript()
        {
            // This state is not used in current implementation
        }

        private void ChangeLanguage()
        {
            if (currentLanguageIndex >= languages.Count)
            {
                currentState = State.Complete;
                return;
            }

            currentLanguage = languages[currentLanguageIndex];
            Debug.Log($"[ScreenshotScript] Processing language: {currentLanguage}");

            // Change the language
            Local.ChangeLanguage(currentLanguage);

            // Reload the scene to apply language change
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
            
            waitTimer = 2f; // Wait for scene load
            currentState = State.WaitingForSceneLoad;
        }

        private void CheckSceneLoaded()
        {
            // Reset command queue for this language
            commandQueue = new Queue<ScreenshotCommand>(script.Commands);
            currentCommand = null;
            
            currentState = State.ExecutingCommand;
        }

        private void ExecuteCurrentCommand()
        {
            if (currentCommand == null && commandQueue.Count > 0)
            {
                currentCommand = commandQueue.Dequeue();
                Debug.Log($"[ScreenshotScript] Executing command: {currentCommand.Type}");
            }
            else if (currentCommand == null && commandQueue.Count == 0)
            {
                // Done with this language
                Debug.Log($"[ScreenshotScript] Completed language: {currentLanguage}");
                currentLanguageIndex++;
                waitTimer = 1f;
                currentState = State.ChangingLanguage;
                return;
            }

            // Find MainGameControl if needed
            if (mainGameControl == null)
            {
                mainGameControl = GameObject.FindObjectOfType<MainGameControl>();
            }

            bool commandStarted = false;

            switch (currentCommand.Type)
            {
                case ScreenshotCommand.CommandType.LOAD_SAVE:
                    commandStarted = ExecuteLoadSave();
                    break;

                case ScreenshotCommand.CommandType.START_GAME:
                    commandStarted = ExecuteStartGame();
                    break;

                case ScreenshotCommand.CommandType.EXPAND:
                    if (currentCommand.Parameters.TryGetValue("id", out string expandId))
                    {
                        commandStarted = ExecuteExpand(expandId);
                    }
                    break;

                case ScreenshotCommand.CommandType.SCREENSHOT:
                    if (currentCommand.Parameters.TryGetValue("name", out string screenshotName))
                    {
                        commandStarted = ExecuteScreenshot(screenshotName);
                    }
                    break;

                case ScreenshotCommand.CommandType.WAIT:
                    waitTimer = currentCommand.WaitTime;
                    commandStarted = true;
                    break;

                case ScreenshotCommand.CommandType.ACTIVATE_TAB:
                    if (currentCommand.Parameters.TryGetValue("id", out string tabId))
                    {
                        commandStarted = ExecuteActivateTab(tabId);
                    }
                    break;
            }

            if (commandStarted)
            {
                currentState = State.WaitingForCommand;
            }
            else
            {
                // Command failed, move to next
                currentCommand = null;
            }
        }

        private void CheckCommandComplete()
        {
            // Most commands complete instantly
            currentCommand = null;
            currentState = State.ExecutingCommand;
        }

        private bool ExecuteLoadSave()
        {
            // Load the save file
            string savePath = Path.Combine(Application.dataPath, "editor/data/save_for_screenshots.hg");
            if (File.Exists(savePath))
            {
                byte[] saveData = File.ReadAllBytes(savePath);
                
                // Extract save data
                var names = new List<string>();
                var content = new List<string>();
                ZipUtilities.ExtractZipFromBytes(saveData, names, content);
                SaveDataCenter.ImportSave(names, content);
                
                Debug.Log("[ScreenshotScript] Save loaded successfully");
                
                // Reload scene to apply save
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                waitTimer = 2f;
                return true;
            }
            else
            {
                Debug.LogError($"[ScreenshotScript] Save file not found: {savePath}");
                return false;
            }
        }

        private bool ExecuteStartGame()
        {
            if (mainGameControl != null)
            {
                var titleScreenData = mainGameControl.GetType()
                    .GetField("titleScreenData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(mainGameControl) as TitleScreenRuntimeData;

                if (titleScreenData != null)
                {
                    // Find and click the play button
                    foreach (var buttonInfo in titleScreenData.TitleButtonsJCUs)
                    {
                        if (buttonInfo.Item1 == TitleScreenRuntimeData.TitleButtons.PLAY_GAME)
                        {
                            var button = buttonInfo.Item2.MainExecuteButton;
                            if (button != null)
                            {
                                // Simulate button click - using the approach from the modified executor
                                button.buttonOwner.ButtonChildren[button.index].Item2.UiUnit.ForceClick();
                                Debug.Log("[ScreenshotScript] Started game from title screen");
                                waitTimer = 3f; // Wait for game to load
                                return true;
                            }
                        }
                    }
                }
            }
            
            Debug.LogWarning("[ScreenshotScript] Could not find start game button");
            return false;
        }

        private bool ExecuteExpand(string id)
        {
            if (mainGameControl?.JControlData == null) return false;

            // Search through all tabs and separators for the unit with matching ID
            foreach (var tabControl in mainGameControl.JControlData.TabControlUnits)
            {
                foreach (var separatorControl in tabControl.SeparatorControls)
                {
                    foreach (var unitList in separatorControl.UnitGroupControls.Values)
                    {
                        foreach (var jcu in unitList)
                        {
                            if (jcu.Data?.ConfigBasic?.Id == id)
                            {
                                // Expand the unit
                                if (jcu.ExpandButton != null && !jcu.Expanded)
                                {
                                    jcu.ExpandButton.buttonOwner.ButtonChildren[jcu.ExpandButton.index].Item2.UiUnit.ForceClick();
                                    Debug.Log($"[ScreenshotScript] Expanded unit: {id}");
                                    waitTimer = 0.5f;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            Debug.LogWarning($"[ScreenshotScript] Could not find unit to expand: {id}");
            return false;
        }

        private bool ExecuteScreenshot(string name)
        {
            // Save screenshots to beggar_proj/Recordings
            string outputDir = Path.Combine(Application.dataPath, "../Recordings");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string fileName = $"{name}_{currentLanguage}.png";
            string outputPath = Path.Combine(outputDir, fileName);

            // Take screenshot
            ScreenCapture.CaptureScreenshot(outputPath);
            
            Debug.Log($"[ScreenshotScript] Screenshot saved: {outputPath}");
            waitTimer = 0.1f;
            return true;
        }

        private bool ExecuteActivateTab(string tabId)
        {
            if (mainGameControl?.JControlData == null) return false;

            // Find the tab with matching ID
            for (int i = 0; i < mainGameControl.JControlData.TabControlUnits.Count; i++)
            {
                var tabControl = mainGameControl.JControlData.TabControlUnits[i];
                if (tabControl.TabData?.ConfigBasic?.Id == tabId)
                {
                    // Click the tab button (desktop version)
                    if (tabControl.DesktopButton != null)
                    {
                        tabControl.DesktopButton.SelfUIUnit.ForceClick();
                        Debug.Log($"[ScreenshotScript] Activated tab: {tabId}");
                        waitTimer = 0.5f;
                        return true;
                        
                    }
                }
            }

            Debug.LogWarning($"[ScreenshotScript] Could not find tab: {tabId}");
            return false;
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