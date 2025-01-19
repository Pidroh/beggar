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
    public EndingControl endingControl = new();

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

    public float lastSaveTime;
    public ControlExploration controlExploration;
    public int SkillFontSize;

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
        endingControl.ManualUpdate(this);
        
        // -----------------------------------------------------------
        // Time, game updating
        // -----------------------------------------------------------
        RobustDeltaTime.ManualUpdate();
        RobustDeltaTime.MultiplyTime(TimeMultiplier);
        while (RobustDeltaTime.TryGetProcessedDeltaTime(out float dt))
        {
            arcaniaModel.ManualUpdate(Time.deltaTime);
        }
        // -----------------------------------------------------------
        // UI update
        // -----------------------------------------------------------
        dynamicCanvas.ManualUpdate();
        // hide lower menu if all the tabs are visible
        dynamicCanvas.LowerMenus[0].SelfChild.VisibleSelf = dynamicCanvas.CalculateNumberOfVisibleHorizontalChildren() < arcaniaModel.arcaniaUnits.datas[UnitType.TAB].Count;
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
            tabControl.Dirty = dynamicCanvas.WidthChangedThisFrame;
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
            tabControl.SelectionButtonLayoutChild.VisibleSelf = tabControl.TabData.Visible;
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
                    bool visible = data.Visible && tabNormalContentVisible;
                    tcu.SetVisible(visible);

                    if (!visible) continue;
                    tcu.ManualUpdate(arcaniaModel);

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
            foreach (var sep in tabControl.Separators)
            {
                sep.ApplyVisible();
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
                float value = md.Value;
                var ttv = modUnit.ModTTVs[i];
                string rawText = md.HumanText;
                FeedModView(md, value, ttv, rawText);
            }
            if (modUnit.ExtraModSeparator == null) return;
            var modsAsIntermediaryVisible = false;
            for (int i = 0; i < data.ModsSelfAsIntermediary.Count; i++)
            {
                ModRuntime modRuntime = data.ModsSelfAsIntermediary[i];
                var ttv = modUnit.ModIntermediaryTTVs[i];
                if (modRuntime.Source.Value <= 0) 
                {
                    ttv.Visible = false;
                    continue;
                }
                FeedModView(modRuntime, modRuntime.Source.Value * modRuntime.Value, ttv, modRuntime.HumanTextIntermediary);
                modsAsIntermediaryVisible = true;
            }
            modUnit.ExtraModSeparator.LayoutChild.VisibleSelf = modsAsIntermediaryVisible;

            static void FeedModView(ModRuntime md, float value, TripleTextView ttv, string rawText)
            {
                ttv.LayoutChild.VisibleSelf = md.ModType != ModType.Lock && ttv.LayoutChild.Visible;
                ttv.MainText.rawText = rawText;
                if (value > 0 && md.ModType != ModType.SpaceConsumption)
                    ttv.SecondaryText.rawText = $"+{value}";
                else
                    ttv.SecondaryText.rawText = $"{value}";
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

public class EndingControl 
{
    public const int ENDING_COUNT = 2;
    public static string[] endingUnitIds = new string[ENDING_COUNT] { "ponderexistence", "ponderhappiness" };
    public static string[] endingPrefix = new string[ENDING_COUNT] { "You have become one with existence", "You are seeking happiness with your cats" };
    public static string[] endingMessageSnippet = new string[ENDING_COUNT] { "I'm the beggar's journey", "The beggar's journey is the cat" };
    public static string endingMessage = "GAME CLEARED \n$PART1$. \n At least until more content is added. \n\n Let me know you finished the game by sending me: \"$PART2$\".\n\n\n You can comment on the Reddit post, email, the Discord channel, etc";
    public RuntimeUnit[] runtimeUnits = new RuntimeUnit[ENDING_COUNT] { null, null };
    public UIUnit EndGameMessage { get; internal set; }

    internal void ManualUpdate(MainGameControl mainGameControl)
    {
        TryShowEnding(mainGameControl);
    }

    private void TryShowEnding(MainGameControl mainGameControl)
    {
        var dynamicCanvas = mainGameControl.dynamicCanvas;
        if (dynamicCanvas == null) return;
        if (dynamicCanvas.OverlayVisible) return;
        for (int i = 0; i < runtimeUnits.Length; i++)
        {
            RuntimeUnit ru = runtimeUnits[i];
            if (ru.Value <= 0) continue;
            dynamicCanvas.ShowOverlay();
            var message = endingMessage;
            message = message.Replace("$PART1$", endingPrefix[i]).Replace("$PART2$", endingMessageSnippet[i]);
            EndGameMessage.rawText = message;
            return;
        }
    }
}