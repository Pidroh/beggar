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
        public const string DefaultCommonsSaveDataKey = "player_commons";
        public SettingModel settingModel;

        private UnityLogIntegration _unityLogIntegration;
        private static CrossSceneData crossSceneDataStatic;
        public CrossSceneData crossSceneData;
        public CrossSceneGenericData crossSceneGenericData = new();

        public static bool MousePlatform => !Application.isConsolePlatform && Application.isMobilePlatform;

        public EngineView EngineView { get; private set; }

        public PlayTimeControlCenter PlayTimeControl = new PlayTimeControlCenter();
        private CommonPlayerSaveDataPersistence _commonSaveDataPersistence;
        public MainGameConfig config;

        public static HeartGame Init()
        {
            var config = GetConfig();
            ReadLocalizationData(config);

            AudioPlayer.Init(config.musicList, config.audioList, config.voiceLists);
            var heartGame = new HeartGame();
            heartGame.config = config;
            heartGame.crossSceneData = crossSceneDataStatic;
            crossSceneDataStatic = default;
#if UNITY_SWITCH && !UNITY_EDITOR
            TryLoadSwitchUser(heartGame);
#endif
            heartGame.settingModel = new SettingModel();
            heartGame.settingModel.Init(config.SettingData, heartGame);
            return heartGame;
        }

        // Can overwrite key if you need multiple keys for slots and stuff
        public void CommonDataLoad(string key = HeartGame.DefaultCommonsSaveDataKey)
        {
            CreateCommonPlayerSaveDataPersistence(key);
            if (_commonSaveDataPersistence.TryLoad(out var playerSave))
            {
                PlayTimeControl.Load(playerSave);
            }
            else 
            {
                PlayTimeControl.Init(0);
            }
        }

        private void CreateCommonPlayerSaveDataPersistence(string key)
        {
            _commonSaveDataPersistence = new CommonPlayerSaveDataPersistence(key, this);
        }

        public void SaveCommon()
        {
            SaveCommonData();
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
            if (engineView.reusableMenuPrefabs == null) return;
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
            BeforeChangeScene();
            SceneManager.LoadScene(newScene);
        }

        // SHOULD BE PRIVATE, changing a scene shouldn't happen outside this class
        private void BeforeChangeScene()
        {
            if (EngineView == null)
            {
                Debug.LogError("Heart Game needs EngineView to be functional, create or bind EngineView through it's API");
            }

            // only saves if it has correctly loaded once
            if (_commonSaveDataPersistence != null)
            {
                SaveCommonData();
                // this should only be called if the PlayTimeControl has been inited, so it's fine if it's called here
                // in other words, if there is no commonSaveDataPersistence, you shouldn't call before change scene
                PlayTimeControl.BeforeChangeScene();
            }

            EngineView.inputManager.RecordSceneLatestDevice();
            var sceneName = SceneManager.GetActiveScene().name;
            crossSceneDataStatic = crossSceneData;
            crossSceneDataStatic.previousSceneName = sceneName;
        }

        private void SaveCommonData()
        {
            var common = new CommonPlayerSaveData();
            PlayTimeControl.FeedSaveCommonData(common);
            _commonSaveDataPersistence.Save(common);
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

        public void ReloadScene()
        {
            BeforeChangeScene();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}