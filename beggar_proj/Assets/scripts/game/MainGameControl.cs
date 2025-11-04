using HeartUnity;
using HeartUnity.View;
using JLayout;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LoadingScreenSetup;

public class MainGameControl : MonoBehaviour
{

    public ArcaniaGameConfigurationUnit ResourceJson;

    [SerializeField]
    public TMP_FontAsset Font;
    public FontGroup FontGroup;
    public Sprite ExpanderSprite;
    public CanvasMaker.CreateObjectRequest ButtonObjectRequest;
    public CanvasMaker.CreateObjectRequest DialogObjectRequest;
    public CanvasMaker.CreateButtonRequest ButtonRequest;
    public CanvasMaker.CreateButtonRequest ButtonRequest_TabSelected;
    public CanvasMaker.CreateCanvasRequest CanvasRequest;
    public CanvasMaker.CreateGaugeRequest SkillXPGaugeRequest;
    public ArcaniaModel arcaniaModel = new();

    public Color MainTextColor;

    public EngineView EngineView { get; internal set; }
    public float TimeMultiplier { get; private set; } = 1;

    

    public LayoutParent TabButtonLayout { get; internal set; }

    public RobustDeltaTime RobustDeltaTime = new();
    public ArcaniaPersistence ArcaniaPersistence;

    public HeartGame HeartGame { get; private set; }
    public ButtonWithProgressBar SettingButtonEnding { get; internal set; }
    public LayoutParent EndingOverlayLayout { get; internal set; }
    public LayoutParent TabButtonOverlayLayout { get; internal set; }
    public JLayoutRuntimeData JLayoutRuntime { get; internal set; }
    public JGameControlDataHolder JControlData { get; internal set; }

    public float lastSaveTime;
    public int SkillFontSize;
    private int _logCountPreProcessedByControl;

    public ControlState controlState = ControlState.TITLE;
    private TitleScreenRuntimeData titleScreenData;
    private LoadingScreenRuntimeData loadingScreenData;

    private static CrossSceneData _crossSceneDataStatic;
    

    private struct CrossSceneData 
    {
        public ControlState? lastControlState;
        public ControlState? requestedControlState;
    }

    public enum ControlState
    {
        TITLE,
        LOADING,
        GAME,
        ARCHIVE_LOADING,
        ARCHIVE_GAME,
        PRESTIGE_WORLD_LOADING,
        PRESTIGE_WORLD
    }

    // Start is called before the first frame update
    void Start()
    {
        HeartGame = HeartGame.Init();
        lastSaveTime = Time.unscaledTime;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        MainGameControlSetupJLayout.SetupCanvas(this);

        var _lastSceneControlState = _crossSceneDataStatic.lastControlState;
        var wannaGoToArchive = _crossSceneDataStatic.requestedControlState == ControlState.ARCHIVE_GAME || _crossSceneDataStatic.requestedControlState == ControlState.ARCHIVE_LOADING;
        var wannaGoToPrestigeWorld = _crossSceneDataStatic.requestedControlState == ControlState.PRESTIGE_WORLD || _crossSceneDataStatic.requestedControlState == ControlState.PRESTIGE_WORLD_LOADING;


        var straightToGameNoTitle =  !wannaGoToArchive && (_lastSceneControlState.HasValue ? _lastSceneControlState == ControlState.GAME : false);

        if (wannaGoToArchive) 
        {
            controlState = ControlState.ARCHIVE_LOADING;
        }

        if (wannaGoToPrestigeWorld)
        {
            controlState = ControlState.PRESTIGE_WORLD_LOADING;
        }

        // SetupMainGame();
        if (straightToGameNoTitle)
        {
            SetupMainGameAllAtOnce();
        }
        else if (controlState == ControlState.TITLE)
        {
            // Setup title screen
            titleScreenData = new TitleScreenRuntimeData();
            TitleScreenSetup.Setup(this, titleScreenData);
        }
        else if (controlState == ControlState.ARCHIVE_LOADING) 
        {
            loadingScreenData = LoadingScreenSetup.Setup(this);
            JControlData.archiveControlData = new();
            loadingScreenData.state = LoadingScreenRuntimeData.State.ARCHIVE_LOADING_PERSISTENCE;
        }
        else if (controlState == ControlState.PRESTIGE_WORLD_LOADING)
        {
            loadingScreenData = LoadingScreenSetup.Setup(this);
        }

        _crossSceneDataStatic = default;
        if (Local.WantToChooseLanguage)
        {
            HeartGame.GoToLanguageSelection();
        }
    }

    void SetupMainGameAllAtOnce()
    {
        MainGameControlSetupJLayout.SetupModelDataAllAtOnce(this);
        RobustDeltaTime = new();
        LoadingScreenControl.LoadSlotAndCommons(this);
        // Let the model run once so you can finish up setup with the latest info on visibility
        arcaniaModel.ManualUpdate(0);
        MainGameControlSetupJLayout.SetupGameCanvasAllAtOnce(this);
        controlState = ControlState.GAME;
    }

    // Update is called once per frame
    void Update()
    {
        #region high DPI handling
        var controlData = this.JControlData;
        var changedScreenSize = Screen.width != controlData.lastScreenSize.x || Screen.height != controlData.lastScreenSize.y;
        controlData.lastScreenSize = new Vector2(Screen.width, Screen.height);
        if (changedScreenSize || EngineView.DpiChanged)
        {
            RectTransformExtensions.LimitDPIBasedOnPhysicalScreenAdjustedWidth(JGameControlExecuter.NormalMinTabWidth, 1920);
            
        }
        #endregion


        // Check title screen state
        if (controlState == ControlState.TITLE)
        {
            var titleState = TitleScreenControl.ManualUpdate(this, titleScreenData);
            if (titleState == TitleScreenState.StartGame)
            {
                loadingScreenData = LoadingScreenSetup.Setup(this);
                this.controlState = ControlState.LOADING;
                //SetupMainGameAllAtOnce();

            }
        }
        else if (controlState == ControlState.LOADING || controlState == ControlState.PRESTIGE_WORLD_LOADING)
        {
            LoadingScreenControl.ManualUpdate(this, loadingScreenData);
            if (loadingScreenData.state == LoadingScreenRuntimeData.State.OVER)
            {
                if (controlState == ControlState.LOADING)
                    controlState = ControlState.GAME;
                else if (controlState == ControlState.PRESTIGE_WORLD_LOADING)
                    controlState = ControlState.PRESTIGE_WORLD;
            }
        }
        else if (controlState == ControlState.ARCHIVE_LOADING)
        {
            LoadingScreenControl.ManualUpdate(this, loadingScreenData);
            if (loadingScreenData.state == LoadingScreenRuntimeData.State.OVER)
            {
                controlState = ControlState.ARCHIVE_GAME;
            }
        }
        else if (controlState == ControlState.ARCHIVE_GAME) {
            ArchiveScreenControlExecuter.ManualUpdate(this);
        }

        #region debug command (title screen too)
        {
            if (DebugMenuManager.CheckCommand("dpi", out int v))
            {
                EngineView.OverwriteDPI(v);
            }
            if (DebugMenuManager.CheckCommand("dpi"))
            {
                EngineView.ClearOverwriteDPI();
            }
            if (DebugMenuManager.CheckCommand("archive"))
            {
                ReloadSceneToArchive();
            }
        }
        #endregion

        //
        // -----------------------------------------------------------
        // Engine etc updating
        // -----------------------------------------------------------
        HeartGame.ManualUpdate();
        EngineView.ManualUpdate();

        if (controlState == ControlState.GAME)
        {

            #region save
            // -----------------------------------------------------------
            // Save data
            // -----------------------------------------------------------
            const int SAVE_COOLDOWN = 30;
            if (Time.unscaledTime - lastSaveTime > SAVE_COOLDOWN)
            {
                lastSaveTime = Time.unscaledTime;
                SaveGameAndCurrentSlot();

                HeartGame.SaveCommon();
            }
            #endregion

            #region debug command
            // -----------------------------------------------------------
            // Debug command
            // -----------------------------------------------------------
            {
                if (DebugMenuManager.CheckCommand("speed", out int v))
                {
                    TimeMultiplier = v;
                }
                if (DebugMenuManager.CheckCommand("speed"))
                {
                    TimeMultiplier = 1;
                }
            }

            {
                if (DebugMenuManager.CheckCommand("value", out string label, out int v))
                {
                    arcaniaModel.FindRuntimeUnit(label).SetValue(v);
                }
            }
            {
                if (DebugMenuManager.CheckCommand("max"))
                {
                    foreach (var item in arcaniaModel.arcaniaUnits.datas)
                    {
                        foreach (var item2 in item.Value)
                        {
                            var endingT = false;
                            foreach (var endU in this.JControlData.EndingData.runtimeUnits)
                            {
                                if (endU == null) continue;
                                if (endU == item2)  {
                                    endingT = true;
                                    break;
                                }
                            }
                            if (item2.HasMax && !endingT)
                            {
                                item2.SetValue(item2.Max);
                                item2.Skill?.Acquire();
                            }
                                
                        }
                    }
                }
            }
            {
                if (DebugMenuManager.CheckCommand("resources"))
                {
                    foreach (var item in arcaniaModel.arcaniaUnits.datas[UnitType.RESOURCE])
                    {
                        item.ChangeValue(99999);
                    }
                }
            }
            {
                if (DebugMenuManager.CheckCommand("require"))
                {
                    foreach (var item in arcaniaModel.arcaniaUnits.datas)
                    {
                        foreach (var d in item.Value)
                        {
                            d.ForceMeetRequire();
                        }
                    }
                }
            }
            #endregion

            #region game update
            // -----------------------------------------------------------
            // Time, game model updating
            // -----------------------------------------------------------
            RobustDeltaTime.ManualUpdate();
            RobustDeltaTime.MultiplyTime(TimeMultiplier);
            while (RobustDeltaTime.TryGetProcessedDeltaTime(out float dt))
            {
                arcaniaModel.ManualUpdate(dt);
            }

            #endregion

            #region J Control Update
            JGameControlExecuter.ManualUpdate(this, this.JControlData, Time.deltaTime);
            #endregion
        }
        if (controlState == ControlState.ARCHIVE_GAME) 
        {
            JGameControlExecuter.ManualUpdateArchive(this, this.JControlData, Time.deltaTime);
        }

        // -----------------------------------------------------------
        // UI update
        // -----------------------------------------------------------


        // Layout is executed after so that it can fix things before rendering
        JLayoutRuntimeExecuter.ManualUpdate(this.JLayoutRuntime);
    }

    public void SaveGameAndCurrentSlot()
    {
        var flavorText = Local.GetText("Nobody");
        var classes = arcaniaModel.arcaniaUnits.datas[UnitType.CLASS];
        int lastTagPriority = -1;
        foreach (var item in classes)
        {
            if (item.Value <= 0) continue;
            foreach (var tag in item.ConfigBasic.Tags)
            {
                for (int tagPriority = 0; tagPriority < JGameControlExecuterSaveSlot.ClassPriorityTier.Length; tagPriority++)
                {
                    string tagP = JGameControlExecuterSaveSlot.ClassPriorityTier[tagPriority];
                    if (tagP != tag.id) continue;
                    if (tagPriority > lastTagPriority)
                    {
                        lastTagPriority = tagPriority;
                        flavorText = item.ConfigBasic.name;
                    }
                }
            }
        }

        SaveSlotModelData modelData = this.JControlData.SaveSlots.ModelData;
        SaveSlotModelData.SaveSlotUnit currentSlotUnit = modelData.CurrentSlotUnit;
        currentSlotUnit.hasSave = true;
        currentSlotUnit.playTimeSeconds = this.JControlData.SaveSlots.PlayTimeOfActiveSlot.PlayTimeToShow;
        currentSlotUnit.lastSaveTime = System.DateTime.Now;
        currentSlotUnit.representativeTextRaw = flavorText;

        SaveSlotExecution.SaveData(modelData, this.HeartGame);
        ArcaniaPersistence.Save(arcaniaModel.arcaniaUnits, arcaniaModel.Exploration);
    }

    public void ReloadScene() 
    {
        BeforeChangeScene();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToSettings()
    {
        BeforeChangeScene();
        HeartGame.GoToSettings();
    }

    public void ReloadSceneToArchive() 
    {
        BeforeChangeScene();
        _crossSceneDataStatic.requestedControlState = ControlState.ARCHIVE_GAME;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReloadSceneToPrestigeWorld()
    {
        BeforeChangeScene();
        _crossSceneDataStatic.requestedControlState = ControlState.PRESTIGE_WORLD;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    internal void BackToGame()
    {
        BeforeChangeScene();
        _crossSceneDataStatic.requestedControlState = ControlState.GAME;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void BeforeChangeScene()
    {
        if (controlState == ControlState.GAME) 
        {
            SaveGameAndCurrentSlot();
            
        }
        MainGameControl._crossSceneDataStatic.lastControlState = this.controlState;
    }
}