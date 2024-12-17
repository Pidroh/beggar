using HeartUnity;
using HeartUnity.View;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainGameControl : MonoBehaviour
{

    public ArcaniaGameConfigurationUnit ResourceJson;
    public DynamicCanvas dynamicCanvas;
    public List<TabControlUnit> TabControlUnits = new();

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
    public RuntimeUnit EndGameRuntimeUnit { get; internal set; }
    public UIUnit EndGameMessage { get; internal set; }
    public LayoutParent TabButtonLayout { get; internal set; }

    public RobustDeltaTime RobustDeltaTime = new();
    public ArcaniaPersistence ArcaniaPersistence;

    public HeartGame HeartGame { get; private set; }
    public ButtonWithProgressBar SettingButtonEnding { get; internal set; }

    public float lastSaveTime;
    public ControlExploration controlExploration;

    // Start is called before the first frame update
    void Start()
    {
        HeartGame = HeartGame.Init();
        lastSaveTime = Time.unscaledTime;
        controlExploration = new ControlExploration(this);
        MainGameControlSetup.Setup(this);
        RobustDeltaTime = new();
        ArcaniaPersistence = new(HeartGame);
        ArcaniaPersistence.Load(arcaniaModel.arcaniaUnits, arcaniaModel.Exploration);
        HeartGame.CommonDataLoad();
    }

    // Update is called once per frame
    void Update()
    {
        // -----------------------------------------------------------
        // Engine etc updating
        // -----------------------------------------------------------
        HeartGame.ManualUpdate();
        EngineView.ManualUpdate();

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

        // -----------------------------------------------------------
        // Show end game
        // -----------------------------------------------------------
        if (EndGameRuntimeUnit != null && EndGameRuntimeUnit.Value > 0 && !dynamicCanvas.OverlayVisible)
        {
            dynamicCanvas.ShowOverlay();
            this.EndGameMessage.rawText = this.EndGameMessage.rawText + $"\n\nThe total play time was {HeartGame.PlayTimeControl.PlayTimeToShowAsString}";
        }
        // -----------------------------------------------------------
        // Time, game updating
        // -----------------------------------------------------------
        RobustDeltaTime.ManualUpdate();
        while (RobustDeltaTime.TryGetProcessedDeltaTime(out float dt))
        {
            arcaniaModel.ManualUpdate(Time.deltaTime * TimeMultiplier);
        }
        // -----------------------------------------------------------
        // UI update
        // -----------------------------------------------------------
        dynamicCanvas.ManualUpdate();
        // hide lower menu if all the tabs are visible
        dynamicCanvas.LowerMenus[0].SelfChild.Visible = dynamicCanvas.CalculateNumberOfVisibleHorizontalChildren() < arcaniaModel.arcaniaUnits.datas[UnitType.TAB].Count;
        TabButtonLayout.SelfChild.RectTransform.SetHeightMilimeters(10);
        if (SettingButtonEnding.Button.Clicked)
        {
            GoToSettings();
        }
        // -----------------------------------------------------------
        // Dialog
        // -----------------------------------------------------------
        if (arcaniaModel.Dialog.ShouldShow != dynamicCanvas.IsDialogActive)
        {
            var dialog = arcaniaModel.Dialog.ActiveDialog;
            if (arcaniaModel.Dialog.ShouldShow)
            {
                dynamicCanvas.ShowDialog(dialog.Id, dialog.Title, dialog.Content);
            }
            else
            {
                dynamicCanvas.HideAllDialogs();
            }
        }
        if (dynamicCanvas.DialogViews[0].buttonConfirm.Button.Clicked)
        {
            arcaniaModel.Dialog.DialogComplete(0);
        }
        if (dynamicCanvas.DialogViews[0].buttonCancel.Button.Clicked)
        {
            arcaniaModel.Dialog.DialogComplete(1);
        }
        // -----------------------------------------------------------
        // Main update loop
        // -----------------------------------------------------------
        for (int tabIndex = 0; tabIndex < TabControlUnits.Count; tabIndex++)
        {
            TabControlUnit tabControl = TabControlUnits[tabIndex];
            var tabNormalContentVisible = !(tabControl.TabData.Tab.ExplorationActiveTab && arcaniaModel.Exploration.IsExplorationActive);
            tabControl.SelectionButton.ManualUpdate();

            foreach (var sep in tabControl.Separators)
            {
                sep.Visible = false;
                sep.SeparatorLC.RectTransform.SetHeightMilimeters(9);
                sep.Text.text.SetFontSizePhysical(14);
                if (sep.SpaceAmountText == null) continue;
                sep.SpaceAmountText.RectTransform.SetHeightMilimeters(11);
                sep.SpaceAmountText.text.SetFontSizePhysical(18);
                sep.SpaceAmountText.rawText = $"Space: {arcaniaModel.Housing.SpaceConsumed} / {arcaniaModel.Housing.TotalSpace}";
            }
            tabControl.SelectionButton.Visible = tabControl.TabData.Visible;
            dynamicCanvas.EnableChild(tabIndex, tabControl.TabData.Visible);
            tabControl.SelectionButton.Button.Image.color = dynamicCanvas.IsChildVisible(tabIndex) ? this.ButtonRequest_TabSelected.MainBody.NormalColor : this.ButtonRequest.MainBody.NormalColor;

            if (tabControl.SelectionButton.Button.Clicked)
            {
                if (tabControl.TabData.Tab.OpenSettings)
                {
                    GoToSettings();
                }
                else
                {
                    if (dynamicCanvas.CanShowOnlyOneChild())
                    {
                        dynamicCanvas.ShowChild(tabIndex);
                    }
                    else
                    {
                        dynamicCanvas.ToggleChild(tabIndex);
                    }

                }

            }
            if (!dynamicCanvas.children[tabIndex].SelfChild.Visible) continue;
            #region log updating
            if (tabControl.TabData.Tab.ContainsLogs)
            {
                while (tabControl.LogControlUnits.Count < arcaniaModel.LogUnits.Count)
                {
                    MainGameControlSetup.CreateLogControlUnit(mgc: this, tabControl: tabControl, lp: dynamicCanvas.children[tabIndex], logUnit: arcaniaModel.LogUnits[tabControl.LogControlUnits.Count]);
                }
                foreach (var item in tabControl.LogControlUnits)
                {
                    item.Text.text.SetFontSizePhysical(15);
                    item.Lc.RectTransform.SetHeightMilimeters(9);
                }
            }
            #endregion
            var UnitGroupControls = tabControl.UnitGroupControls;

            // -----------------------------------------------------------
            // Update per unit
            // -----------------------------------------------------------
            foreach (var pair in UnitGroupControls)
            {
                foreach (var tcu in pair.Value)
                {
                    var data = tcu.Data;
                    tcu.ManualUpdate(arcaniaModel);
                    bool visible = data.Visible && tabNormalContentVisible;
                    tcu.SetVisible(visible);

                    if (!visible) continue;
                    if (tcu.ParentTabSeparator != null) tcu.ParentTabSeparator.Visible = true;
                    if (tcu.ExpandManager.Expanded)
                    {
                        var modUnit = tcu.ModsUnit;
                        FeedMods(data, modUnit);
                        tcu.needConditionUnit?.TTV?.ManualUpdate();
                    }



                    switch (pair.Key)
                    {

                        case UnitType.SKILL:
                            {

                                tcu.bwe.MainButton.ButtonEnabled = data.Skill.Acquired ? arcaniaModel.Runner.CanStudySkill(data) : arcaniaModel.Runner.CanAcquireSkill(data);
                                if (tcu.TaskClicked)
                                {
                                    if (data.Skill.Acquired) arcaniaModel.Runner.StudySkill(data);
                                    else arcaniaModel.Runner.AcquireSkill(data);

                                }

                            }
                            break;
                        case UnitType.HOUSE:
                            tcu.bwe.MainButton.Image.color = !arcaniaModel.Housing.IsLivingInHouse(data) ? ButtonRequest.MainBody.NormalColor : ButtonRequest.MainBody.SelectedColor;
                            tcu.bwe.MainButton.ButtonEnabled = arcaniaModel.Housing.CanChangeHouse(data);

                            if (tcu.TaskClicked)
                            {
                                if (!arcaniaModel.Housing.IsLivingInHouse(data)) arcaniaModel.Housing.ChangeHouse(data);
                            }

                            break;
                        case UnitType.FURNITURE:
                            {
                                tcu.ButtonAdd.Button.ButtonEnabled = arcaniaModel.Housing.CanAcquireFurniture(data);
                                tcu.ButtonRemove.Button.ButtonEnabled = arcaniaModel.Housing.CanRemoveFurniture(data);

                                if (tcu.ButtonAdd.Button.Clicked)
                                {
                                    arcaniaModel.Housing.AcquireFurniture(data);
                                }
                                if (tcu.ButtonRemove.Button.Clicked)
                                {
                                    arcaniaModel.Housing.RemoveFurniture(data);
                                }

                            }
                            break;
                        case UnitType.RESOURCE:
                            break;
                        case UnitType.TASK:
                            {
                                tcu.bwe.MainButtonEnabled = arcaniaModel.Runner.CanStartAction(data);
                                tcu.bwe.MainButtonSelected(arcaniaModel.Runner.RunningTasks.Contains(data));
                                if (tcu.TaskClicked)
                                {
                                    arcaniaModel.Runner.StartActionExternally(data);
                                }
                            }
                            break;
                        case UnitType.LOCATION:
                            {
                                tcu.bwe.MainButtonEnabled = arcaniaModel.Runner.CanStartAction(data);
                                tcu.bwe.MainButtonSelected(arcaniaModel.Runner.RunningTasks.Contains(data));
                                if (tcu.TaskClicked)
                                {
                                    arcaniaModel.Runner.StartActionExternally(data);
                                }
                            }
                            break;
                        case UnitType.CLASS:
                            {
                                tcu.bwe.MainButtonEnabled = arcaniaModel.Runner.CanStartAction(data);
                                if (tcu.TaskClicked)
                                {
                                    arcaniaModel.Runner.StartActionExternally(data);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

        }

        #region Sub unit update
        controlExploration.ManualUpdate();
        #endregion

        static void FeedMods(RuntimeUnit data, ModsControlUnit modUnit)
        {
            for (int i = 0; i < data.ModsOwned.Count; i++)
            {
                ModRuntime md = data.ModsOwned[i];

                var ttv = modUnit.ModTTVs[i];
                ttv.LayoutChild.Visible = md.ModType != ModType.Lock && ttv.LayoutChild.Visible;

                ttv.MainText.rawText = md.HumanText;

                if (md.Value > 0 && md.ModType != ModType.SpaceConsumption)
                    ttv.SecondaryText.rawText = $"+{md.Value}";
                else
                    ttv.SecondaryText.rawText = $"{md.Value}";
                ttv.TertiaryText.rawText = string.Empty;
                ttv.ManualUpdate();
            }
        }
    }

    private void GoToSettings()
    {
        ArcaniaPersistence.Save(arcaniaModel.arcaniaUnits, arcaniaModel.Exploration);
        HeartGame.GoToSettings();
    }
}
