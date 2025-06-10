//using UnityEngine.U2D;

using System.Collections.Generic;
#if !UNITY_SWITCH
using System.IO;
#endif
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using static HeartUnity.SettingModel;

namespace HeartUnity.View
{

    public class ReusableSettingMenu : MonoBehaviour
    {
        public static string CloseScene = "";

        public ReusableMenuInCanvas reusableMenuCanvas;
        public ReusableMenuPrefabs reusablePrefabs;
        public SettingPersistence persistence;
        public static CrossSceneData crossSceneData;
#if !PLATFORM_SWITCH && !PLATFORM_ANDROID
        private FileUtilities _fileUtilities;
#endif
        public SettingDialog settingDialog;

        internal void RequestReturn()
        {
            switch (settingSceneMode)
            {
                case SettingSceneMode.SETTINGS:
                    ToPreviousScene();
                    break;
                case SettingSceneMode.LANGUAGE_EXTERNAL:
                    ToPreviousScene();
                    break;
                case SettingSceneMode.CREDITS:
                case SettingSceneMode.LANGUAGE_IN_SETTINGS:
                case SettingSceneMode.CUSTOM_CHOICE:
                    ReusableSettingMenu.BeforeGoToSettings(CloseScene);
                    heartGame.ChangeScene(SettingSceneName);
                    break;
                default:
                    break;
            }
        }

        public List<SettingUnitUI> unitUIs = new List<SettingUnitUI>();
        private HeartGame heartGame;
        private SettingModel model;
        public SettingSceneMode settingSceneMode = SettingSceneMode.SETTINGS;
        public ReusableSettingInput input = new();
        public EngineView engineView;
        public ScrollManager scroll;


        public static Dictionary<string, string> languageLabels = new Dictionary<string, string>(){
            { "english", "English" },
            { "portuguese", "Português" },
            { "brazilianportuguese", "Português BR" },
            { "japanese", "日本語" },
            { "traditionalchinese", "繁體中文" },
            { "simplifiedchinese", "简体中文" },
            { "korean", "한국어" },

        };
        private List<ButtonBinding> bindings;
        private bool _importingSave;
        private const string DIALOG_ID_DELETE_DATA = "dialog_id_delete_data";
        public const string SettingSceneName = "SettingsMenu";

        public static void BeforeGoToSettings(string closeScene = null)
        {
            if (closeScene == null)
                CloseScene = SceneManager.GetActiveScene().name;
            else
                CloseScene = closeScene;
            crossSceneData.settingSceneMode = SettingSceneMode.SETTINGS;
        }

        public static string GetAllLanguageNamesConcatenated()
        {
            var s = "";
            foreach (var item in languageLabels.Values)
            {
                s += item;
            }
            return s;
        }

        public static void BeforeGoToLanguageSelection(bool recordSceneReturn = true)
        {
            if (recordSceneReturn)
                CloseScene = SceneManager.GetActiveScene().name;
            crossSceneData.settingSceneMode = recordSceneReturn ? SettingSceneMode.LANGUAGE_EXTERNAL : SettingSceneMode.LANGUAGE_IN_SETTINGS;
        }


        // currently there is no use for a dialog that doesn't go back to the setting scene
        private void GoToDialog(SettingDialog settingDialog)
        {
            engineView.inputManager.RecordSceneLatestDevice();
            crossSceneData.settingSceneMode = SettingSceneMode.DIALOG;
            ReusableSettingMenu.crossSceneData.settingDialog = settingDialog;

            heartGame.ChangeScene(SettingSceneName);
        }

        public void Awake()
        {
            heartGame = HeartGame.Init();
            model = heartGame.settingModel;
            Cursor.visible = true;
            input.menu = this;
            engineView = heartGame.BindAndGetEngineView();
            bindings = InputManager.CreateDefaultButtonBindings();
            settingDialog = crossSceneData.settingDialog;
            settingSceneMode = crossSceneData.settingSceneMode;
            crossSceneData = default;
#if !UNITY_SWITCH && !UNITY_ANDROID
            _fileUtilities = new FileUtilities();
#endif
            switch (settingSceneMode)
            {
                case SettingSceneMode.SETTINGS:
                    SettingMode();
                    break;
                case SettingSceneMode.LANGUAGE_IN_SETTINGS:
                case SettingSceneMode.LANGUAGE_EXTERNAL:
                    LanguageSelecMode();
                    break;
                case SettingSceneMode.CUSTOM_CHOICE:
                    CustomChoiceMode();
                    break;
                case SettingSceneMode.CREDITS:
                    ShowCreditsMode();
                    break;
                case SettingSceneMode.DIALOG:
                    DialogMode();
                    break;
                default:
                    break;
            }

        }

        private void DialogMode()
        {

            var prompt = Instantiate(reusablePrefabs.text, reusableMenuCanvas.menuContent.transform);
            prompt.text = Local.GetText(settingDialog.promptKey);
            for (int i = 0; i < 2; i++)
            {
                var cancelbutton = i == 0;
                SettingUnitUI suu = new SettingUnitUI();
                suu.dialogConfirm = !cancelbutton;
                unitUIs.Add(suu);
                var buttonHolder = Instantiate(reusablePrefabs.button, reusableMenuCanvas.menuContent.transform);
                var button = buttonHolder.buttonManager;
                button.buttonText = Local.GetText(cancelbutton ? settingDialog.cancelKey : settingDialog.confirmKey);
                button.UpdateUI();
                suu.button = buttonHolder;
                button.onClick.AddListener(() =>
                {
                    PressDialogButton(!cancelbutton);
                });
            }

        }

        public void PressDialogButton(bool confirmTCancelF)
        {
            if (!confirmTCancelF)
            {
                BeforeGoToSettings(CloseScene);
                heartGame.ChangeScene(SettingSceneName);
                return;
            }
            switch (settingDialog.id)
            {
                case ReusableSettingMenu.DIALOG_ID_DELETE_DATA:
#if !UNITY_SWITCH
                    string dataPath = Application.persistentDataPath;
                    string[] files = Directory.GetFiles(dataPath);
                    foreach (string file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (System.Exception){}
                    }
                    
#endif
                    PlayerPrefs.DeleteAll();
                    break;
                default:
                    break;
            }
            BeforeGoToSettings(CloseScene);
            heartGame.ChangeScene(SettingSceneName);

        }

        private void ShowCreditsMode()
        {
            var config = HeartGame.GetConfig();
            {
                var text = Instantiate(reusablePrefabs.textAutoFitForSettings, reusableMenuCanvas.menuContent.transform);
                text.rawText = config.Credits.text;
            }
            {
                var buttonHolder = Instantiate(reusablePrefabs.button, reusableMenuCanvas.menuContent.transform);
                var button = buttonHolder.buttonManager;
                button.buttonText = Local.GetText("Return");
                button.UpdateUI();
                SettingUnitUI item = new SettingUnitUI();
                item.button = buttonHolder;
                item.leaveLanguage = true;
                this.unitUIs.Add(item);
                button.onClick.AddListener(() =>
                {
                    AudioPlayer.PlaySFX("click");
                    RequestReturn();
                });
            }

        }

        private void LanguageSelecMode()
        {
            var config = HeartGame.GetConfig();
            for (int i = 0; i < Local.Instance.languages.Count; i++)
            {

                int currentLanguage = i;
                Local.LanguageSet lang = Local.Instance.languages[i];
                if (config.blacklistedLanguages.Contains(lang.languageName)) continue;

                SettingUnitUI suu = new SettingUnitUI();
                suu.language = lang;
                unitUIs.Add(suu);

                var langKey = lang.languageName.ToLower().Replace(" ", "").Trim();
                var languageLabel = languageLabels[langKey];
                var buttonHolder = Instantiate(reusablePrefabs.button, reusableMenuCanvas.menuContent.transform);
                var button = buttonHolder.buttonManager;
                suu.button = buttonHolder;
                button.buttonText = languageLabel;
                button.UpdateUI();
                button.onClick.AddListener(() =>
                {
                    var langu = lang;
                    LanguageButtonPressed(langu);
                });
            }
            if (settingSceneMode == SettingSceneMode.LANGUAGE_IN_SETTINGS)
            {
                var buttonHolder = Instantiate(reusablePrefabs.button, reusableMenuCanvas.menuContent.transform);
                var button = buttonHolder.buttonManager;
                button.buttonText = Local.GetText("Return");
                button.UpdateUI();
                SettingUnitUI item = new SettingUnitUI();
                item.button = buttonHolder;
                item.leaveLanguage = true;
                this.unitUIs.Add(item);
                button.onClick.AddListener(() =>
                {
                    LeaveLanguageButton();
                });
            }
        }

        private void CustomChoiceMode()
        {
            var config = HeartGame.GetConfig();
            // TODO get data from an ID sent through cross scene
            var choiceD = config.SettingCustomChoices[0];
            for (int i = 0; i < choiceD.choiceKeys.Count; i++)
            {
                SettingUnitUI suu = new SettingUnitUI();
                suu.ChoicePosition = i;
                unitUIs.Add(suu);

                var buttonLabel = choiceD.choiceKeys[i];
                var buttonHolder = Instantiate(reusablePrefabs.button, reusableMenuCanvas.menuContent.transform);
                var button = buttonHolder.buttonManager;
                suu.button = buttonHolder;
                button.buttonText = Local.GetText(buttonLabel);
                button.UpdateUI();
                button.onClick.AddListener(() =>
                {
                    var choiceP = i;
                    CustomChoiceChosen(choiceP);
                });
            }
            if (settingSceneMode == SettingSceneMode.LANGUAGE_IN_SETTINGS)
            {
                var buttonHolder = Instantiate(reusablePrefabs.button, reusableMenuCanvas.menuContent.transform);
                var button = buttonHolder.buttonManager;
                button.buttonText = Local.GetText("Return");
                button.UpdateUI();
                SettingUnitUI item = new SettingUnitUI();
                item.button = buttonHolder;
                item.leaveLanguage = true;
                this.unitUIs.Add(item);
                button.onClick.AddListener(() =>
                {
                    LeaveLanguageButton();
                });
            }
        }

        public void LeaveLanguageButton()
        {
            AudioPlayer.PlaySFX("click");
            RequestReturn();
        }

        private void SettingMode()
        {
            MainGameConfig mainGameConfig = HeartGame.GetConfig();
            var credits = mainGameConfig.Credits;
            var disableDiscordButton = string.IsNullOrWhiteSpace(mainGameConfig.urls?.DiscordServer) || Application.platform == RuntimePlatform.Switch;
            var disableDeleteData = Application.platform == RuntimePlatform.Switch;
            var disableExitButton = Application.platform == RuntimePlatform.Switch || Application.platform == RuntimePlatform.WebGLPlayer;
            var disableFullScreen = Application.platform == RuntimePlatform.Switch;
            var disableVoice = mainGameConfig.voiceLists == null || mainGameConfig.voiceLists.Length == 0;

            foreach (var uc in model.unitControls)
            {
                
                if (uc.settingData.standardSettingType == SettingUnitData.StandardSettingType.LANGUAGE_SELECTION && Local.Instance.languages.Count <= 1) continue;
                if (uc.settingData.standardSettingType == SettingUnitData.StandardSettingType.SHOW_CREDITS && credits == null) continue;
                if (uc.settingData.standardSettingType == SettingUnitData.StandardSettingType.DISCORD_SERVER && disableDiscordButton) continue;
                if (uc.settingData.standardSettingType == SettingUnitData.StandardSettingType.DELETE_DATA && disableDeleteData) continue;
                if (uc.settingData.standardSettingType == SettingUnitData.StandardSettingType.FULLSCREEN && disableFullScreen) continue;
                if (uc.settingData.standardSettingType == SettingUnitData.StandardSettingType.VOICE_VOLUME && disableVoice) continue;
                if (disableExitButton && uc.settingData.standardSettingType == SettingUnitData.StandardSettingType.EXIT_GAME) continue;
                var settingUnitUI = new SettingUnitUI();
                unitUIs.Add(settingUnitUI);
                settingUnitUI.settingRT = uc;
                switch (uc.settingData.settingType)
                {
                    case SettingUnitData.SettingType.BUTTON:
                        var buttonH = Instantiate(reusablePrefabs.button, reusableMenuCanvas.menuContent.transform);
                        var button = buttonH.buttonManager;
                        button.buttonText = Local.GetText(uc.settingData.titleTexstring);
                        button.UpdateUI();
                        var unitControl = uc;
                        settingUnitUI.button = buttonH;
                        // has to be on cursor down for better compatibility
                        // with WebGL Javascript functions
                        // like save exporting
                        buttonH.OnCursorDown.AddListener(() =>
                        {
                            ButtonPressed(unitControl);
                        });
                        break;
                    case SettingUnitData.SettingType.SLIDER:
                        var slider = Instantiate(reusablePrefabs.slider, reusableMenuCanvas.menuContent.transform);
                        slider.value = uc.rtFloat;
                        settingUnitUI.slider = slider;
                        slider.label.SetTextKey(uc.settingData.titleTexstring);
                        break;
                    case SettingUnitData.SettingType.SWITCH:

                        var toggle = Instantiate(reusablePrefabs.toggle, reusableMenuCanvas.menuContent.transform);
                        if (uc.rtBool) toggle.switchManager.SetOn();
                        else toggle.switchManager.SetOff();
                        toggle.switchManager.onValueChanged.AddListener((b) =>
                        {
                            Debug.Log("Switch value change " + b);
                            ToogleUpdated(b, uc);
                        });
                        settingUnitUI.toggle = toggle;
                        toggle.label.SetTextKey(uc.settingData.titleTexstring);

                        break;
                    default:
                        break;
                }
            }
        }


        public void LanguageButtonPressed(Local.LanguageSet langu)
        {
            model.SetString(SettingUnitData.StandardSettingType.LANGUAGE_SELECTION, langu.languageName);
            AudioPlayer.PlaySFX("click");
            RequestReturn();
        }

        private void CustomChoiceChosen(int choiceP)
        {
            // TODO support for multiple choices, not just 1
            model.SetInt(SettingUnitData.StandardSettingType.CUSTOM_CHOICE_1, choiceP);
            AudioPlayer.PlaySFX("click");
            RequestReturn();
        }

        public void ToogleUpdated(bool b, SettingUnitRealTime uc)
        {
            uc.rtBool = b;
            model.SaveData();
            model.Enforce(uc);
        }

        public void Update()
        {
            heartGame.ManualUpdate();
            engineView.ManualUpdate();
            engineView.inputManager.UpdateWithButtonBindings(bindings);
            input.ManualUpdate();
#if !PLATFORM_SWITCH && !PLATFORM_ANDROID
            if (_importingSave && _fileUtilities.UploadedBytes != null)
            {
                _importingSave = false;
                using var _1 = ListPool<string>.Get(out var names);
                using var _2 = ListPool<string>.Get(out var content);
                ZipUtilities.ExtractZipFromBytes(_fileUtilities.UploadedBytes, names, content);
                SaveDataCenter.ImportSave(names, content);
                RequestReturn();
            }
#endif

            foreach (var uu in unitUIs)
            {
                if (uu.settingRT == null) continue;
                switch (uu.settingRT.settingData.settingType)
                {
                    case SettingUnitData.SettingType.SLIDER:
                        if (uu.settingRT.rtFloat != uu.slider.sliderManager.mainSlider.normalizedValue)
                        {
                            uu.settingRT.rtFloat = uu.slider.sliderManager.mainSlider.normalizedValue;
                            model.SaveData();
                            model.Enforce(uu.settingRT);
                        }

                        break;
                }
            }
            model.ManualUpdate(Time.deltaTime);
            var discrepant = model.CheckForDiscrepancies();
            if (discrepant)
            {
                RefreshUI();
            }
        }

        // Model information fed into the UI (the opposite of the normal flow)
        private void RefreshUI()
        {
            foreach (var uu in this.unitUIs)
            {
                switch (uu.settingRT.settingData.settingType)
                {
                    case SettingUnitData.SettingType.BUTTON:
                        break;
                    case SettingUnitData.SettingType.SWITCH:
                        uu.toggle.IsOn = uu.settingRT.rtBool;
                        Debug.Log("Discrepancy update " + uu.settingRT.rtBool);
                        break;
                    case SettingUnitData.SettingType.SLIDER:

                        break;
                    default:
                        break;
                }
            }
        }

        public void ButtonPressed(SettingUnitRealTime unitControl)
        {
            AudioPlayer.PlaySFX("click");
            switch (unitControl.settingData.standardSettingType)
            {

                case SettingUnitData.StandardSettingType.EXIT_GAME:
#if PLATFORM_SWITCH
                    // SWITCH should have no EXIT_GAME
                    // new NintendoSwitchSaveLoad().Init().Save();
#else
                    Application.Quit();
#endif
                    break;
                case SettingUnitData.StandardSettingType.EXIT_MENU:
                    RequestReturn();
                    break;
#if !PLATFORM_SWITCH &&!PLATFORM_ANDROID
                case SettingUnitData.StandardSettingType.EXPORT_SAVE:
                    {
                        var config = HeartGame.GetConfig();
                        var bytes = SaveDataCenter.GenerateExportSave();
                        _fileUtilities.ExportBytes(bytes, $"{config.gameTitle}_savedata{System.DateTime.Now.ToString("yyyy_M_d_H_m_s")}", "hg");
                    }
                    break;
                case SettingUnitData.StandardSettingType.IMPORT_SAVE:
                    {
                        _fileUtilities.ImportFileRequest("hg");
                        _importingSave = true;
                    }
                    break;
#endif
                case SettingUnitData.StandardSettingType.DELETE_DATA:

                    GoToDialog(new SettingDialog()
                    {
                        id = DIALOG_ID_DELETE_DATA,
                        cancelKey = ReusableLocalizationKeys.CST_NO,
                        confirmKey = ReusableLocalizationKeys.CST_YES,
                        promptKey = ReusableLocalizationKeys.CST_DELETE_DATA_CONFIRMATION
                    });
                    break;
                case SettingUnitData.StandardSettingType.LANGUAGE_SELECTION:
                    engineView.inputManager.RecordSceneLatestDevice();
                    ReusableSettingMenu.BeforeGoToLanguageSelection(false);
                    heartGame.ChangeScene(ReusableSettingMenu.SettingSceneName);
                    break;
                case SettingUnitData.StandardSettingType.CUSTOM_CHOICE_1:
                    engineView.inputManager.RecordSceneLatestDevice();
                    crossSceneData.settingSceneMode = SettingSceneMode.CUSTOM_CHOICE;
                    heartGame.ChangeScene(ReusableSettingMenu.SettingSceneName);
                    break;
                case SettingUnitData.StandardSettingType.SHOW_CREDITS:
                    crossSceneData.settingSceneMode = SettingSceneMode.CREDITS;
                    engineView.inputManager.RecordSceneLatestDevice();
                    heartGame.ChangeScene(SettingSceneName);
                    break;
                case SettingUnitData.StandardSettingType.DISCORD_SERVER:
                    URLOpener.OpenURL(HeartGame.GetConfig().urls.DiscordServer);
                    break;
                case SettingUnitData.StandardSettingType.MASTER_VOLUME:
                case SettingUnitData.StandardSettingType.MUSIC_VOLUME:
                case SettingUnitData.StandardSettingType.SFX_VOLUME:
                case SettingUnitData.StandardSettingType.VOICE_VOLUME:
                case SettingUnitData.StandardSettingType.FULLSCREEN:
                default:
                    break;
            }
        }


        public void ToPreviousScene()
        {
            engineView.inputManager.RecordSceneLatestDevice();
            heartGame.ChangeSceneFromSettings(CloseScene, this.settingSceneMode);
        }

        public class SettingUnitUI
        {
            public SettingUnitRealTime settingRT;
            public ToggleHolder toggle;
            internal SliderHolder slider;
            public ButtonHolder button;
            internal Local.LanguageSet language;
            internal bool leaveLanguage;
            internal bool? dialogConfirm;

            public int? ChoicePosition { get; internal set; }
        }

        public class SettingDialog
        {
            public string id;
            public string promptKey;
            public string confirmKey;
            public string cancelKey;
        }

        public struct CrossSceneData
        {
            public SettingDialog settingDialog;
            public SettingSceneMode settingSceneMode;
        }

        public enum SettingSceneMode
        {
            SETTINGS,
            LANGUAGE_IN_SETTINGS,
            LANGUAGE_EXTERNAL,
            DIALOG,
            CREDITS,
            CUSTOM_CHOICE
        }
    }
}