using HeartUnity.View;
#if UNITY_SWITCH
using nn.account;
#endif
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

        public EngineView EngineView { get; private set; }

        public PlayTimeControl PlayTimeControl = new PlayTimeControl();

        public static HeartGame Init()
        {
            var config = GetConfig();
            ReadLocalizationData(config);

            AudioPlayer.Init(config.musicList, config.audioList, config.voiceLists);
            var heartGame = new HeartGame();
            heartGame.crossSceneData = crossSceneDataStatic;
            crossSceneDataStatic = default;
#if UNITY_SWITCH && !UNITY_EDITOR
            TryLoadSwitchUser(heartGame);
#endif
            heartGame.settingModel = new SettingModel();
            heartGame.settingModel.Init(config.SettingData, heartGame);
            

            
            return heartGame;
        }

        public void CommonDataLoad()
        {
            CommonPlayerSaveDataPersistence commonPlayerSaveDataPersistence = HeartGame.CreateCommonPlayerSaveDataPersistence("player_commons");
            if (commonPlayerSaveDataPersistence.TryLoad(out var playerSave))
            {
                PlayTimeControl.Init(playerSave);
            }
        }

#if UNITY_SWITCH
        private static void TryLoadSwitchUser(HeartGame heartGame)
        {
            Debug.Log($"NSSL Try Load User {heartGame.crossSceneData.UserId} | {default(Uid)}");
            if (heartGame.crossSceneData.UserId == default)
            {
                nn.account.Account.Initialize();
                Debug.Log("NSSL Account Init");
                nn.account.UserHandle userHandle = new nn.account.UserHandle();
                Debug.Log("NSSL before open selected user");
                if (!nn.account.Account.TryOpenPreselectedUser(ref userHandle))
                {
                    Debug.Log("NSSL before abort call");
                    nn.Nn.Abort("Failed to open preselected user.");
                }
                Debug.Log("NSSL before user id");
                nn.account.Uid id = default;
                nn.Result result = nn.account.Account.GetUserId(ref id, userHandle);
                Debug.Log("NSSL Try abort on user id");
                result.abortUnlessSuccess();
                heartGame.crossSceneData.UserId = id;
                result = nn.fs.SaveData.Mount(NintendoSwitchPersistentTextUnit.MountName, id);
                Debug.Log("NSSL Try abort on mounting");
                result.abortUnlessSuccess();
            }
        }
#endif

        public static MainGameConfig GetConfig()
        {
            return Resources.Load<MainGameConfig>("MainGameConfig");
        }

        public static void ReadLocalizationData(MainGameConfig config = null)
        {
            if (config == null) config = Resources.Load<MainGameConfig>("MainGameConfig");
            if (config.localizationData != null) Local.Instance.Init(config.localizationData.text);
        }

        private void BindEngineView(EngineView engineView)
        {
            _unityLogIntegration = new UnityLogIntegration(engineView);

        }

        public void ManualUpdate()
        {
            AudioPlayer.ManualUpdate();
            settingModel.ManualUpdate(Time.deltaTime);
            settingModel.CheckForDiscrepancies();
            _unityLogIntegration?.ManualUpdate();
            PlayTimeControl.Update();
        }

        internal void ChangeSceneFromSettings(string newScene, ReusableSettingMenu.SettingSceneMode settingSceneMode)
        {
            ChangeScene(newScene, settingSceneMode);
        }

        public void ChangeScene(string newScene, ReusableSettingMenu.SettingSceneMode? settingSceneMode = null)
        {
            if (EngineView == null) {
                Debug.LogError("Heart Game needs EngineView to be functional, create or bind EngineView through it's API");
            }
            EngineView.inputManager.RecordSceneLatestDevice();
            var sceneName = SceneManager.GetActiveScene().name;
            crossSceneDataStatic = crossSceneData;
            crossSceneDataStatic.previousSceneName = sceneName;
            SceneManager.LoadScene(newScene);
        }

        public struct CrossSceneData
        {

            public string previousSceneName;
            public ReusableSettingMenu.SettingSceneMode? settingSceneMode;
#if UNITY_SWITCH
            public Uid UserId;
#endif
        }

        public EngineView BindAndGetEngineView()
        {
            EngineView = GameObject.FindObjectOfType<EngineView>();
            EngineView.Init(0);
            BindEngineView(EngineView);
            return EngineView;
        }

        public EngineView CreateEngineView(EngineView.EngineViewInitializationParameter param, int initialLayer)
        {
            EngineView = EngineView.CreateEngineViewThroughCode(param);
            EngineView.Init(initialLayer);
            BindEngineView(EngineView);
            return EngineView;
        }

        public void GoToSettings()
        {
            ReusableSettingMenu.BeforeGoToSettings();
            ChangeScene(ReusableSettingMenu.SettingSceneName);
        }

        public void GoToLanguageSelection()
        {
            ReusableSettingMenu.BeforeGoToLanguageSelection();
            ChangeScene(ReusableSettingMenu.SettingSceneName);
        }

        public static void LogTimeStamp(string v)
        {
            Debug.Log($"Log time {v} {Time.realtimeSinceStartup:F2}");

        }
    }
}