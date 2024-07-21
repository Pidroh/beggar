using HeartUnity.View;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeartUnity
{
    public class UnityLogIntegration
    {
        private EngineView engineView;
        public List<string> errorStrings = new();
        private UIUnit logText;
        private bool logActive;
        public int logShown;

        public UnityLogIntegration(EngineView engineView)
        {
            this.engineView = engineView;
            Application.logMessageReceived += LogReceived;
            var cl = engineView.creationHelper.ToMaxLayer();
            logText = engineView.creationHelper.InstantiateObject(engineView.reusableMenuPrefabs.textFullScreen);
            engineView.creationHelper.currentLayer = cl;

        }

        internal void ManualUpdate()
        {
            if (engineView.inputManager.IsButtonPressed(DefaultButtons.LEFT_TRIGGER_2) && engineView.inputManager.IsButtonDown(DefaultButtons.RIGHT_TRIGGER_2)) {
                logActive = !logActive;
            }
            logText.gameObject.SetActive(logActive);
            if (logActive)
            {
                if (engineView.inputManager.IsButtonDown(DefaultButtons.LEFT))
                {
                    logShown--;
                }
                if (engineView.inputManager.IsButtonDown(DefaultButtons.LEFT))
                {
                    logShown++;
                }
                if (errorStrings.Count == 0)
                {
                    logText.rawText = "No errors ";
                }
                else {
                    logShown = Mathf.Clamp(logShown, 0, errorStrings.Count - 1);
                    logText.rawText = errorStrings[logShown];
                }
                
                
            }
        }
        private void LogReceived(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    Log(condition, stackTrace);
                    break;
                case LogType.Assert:
                    break;
                case LogType.Warning:
                    break;
                case LogType.Log:
                    break;
                case LogType.Exception:
                    Log(condition, stackTrace);
                    break;
                default:
                    break;
            }

            void Log(string condition, string stackTrace)
            {
                errorStrings.Add($"{condition}\n\n{stackTrace}");
                logShown = errorStrings.Count - 1;
            }
        }
    }
    public class HeartGame
    {
        public SettingModel settingModel;

        private UnityLogIntegration _unityLogIntegration;
        private static CrossSceneData crossSceneDataStatic;
        public CrossSceneData crossSceneData;

        public static HeartGame Init()
        {
            var config = GetConfig();
            ReadLocalizationData(config);

            AudioPlayer.Init(config.musicList, config.audioList, config.voiceLists);
            var heartGame = new HeartGame();
            heartGame.settingModel = new SettingModel();
            heartGame.settingModel.Init(config.SettingData);
            heartGame.crossSceneData = crossSceneDataStatic;
            crossSceneDataStatic = default;
            return heartGame;
        }



        public static MainGameConfig GetConfig()
        {
            return Resources.Load<MainGameConfig>("MainGameConfig");
        }

        public static void ReadLocalizationData(MainGameConfig config = null)
        {
            if (config == null) config = Resources.Load<MainGameConfig>("MainGameConfig");
            Local.Instance.Init(config.localizationData.text);
        }

        public void BindEngineView(EngineView engineView)
        {
            _unityLogIntegration = new UnityLogIntegration(engineView);

        }

        public void ManualUpdate()
        {
            AudioPlayer.ManualUpdate();
            settingModel.ManualUpdate(Time.deltaTime);
            settingModel.CheckForDiscrepancies();
            _unityLogIntegration?.ManualUpdate();
        }

        internal void ChangeSceneFromSettings(string newScene, ReusableSettingMenu.SettingSceneMode settingSceneMode)
        {
            var sceneName = SceneManager.GetActiveScene().name;
            crossSceneDataStatic = new CrossSceneData() { 
                previousSceneName = sceneName,
                settingSceneMode = settingSceneMode
            };
            SceneManager.LoadScene(newScene);
        }

        public struct CrossSceneData 
        {
            public string previousSceneName;
            public ReusableSettingMenu.SettingSceneMode? settingSceneMode;
        }
    }

    // Define your ScriptableObject class
    [CreateAssetMenu(fileName = "MainGameConfig", menuName = "Custom/Main Game Config", order = 1)]
    public class MainGameConfig : ScriptableObject
    {
        public MusicDataList musicList;
        public AudioDataList audioList;
        public AudioDataList[] voiceLists;
        public TextAsset SettingData;
        public TextAsset Credits;   
        public TextAsset localizationData;
        public List<string> blacklistedLanguages;
        public View viewConfig;
        public InputPromptVisuals inputPromptVisuals;


        [Serializable]
        public class View
        {
            public CursorView cursorView;
            public MouseAsSpriteInfo mouseAsSprite;
            public PostProcessingScale bloomConfig;
        }
    }
}