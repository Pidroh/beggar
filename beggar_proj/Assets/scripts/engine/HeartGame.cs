using HeartUnity.View;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeartUnity
{
    public class HeartGame
    {
        public SettingModel settingModel;

        private UnityLogIntegration _unityLogIntegration;
        private static CrossSceneData crossSceneDataStatic;
        public CrossSceneData crossSceneData;

        public static bool MousePlatform => !Application.isConsolePlatform && Application.isMobilePlatform;

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
            if(config.localizationData != null) Local.Instance.Init(config.localizationData.text);
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
}