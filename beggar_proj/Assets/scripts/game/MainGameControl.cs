using HeartUnity;
using HeartUnity.View;
using JLayout;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
public class MainGameControl : MonoBehaviour
{

    public ArcaniaGameConfigurationUnit ResourceJson;

    [SerializeField]
    public TMP_FontAsset Font;
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

    private ControlState controlState = ControlState.TITLE;
    private TitleScreenRuntimeData titleScreenData;

    public enum ControlState
    {
        TITLE,
        GAME
    }

    // Start is called before the first frame update
    void Start()
    {
        HeartGame = HeartGame.Init();
        lastSaveTime = Time.unscaledTime;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        MainGameControlSetupJLayout.SetupCanvas(this);

        SetupMainGame();

        /*
        // Setup title screen
        titleScreenData = new TitleScreenRuntimeData();
        TitleScreenSetup.Setup(this, titleScreenData);
        GameStarted = false;
        */
    }

    void SetupMainGame()
    {
        MainGameControlSetupJLayout.SetupModelData(this);
        RobustDeltaTime = new();
        ArcaniaPersistence = new(HeartGame);
        ArcaniaPersistence.Load(arcaniaModel.arcaniaUnits, arcaniaModel.Exploration);
        HeartGame.CommonDataLoad();
        // Let the model run once so you can finish up setup with the latest info on visibility
        arcaniaModel.ManualUpdate(0);
        MainGameControlSetupJLayout.SetupGameCanvas(this);
        controlState = ControlState.GAME;
    }

    // Update is called once per frame
    void Update()
    {
        /*/ Check title screen state
        if (controlState == ControlState.TITLE)
        {
            var titleState = TitleScreenControl.ManualUpdate(titleScreenData);
            if (titleState == TitleScreenState.StartGame)
            {
                SetupMainGame();
            }
        }
        //*/
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
                ArcaniaPersistence.Save(arcaniaModel.arcaniaUnits, arcaniaModel.Exploration);
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
                if (DebugMenuManager.CheckCommand("dpi", out int v))
                {
                    EngineView.OverwriteDPI(v);
                }
                if (DebugMenuManager.CheckCommand("dpi"))
                {
                    EngineView.ClearOverwriteDPI();
                }
            }
            {
                if (DebugMenuManager.CheckCommand("value", out string label, out int v))
                {
                    arcaniaModel.FindRuntimeUnit(label).SetValue(v);
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

            // -----------------------------------------------------------
            // Time, game updating
            // -----------------------------------------------------------
            RobustDeltaTime.ManualUpdate();
            RobustDeltaTime.MultiplyTime(TimeMultiplier);
            while (RobustDeltaTime.TryGetProcessedDeltaTime(out float dt))
            {
                arcaniaModel.ManualUpdate(dt);
            }

            #region J Control Update
            JGameControlExecuter.ManualUpdate(this, this.JControlData, Time.deltaTime);
            #endregion
        }

        // -----------------------------------------------------------
        // UI update
        // -----------------------------------------------------------


        // Layout is executed after so that it can fix things before rendering
        JLayoutRuntimeExecuter.ManualUpdate(this.JLayoutRuntime);
    }

    public void GoToSettings()
    {
        ArcaniaPersistence.Save(arcaniaModel.arcaniaUnits, arcaniaModel.Exploration);
        HeartGame.GoToSettings();
    }
}