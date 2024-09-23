using HeartUnity.View;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        public List<PersistenceUnit> PersistenceUnits;

        [Serializable]
        public class View
        {
            public CursorView cursorView;
            public MouseAsSpriteInfo mouseAsSprite;
            public PostProcessingScale bloomConfig;
        }

        [Serializable]
        public class PersistenceUnit 
        {
            public bool ForcePrefs;
            public string Key;
        }
    }
}