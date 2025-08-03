#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using HeartUnity;
using JLayout;

namespace BeggarEditor
{
    public class ScreenshotScriptExecutor : MonoBehaviour
    {
        private Queue<ScreenshotCommand> commandQueue = new Queue<ScreenshotCommand>();
        private bool isExecuting = false;
        private string currentLanguage;
        private MainGameControl mainGameControl;
        private float waitTimer = 0f;
        private ScreenshotCommand currentCommand;

        private RecorderController recorderController;
        private RecorderControllerSettings controllerSettings;
        private ImageRecorderSettings imageSettings;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SetupRecorder();
        }

        private void SetupRecorder()
        {
            controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            recorderController = new RecorderController(controllerSettings);

            imageSettings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            imageSettings.name = "Screenshot Recorder";
            imageSettings.Enabled = true;
            imageSettings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            imageSettings.CaptureAlpha = true;

            // Setup game view input
            imageSettings.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = 1920,
                OutputHeight = 1080
            };

            controllerSettings.AddRecorderSettings(imageSettings);
            controllerSettings.SetRecordModeToSingleFrame(0);
        }

        public void ExecuteScript(ScreenshotScript script, string language)
        {
            if (isExecuting) return;

            currentLanguage = language;
            commandQueue.Clear();
            
            foreach (var command in script.Commands)
            {
                commandQueue.Enqueue(command);
            }

            isExecuting = true;
            StartCoroutine(ExecuteCommands());
        }

        private IEnumerator ExecuteCommands()
        {
            while (commandQueue.Count > 0 || currentCommand != null)
            {
                if (currentCommand == null && commandQueue.Count > 0)
                {
                    currentCommand = commandQueue.Dequeue();
                    Debug.Log($"[ScreenshotScript] Executing command: {currentCommand.Type}");
                }

                if (currentCommand != null)
                {
                    yield return ExecuteCommand(currentCommand);
                    currentCommand = null;
                }

                yield return null;
            }

            isExecuting = false;
            Debug.Log($"[ScreenshotScript] Script execution completed for language: {currentLanguage}");
        }

        private IEnumerator ExecuteCommand(ScreenshotCommand command)
        {
            // Find MainGameControl if needed
            if (mainGameControl == null)
            {
                mainGameControl = FindObjectOfType<MainGameControl>();
                if (mainGameControl == null && command.Type != ScreenshotCommand.CommandType.LOAD_SAVE)
                {
                    Debug.LogError("[ScreenshotScript] MainGameControl not found!");
                    yield break;
                }
            }

            switch (command.Type)
            {
                case ScreenshotCommand.CommandType.LOAD_SAVE:
                    yield return ExecuteLoadSave();
                    break;

                case ScreenshotCommand.CommandType.START_GAME:
                    yield return ExecuteStartGame();
                    break;

                case ScreenshotCommand.CommandType.EXPAND:
                    if (command.Parameters.TryGetValue("id", out string expandId))
                    {
                        yield return ExecuteExpand(expandId);
                    }
                    break;

                case ScreenshotCommand.CommandType.SCREENSHOT:
                    if (command.Parameters.TryGetValue("name", out string screenshotName))
                    {
                        yield return ExecuteScreenshot(screenshotName);
                    }
                    break;

                case ScreenshotCommand.CommandType.WAIT:
                    yield return new WaitForSeconds(command.WaitTime);
                    break;

                case ScreenshotCommand.CommandType.ACTIVATE_TAB:
                    if (command.Parameters.TryGetValue("id", out string tabId))
                    {
                        yield return ExecuteActivateTab(tabId);
                    }
                    break;
            }
        }

        private IEnumerator ExecuteLoadSave()
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
                
                // Wait for save to be processed
                yield return new WaitForSeconds(0.5f);
                
                // Reload scene to apply save
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                yield return new WaitForSeconds(2f);
            }
            else
            {
                Debug.LogError($"[ScreenshotScript] Save file not found: {savePath}");
            }
        }

        private IEnumerator ExecuteStartGame()
        {
            // Find title screen setup or loading screen
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
                                // Simulate button click
                                button.buttonOwner.ButtonChildren[button.index].Item2.UiUnit.ForceClick();
                                //button.GetButton().onClick.Invoke();
                                Debug.Log("[ScreenshotScript] Started game from title screen");
                                yield return new WaitForSeconds(3f); // Wait for game to load
                                yield break;
                            }
                        }
                    }
                }
            }
            
            Debug.LogWarning("[ScreenshotScript] Could not find start game button");
        }

        private IEnumerator ExecuteExpand(string id)
        {
            if (mainGameControl?.JControlData == null) yield break;

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
                                    yield return new WaitForSeconds(0.5f);
                                    yield break;
                                }
                            }
                        }
                    }
                }
            }

            Debug.LogWarning($"[ScreenshotScript] Could not find unit to expand: {id}");
        }

        private IEnumerator ExecuteScreenshot(string name)
        {
            // Setup output path with language
            string outputDir = Path.Combine(Application.dataPath, "../Screenshots", currentLanguage);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string fileName = $"{name}_{currentLanguage}.png";
            string outputPath = Path.Combine(outputDir, fileName);

            // Configure recorder output
            imageSettings.OutputFile = outputPath;

            // Take screenshot
            recorderController.PrepareRecording();
            recorderController.StartRecording();
            
            yield return new WaitForEndOfFrame();
            
            recorderController.StopRecording();
            
            Debug.Log($"[ScreenshotScript] Screenshot saved: {outputPath}");
            yield return new WaitForSeconds(0.1f);
        }

        private IEnumerator ExecuteActivateTab(string tabId)
        {
            if (mainGameControl?.JControlData == null) yield break;

            // Find the tab with matching ID
            for (int i = 0; i < mainGameControl.JControlData.TabControlUnits.Count; i++)
            {
                var tabControl = mainGameControl.JControlData.TabControlUnits[i];
                if (tabControl.TabData?.ConfigBasic?.Id == tabId)
                {
                    // Click the tab button (desktop version)
                    if (tabControl.DesktopButton != null)
                    {
                        var button = tabControl.DesktopButton.ButtonChildren[0];
                        if (button.Item2 != null)
                        {
                            button.Item2.UiUnit.ForceClick();
                            Debug.Log($"[ScreenshotScript] Activated tab: {tabId}");
                            yield return new WaitForSeconds(0.5f);
                            yield break;
                        }
                    }
                }
            }

            Debug.LogWarning($"[ScreenshotScript] Could not find tab: {tabId}");
        }

        private void OnDestroy()
        {
            if (recorderController != null)
            {
                recorderController.StopRecording();
            }
        }
    }
}
#endif