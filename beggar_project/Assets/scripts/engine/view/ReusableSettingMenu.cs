//using UnityEngine.U2D;

using System.Collections.Generic;
using System.IO;
using UnityEngine;
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
        public SettingDialog settingDialog;

        internal void RequestReturn()
        {
            switch (settingSceneMode)
            {
                case SettingSceneMode.SETTINGS:
                    ToPreviousScene();
                    break;
                case SettingSceneMode.LANGUAGE_IN_SETTINGS:
                    ReusableSettingMenu.GoToSettings(CloseScene);
                    break;
                case SettingSceneMode.LANGUAGE_EXTERNAL:
                    ToPreviousScene();
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
        private const string DIALOG_ID_DELETE_DATA = "dialog_id_delete_data";
        public const string SettingSceneName = "SettingsMenu";

        public static void GoToSettings(string closeScene = null)
        {
            if (closeScene == null)
                CloseScene = SceneManager.GetActiveScene().name;
            else
                CloseScene = closeScene;
            SceneManager.LoadScene(SettingSceneName);
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

        public static void GoToLanguageSelection(bool recordSceneReturn = true)
        {
            if (recordSceneReturn)
                CloseScene = SceneManager.GetActiveScene().name;
            crossSceneData.settingSceneMode = recordSceneReturn ? SettingSceneMode.LANGUAGE_EXTERNAL : SettingSceneMode.LANGUAGE_IN_SETTINGS;
            SceneManager.LoadScene(SettingSceneName);
        }


        // currently there is no use for a dialog that doesn't go back to the setting scene
        private static void GoToDialog(SettingDialog settingDialog)
        {
            crossSceneData.settingSceneMode = SettingSceneMode.DIALOG;
            ReusableSettingMenu.crossSceneData.settingDialog = settingDialog;
            SceneManager.LoadScene(SettingSceneName);
        }

        public void Awake()
        {
            heartGame = HeartGame.Init();
            model = heartGame.settingModel;
            Cursor.visible = true;
            input.menu = this;
            engineView = GameObject.FindObjectOfType<EngineView>();
            engineView.Init(0);
            heartGame.BindEngineView(engineView);
            bindings = InputManager.CreateDefaultButtonBindings();
            settingDialog = crossSceneData.settingDialog;
            settingSceneMode = crossSceneData.settingSceneMode;
            crossSceneData = default;
            switch (settingSceneMode)
            {
                case SettingSceneMode.SETTINGS:
                    SettingMode();
                    break;
                case SettingSceneMode.LANGUAGE_IN_SETTINGS:
                case SettingSceneMode.LANGUAGE_EXTERNAL:
                    LanguageSelecMode();
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
            if (!confirmTCancelF) { GoToSettings(CloseScene); return; }
            switch (settingDialog.id)
            {
                case ReusableSettingMenu.DIALOG_ID_DELETE_DATA:
#if UNITY_WEBGL
#else
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
            GoToSettings(CloseScene);

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


        public void LeaveLanguageButton()
        {
            AudioPlayer.PlaySFX("click");
            RequestReturn();
        }

        private void SettingMode()
        {
            foreach (var uc in model.unitControls)
            {
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
                        button.onClick.AddListener(() =>
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
                    Application.Quit();
                    break;
                case SettingUnitData.StandardSettingType.EXIT_MENU:
                    RequestReturn();
                    break;
                case SettingUnitData.StandardSettingType.DELETE_DATA:
                    ReusableSettingMenu.GoToDialog(new SettingDialog()
                    {
                        id = DIALOG_ID_DELETE_DATA,
                        cancelKey = ReusableLocalizationKeys.CST_NO,
                        confirmKey = ReusableLocalizationKeys.CST_YES,
                        promptKey = ReusableLocalizationKeys.CST_DELETE_DATA_CONFIRMATION
                    });
                    break;
                case SettingUnitData.StandardSettingType.LANGUAGE_SELECTION:
                    ReusableSettingMenu.GoToLanguageSelection(false);
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
        }
    }
}