using HeartEngineCore;
using HeartUnity;
using HeartUnity.View;
using JLayout;
using System.Collections.Generic;
using UnityEngine;

public class MainGameControlSetupJLayout
{

    internal static void SetupModelDataAllAtOnce(MainGameControl mgc)
    {
        var arcaniaModel = mgc.arcaniaModel;
        var arcaniaDatas = arcaniaModel.arcaniaUnits;
        bool hasLocalizationFileArcania;
        SetupLocalizationSingleStep(mgc, out hasLocalizationFileArcania);
        JsonReader.ReadJsonAllAtOnce(mgc.ResourceJson, arcaniaModel, hasLocalizationFileArcania && !Local.IsFirstLanguage);
        arcaniaModel.FinishedSettingUpUnits();
    }

    public static void SetupLocalizationSingleStep(MainGameControl mgc, out bool hasLocalizationFileArcania)
    {
        hasLocalizationFileArcania = mgc.ResourceJson.arcaniaTranslationFile.TryGetText(out var localizedText);
        if (hasLocalizationFileArcania)
        {
            Local.Instance.AppendLocalizationData(localizedText, true);
        }
    }

    public static JGameControlDataHolder SetupCanvas(MainGameControl mgc)
    {
        JGameControlDataHolder jControlDataHolder = new();
        {
            JLayoutRuntimeData runtime = new();
            LayoutDataMaster layoutMaster = new LayoutDataMaster();
            JsonInterpreter.ReadJson(mgc.ResourceJson.layoutJson.text, layoutMaster);
            var arcaniaModel = mgc.arcaniaModel;
            var arcaniaDatas = arcaniaModel.arcaniaUnits;
            var config = HeartGame.GetConfig();
            //runtime.DefaultFont = mgc.Font;
            runtime.DefaultFont = mgc.FontGroup.GetFont(Local.Instance.Lang.languageName).fontAsset;
            runtime.ImageSprites = mgc.ResourceJson.spritesForLayout;
            #region identify color scheme
            for (int i = 0; i < mgc.HeartGame.config.SettingCustomChoices.Count; i++)
            {
                SettingCustomChoice item = mgc.HeartGame.config.SettingCustomChoices[i];
                if (item.id == "LAYOUT_COLOR_SCHEME")
                {
                    var positionOfChoice = i;
                    // for now, has to be the first choice
                    Debug.Assert(positionOfChoice == 0);
                    foreach (var unit in mgc.HeartGame.settingModel.unitControls)
                    {
                        if (unit.settingData.standardSettingType == SettingModel.SettingUnitData.StandardSettingType.CUSTOM_CHOICE_1)
                        {
                            var chosenScheme = unit.rtInt;
                            chosenScheme = Mathf.Clamp(chosenScheme, 0, item.choiceKeys.Count - 1);
                            runtime.CurrentColorSchemeId = chosenScheme;
                        }
                    }
                }
            }
            #endregion

            //var jCanvas = JCanvasMaker.CreateCanvas(Mathf.Max(arcaniaDatas.datas.ContainsKey(UnitType.TAB) == false ? 1 : arcaniaDatas.datas[UnitType.TAB].Count, 1), mgc.CanvasRequest, config.reusableCanvas, runtime);
            var jCanvas = JCanvasMaker.CreateCanvas(15, mgc.CanvasRequest, config.reusableCanvas, runtime);

            mgc.JLayoutRuntime = runtime;
            runtime.LayoutMaster = layoutMaster;

            Camera.main.backgroundColor = runtime.LayoutMaster.General.BackgroundColor.data.Colors[runtime.CurrentColorSchemeId];


            jControlDataHolder.LayoutRuntime = runtime;
            jControlDataHolder.gameViewMiscData.ButtonColorDotActive = layoutMaster.ColorDatas.GetData("task_button_buff");
            jControlDataHolder.gameViewMiscData.ButtonColorDotActive_bar = layoutMaster.ColorDatas.GetData("task_button_buff_bar");
            // var dynamicCanvas = CanvasMaker.CreateCanvas(Mathf.Max(arcaniaDatas.datas[UnitType.TAB].Count, 1), mgc.CanvasRequest, config.reusableCanvas);
            //mgc.dynamicCanvas = dynamicCanvas;

            #region localized strings
            jControlDataHolder.LabelDuration = Local.GetText("Duration");
            jControlDataHolder.LabelEffectDuration = Local.GetText("Effect duration");

            jControlDataHolder.LabelAcquire = Local.GetText("Acquire", "A word for learning or becoming able to do something new");
            jControlDataHolder.LabelDeactivate = Local.GetText("Deactivate");

            jControlDataHolder.LabelModifications = Local.GetText("modifications");
            jControlDataHolder.LabelModificationsExtra = Local.GetText("extra mods");
            jControlDataHolder.LabelModificationsTargeting = Local.GetText("mods targeting this");
            jControlDataHolder.LabelModificationsExtraEffect = Local.GetText("extra effect mods");
            jControlDataHolder.LabelSuccessRate = Local.GetText("Success rate");
            jControlDataHolder.LabelGoToArchive = Local.GetText("Go to archive");
            jControlDataHolder.LabelCost = Local.GetText("cost");
            jControlDataHolder.LabelUnlocked = Local.GetText("Unlocked", "in the sense of gaining access to a new feature, resource or task");
            jControlDataHolder.LabelSpace = Local.GetText("Space", "In the sense of a table taking up too much space");
            jControlDataHolder.LabelResult = Local.GetText("result");
            jControlDataHolder.LabelRun = Local.GetText("run");
            jControlDataHolder.LabelEffect = Local.GetText("effect");
            jControlDataHolder.LabelResultOnce = Local.GetText("first time");
            jControlDataHolder.LabelResultFail = Local.GetText("result failure");
            jControlDataHolder.LabelBuy = Local.GetText("acquisition");
            jControlDataHolder.LabelAcquireSkill = Local.GetText("Acquire Skill");
            jControlDataHolder.LabelPracticeSkill = Local.GetText("Practice Skill");
            jControlDataHolder.LabelLivingHere = Local.GetText("living here");

            jControlDataHolder.SuccessRateLabel = Local.GetText("success rate", "In the sense of a probability of success");
            jControlDataHolder.SpeedLabel = Local.GetText("Speed");
            jControlDataHolder.RateLabel = Local.GetText("Rate", "in the sense of something gained over time (translate as a single word if possible though)");
            jControlDataHolder.LabelMaxSpace = Local.GetText("Max Space", "In the sense of how much space you have in your room to house things");
            jControlDataHolder.spaceOccuppiedLabel = Local.GetText("Space Occupied", "In the sense of a table taking up too much space");

            jControlDataHolder.ColorForResourceChangeType[ResourceChangeType.COST] = layoutMaster.ColorDatas.GetData("change_cost_color");
            jControlDataHolder.ColorForResourceChangeType[ResourceChangeType.EFFECT] = layoutMaster.ColorDatas.GetData("change_effect_color");
            jControlDataHolder.ColorForResourceChangeType[ResourceChangeType.RUN] = layoutMaster.ColorDatas.GetData("change_run_color");
            jControlDataHolder.ColorForResourceChangeType[ResourceChangeType.RESULT] = layoutMaster.ColorDatas.GetData("change_result_color");

            jControlDataHolder.ColorForModType[ModType.MaxChange] = jControlDataHolder.ColorForResourceChangeType[ResourceChangeType.RESULT];
            jControlDataHolder.ColorForModType[ModType.RateChange] = jControlDataHolder.ColorForResourceChangeType[ResourceChangeType.EFFECT];
            jControlDataHolder.ColorForModType[ModType.Speed] = jControlDataHolder.ColorForResourceChangeType[ResourceChangeType.EFFECT];
            jControlDataHolder.ColorForModType[ModType.SuccessRate] = jControlDataHolder.ColorForResourceChangeType[ResourceChangeType.RESULT];
            jControlDataHolder.ColorForModType[ModType.SpaceConsumption] = jControlDataHolder.ColorForResourceChangeType[ResourceChangeType.COST];


            jControlDataHolder.LabelConfirmSlotDeleteTitle = Local.GetText("Delete save slot");
            jControlDataHolder.LabelConfirmSlotDeleteContent = Local.GetText("Data cannot be recovered");
            jControlDataHolder.LabelGoToSaveSlots = Local.GetText("Save slots", "As in save slots on a memory card, games context, saving data");
            jControlDataHolder.LabelUnitTypeDescription[UnitType.RESOURCE] = Local.GetText("resource");
            jControlDataHolder.LabelUnitTypeDescription[UnitType.SKILL] = Local.GetText("skill");
            jControlDataHolder.LabelUnitTypeDescription[UnitType.CLASS] = Local.GetText("class");
            jControlDataHolder.LabelUnitTypeDescription[UnitType.FURNITURE] = Local.GetText("house item");
            jControlDataHolder.LabelUnitTypeDescription[UnitType.HINT] = Local.GetText("hint");
            jControlDataHolder.LabelUnitTypeDescription[UnitType.HOUSE] = Local.GetText("house");
            jControlDataHolder.LabelUnitTypeDescription[UnitType.LOCATION] = Local.GetText("location");
            jControlDataHolder.LabelUnitTypeDescription[UnitType.TASK] = Local.GetText("task");
            //jControlDataHolder.LabelArchiveHeuristicLabel[ArcaniaArchiveModelData.ArchiveEuristics.Encounters] = Local.GetText("Encounters");
            jControlDataHolder.LabelArchiveHeuristicLabel[ArcaniaArchiveModelData.ArchiveEuristics.Furnitures] = Local.GetText("Furniture");
            jControlDataHolder.LabelArchiveHeuristicLabel[ArcaniaArchiveModelData.ArchiveEuristics.Houses] = Local.GetText("Houses");
            jControlDataHolder.LabelArchiveHeuristicLabel[ArcaniaArchiveModelData.ArchiveEuristics.Locations] = Local.GetText("Locations");
            jControlDataHolder.LabelArchiveHeuristicLabel[ArcaniaArchiveModelData.ArchiveEuristics.Powerups] = Local.GetText("Power-ups");
            jControlDataHolder.LabelArchiveHeuristicLabel[ArcaniaArchiveModelData.ArchiveEuristics.Resources] = Local.GetText("Resources");
            jControlDataHolder.LabelArchiveHeuristicLabel[ArcaniaArchiveModelData.ArchiveEuristics.Skills] = Local.GetText("Skills");
            jControlDataHolder.LabelArchiveHeuristicLabel[ArcaniaArchiveModelData.ArchiveEuristics.Tasks] = Local.GetText("Tasks");
            jControlDataHolder.LabelArchiveHeuristicLabel[ArcaniaArchiveModelData.ArchiveEuristics.Total] = Local.GetText("Total");

            jControlDataHolder.LabelDuration = Local.GetText("Duration");

            #endregion

            mgc.EngineView = mgc.HeartGame.CreateEngineView(new EngineView.EngineViewInitializationParameter()
            {
                canvas = jCanvas.Canvas,
                DisableAutoScaling = true

            }, 2);

        }
        mgc.JControlData = jControlDataHolder;

        return jControlDataHolder;
    }

    internal static void SetupGameCanvasAllAtOnce(MainGameControl mgc)
    {

        SetupGameCanvasTabMenuInstantiation(mgc, false);
        SetupGameCanvasMainRuntimeUnits(mgc, default);
        SetupGameCanvasMisc(mgc);
    }

    public static void SetupGameCanvasMisc(MainGameControl mgc)
    {
        var jControlDataHolder = mgc.JControlData;
        var runtime = jControlDataHolder.LayoutRuntime;
        var arcaniaModel = mgc.arcaniaModel;
        var arcaniaDatas = arcaniaModel.arcaniaUnits;
        var jCanvas = runtime.jLayCanvas;
        var layoutMaster = runtime.LayoutMaster;

        #region instantiating exploration graphics

        for (int tabIndex = 0; tabIndex < jControlDataHolder.TabControlUnits.Count; tabIndex++)
        {
            var tab = jControlDataHolder.TabControlUnits[tabIndex];
            if (!tab.TabData.Tab.ExplorationActiveTab) continue;
            var tabHolder = jCanvas.children[tabIndex];

            for (int indexExplorationElement = 0; indexExplorationElement < 2; indexExplorationElement++)
            {

                var parent = JCanvasMaker.CreateLayout("content_holder_expandable", runtime);
                jControlDataHolder.Exploration.ExplorationModeLayouts.Add(parent);
                var layoutRU = parent;
                tabHolder.LayoutRuntimeUnit.AddLayoutAsChild(parent);
                parent.DefaultPositionModes = new PositionMode[] { PositionMode.LEFT_ZERO, PositionMode.SIBLING_DISTANCE };
                parent.ChildSelf.Rect.gameObject.name += " " + (indexExplorationElement == 0 ? "area" : "encounter");
                var expandableTextWithBar = parent.AddLayoutAsChild(JCanvasMaker.CreateLayout("exploration_progress_part_expandable", runtime));
                JLayoutRuntimeUnit layoutThatHasName = expandableTextWithBar.LayoutRU.Children[0].LayoutRU;
                layoutThatHasName.SetTextRaw(0, (indexExplorationElement == 0 ? "area" : "encounter"));
                JRTControlUnit jCU = new();
                jCU.ExpandButton = new JButtonAccessor(expandableTextWithBar.LayoutRU, 0);
                jCU.ExpandButtonImage = new JImageAccessor(expandableTextWithBar.LayoutRU.ButtonChildren[0].Item1, 0);
                jCU.GaugeProgressImage = new JImageAccessor(expandableTextWithBar.LayoutRU.Children[0].LayoutRU.Children[1].LayoutRU, 1);
                jCU.Name = new JLayTextAccessor(layoutThatHasName, 0);
                jCU.ExpandWhenClickingLayout = expandableTextWithBar.LayoutRU;
                jCU.MainLayout = parent;

                {
                    var descLayout = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("lore_text"), runtime);
                    jCU.Description = new JLayTextAccessor(descLayout, 0);
                    AddToExpand(descLayout);
                }
                void AddToExpand(JLayoutRuntimeUnit unit)
                {
                    layoutRU.AddLayoutAsChild(unit);
                    jCU.InsideExpandable.Add(unit);
                    unit.SetParentShowing(false);
                }
                if (indexExplorationElement == 0)
                    jControlDataHolder.Exploration.AreaJCU = jCU;
                if (indexExplorationElement == 1)
                    jControlDataHolder.Exploration.EncounterJCU = jCU;
            }
            var playerParent = JCanvasMaker.CreateLayout("content_holder_expandable", runtime);
            playerParent.DefaultPositionModes = new PositionMode[] {
                PositionMode.CENTER,
                PositionMode.SIBLING_DISTANCE
            };
            jControlDataHolder.Exploration.ExplorationModeLayouts.Add(playerParent);
            tabHolder.LayoutRuntimeUnit.AddLayoutAsChild(playerParent);
            var label = playerParent.AddLayoutAsChild(JCanvasMaker.CreateLayout("exploration_player_upper_label", runtime));
            label.LayoutRU.SetTextRaw(0, "Player");
            foreach (var item in mgc.arcaniaModel.Exploration.Stressors)
            {
                var labelWithBar = JCanvasMaker.CreateLayout("exploration_progress_player_stat", runtime);
                JRTControlUnit jCU = new();
                jCU.GaugeProgressImage = new JImageAccessor(labelWithBar.Children[1].LayoutRU, 1);
                labelWithBar.SetTextRaw(0, item.Name);
                //jCU.Name = new JLayTextAccessor(labelWithBar, 0);
                jCU.MainLayout = labelWithBar;
                playerParent.AddLayoutAsChild(jCU.MainLayout);
                jControlDataHolder.Exploration.StressorJCUs.Add(jCU);
                jCU.Data = item;
            }
            {
                var fleeButtonLayout = JCanvasMaker.CreateLayout("exploration_simple_button", runtime);
                var lc = playerParent.AddLayoutAsChild(fleeButtonLayout);
                fleeButtonLayout.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("Flee"));
                JRTControlUnit jCU = new();
                jCU.MainLayout = fleeButtonLayout;
                jCU.MainExecuteButton = new JButtonAccessor(fleeButtonLayout, 0);
                fleeButtonLayout.ButtonChildren[0].Item1.ImageChildren[1].UiUnit.ActiveSelf = false;
                jControlDataHolder.Exploration.FleeButtonJCU = jCU;
            }
        }

        #endregion

        #region save slot instantiation
        for (int tabIndex = 0; tabIndex < jControlDataHolder.TabControlUnits.Count; tabIndex++)
        {
            var tab = jControlDataHolder.TabControlUnits[tabIndex];
            foreach (var sep in tab.SeparatorControls)
            {
                if (!sep.SepD.ContainsSaveSlots) continue;
                var tabHolder = jCanvas.children[tabIndex];
                jControlDataHolder.SaveSlots.FileUtilities = new FileUtilities();
                int nSlots = 3;
                for (int slotIndex = 0; slotIndex < nSlots; slotIndex++)
                {
                    JGameControlDataSaveSlot.ControlSaveSlotUnit unit = new();
                    jControlDataHolder.SaveSlots.saveSlots.Add(unit);
                    var layoutD = layoutMaster.LayoutDatas.GetData("content_holder_expandable");
                    JLayoutRuntimeUnit layoutRU = JCanvasMaker.CreateLayout(layoutD, runtime);

                    layoutRU.DefaultPositionModes = new PositionMode[] { PositionMode.LEFT_ZERO, PositionMode.SIBLING_DISTANCE };

                    var slotExpandable = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("expandable_resource_text"), runtime);

                    tabHolder.LayoutRuntimeUnit.AddLayoutAsChild(layoutRU);
                    //tabHolder.LayoutRuntimeUnit.AddLayoutAsChild(slotExpandable);
                    layoutRU.AddLayoutAsChild(slotExpandable);

                    JRTControlUnit jCUSlot = new();
                    jControlDataHolder.SaveSlots.slotControlUnits.Add(jCUSlot);
                    jCUSlot.ExpandWhenClickingLayout = slotExpandable;
                    jCUSlot.ExpandButton = new JButtonAccessor(slotExpandable, 0);
                    jCUSlot.ExpandButtonImage = new JImageAccessor(slotExpandable.ButtonChildren[0].Item1, 0);
                    unit.TextForFlavor = new JLayTextAccessor(slotExpandable, 0);
                    unit.TextForTimeStuff = new JLayTextAccessor(slotExpandable, 1);
                    for (int i = 0; i < 5; i++)
                    {
                        bool importButtonCreation = i == 2;
                        // can't import into the current slot
                        if (importButtonCreation && slotIndex == mgc.JControlData.SaveSlots.ModelData.currentSlot)
                        {
                            continue;
                        }
                        var tempSlotButton = JCanvasMaker.CreateLayout("exploration_simple_button", runtime);
                        jCUSlot.MainLayout = tempSlotButton;
                        layoutRU.AddLayoutAsChild(tempSlotButton);
                        jCUSlot.InsideExpandable.Add(tempSlotButton);
                        tempSlotButton.SetParentShowing(false);
                        JRTControlUnit jCU = new();
                        jCU.MainLayout = tempSlotButton;
                        jCU.MainExecuteButton = new JButtonAccessor(tempSlotButton, 0);
                        tempSlotButton.ButtonChildren[0].Item1.ImageChildren[1].UiUnit.ActiveSelf = false;

                        switch (i)
                        {
                            case 0:
                                {
                                    tempSlotButton.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("new game"));
                                    unit.newGameOrLoadGameButton = jCU;


                                }
                                break;
                            case 1:
                                {
                                    tempSlotButton.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("export"));
                                    unit.exportButton = jCU;
                                }
                                break;
                            case 2:
                                {
                                    tempSlotButton.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("import"));
                                    unit.importButton = jCU;
                                }
                                break;
                            case 3:
                                {
                                    tempSlotButton.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("delete"));
                                    unit.deleteButton = jCU;
                                }
                                break;
                            case 4:
                                {
                                    tempSlotButton.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("copy"));
                                    unit.copyButton = jCU;
                                }
                                break;
                            default:
                                break;
                        }
                    }


                }
                var goToArchiveButton = JCanvasMaker.CreateLayout("exploration_simple_button", runtime);
                goToArchiveButton.ButtonChildren[0].Item1.ImageChildren[1].UiUnit.ActiveSelf = false;
                jControlDataHolder.SaveSlots.ArchiveButtonLayout = goToArchiveButton;
                jControlDataHolder.SaveSlots.ArchiveButton = new JButtonAccessor(goToArchiveButton, 0);
                goToArchiveButton.ButtonChildren[0].Item1.SetTextRaw(0, jControlDataHolder.LabelGoToArchive);
                tabHolder.LayoutRuntimeUnit.AddLayoutAsChild(goToArchiveButton);
            }


        }
        #endregion

        #region instantiating dialog stuff
        var overlay = jCanvas.Overlays[0];
        {
            jCanvas.overlayImage.color = runtime.LayoutMaster.General.OverlayColor.data.Colors[runtime.CurrentColorSchemeId];

            var dialogLay = overlay.LayoutRuntimeUnit.AddLayoutAsChild(JCanvasMaker.CreateLayout("dialog_yes_no", runtime));
            dialogLay.LayoutRU.SetTextRaw(0, "Dialog title");
            dialogLay.LayoutRU.SetTextRaw(1, "Dialog text");
            dialogLay.LayoutRU.LayoutChildren[0].LayoutRU.ButtonChildren[0].Item1.SetTextRaw(0, "Yes");
            dialogLay.LayoutRU.LayoutChildren[0].LayoutRU.ButtonChildren[1].Item1.SetTextRaw(0, "No");
            jControlDataHolder.DialogLayout = dialogLay;
        }
        #endregion

        #region setup ending stuff
        for (int i = 0; i < JGameControlExecuterEnding.ENDING_COUNT; i++)
        {
            var unit = arcaniaModel.FindRuntimeUnit(JGameControlExecuterEnding.endingUnitIds[i]);
            jControlDataHolder.EndingData.runtimeUnits[i] = unit;
        }
        {

            var endingLay = overlay.LayoutRuntimeUnit.AddLayoutAsChild(JCanvasMaker.CreateLayout("ending_text", runtime));
            endingLay.LayoutRU.SetTextRaw(0, "Ending title");
            endingLay.LayoutRU.SetTextRaw(1, "Ending normal");
            jControlDataHolder.EndingLayout = endingLay;
            endingLay.LayoutRU.SetVisibleSelf(false);
            endingLay.LayoutRU.LayoutChildren[0].LayoutRU.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("Settings"));
#if UNITY_STANDALONE_WIN
            endingLay.LayoutRU.LayoutChildren[1].LayoutRU.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("Wishlist on Steam"));
#else
            endingLay.LayoutRU.LayoutChildren[1].LayoutRU.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("Demo on Steam", "Steam as in the PC game store"));
#endif
            endingLay.LayoutRU.LayoutChildren[2].LayoutRU.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("Latest Version on Patreon"));
            endingLay.LayoutRU.LayoutChildren[3].LayoutRU.ButtonChildren[0].Item1.SetTextRaw(0, jControlDataHolder.LabelGoToSaveSlots);
            // archive disabled for now
            endingLay.LayoutRU.LayoutChildren[4].LayoutRU.SetVisibleSelf(false);

            jControlDataHolder.EndingData.SettingsButton = endingLay.LayoutRU.LayoutChildren[0].LayoutRU.ButtonChildren[0];
            jControlDataHolder.EndingData.SteamButton = endingLay.LayoutRU.LayoutChildren[1].LayoutRU.ButtonChildren[0];
            jControlDataHolder.EndingData.PatreonButton = endingLay.LayoutRU.LayoutChildren[2].LayoutRU.ButtonChildren[0];
            jControlDataHolder.EndingData.SaveSlotButton = endingLay.LayoutRU.LayoutChildren[3].LayoutRU.ButtonChildren[0];
            jControlDataHolder.EndingData.ArchiveButton = endingLay.LayoutRU.LayoutChildren[4].LayoutRU.ButtonChildren[0];
        }
        #endregion

        #region hover
        JGameHoverSetup.Setup(mgc);
        #endregion
        JGameControlExecuter.HideOverlay(mgc);
        mgc.JControlData.DialogLayout.LayoutRU.SetVisibleSelf(false);
    }

    public struct SetupGameCanvasMainRuntimeUnitConfig
    {
        public bool ArchiveMode { get; internal set; }
        public WorldType WorldType;
    }

    public static void SetupGameCanvasMainRuntimeUnits(MainGameControl mgc, SetupGameCanvasMainRuntimeUnitConfig config)
    {
        var jControlDataHolder = mgc.JControlData;
        var runtime = jControlDataHolder.LayoutRuntime;
        var arcaniaModel = mgc.arcaniaModel;
        var jCanvas = runtime.jLayCanvas;
        var layoutMaster = runtime.LayoutMaster;
        var world = JGameControlExecuter.GetWorld(mgc);
        {
            var modReplacements = new Dictionary<ModType, (string key, string value, string valueWithColor, ColorData)>();
            mgc.JControlData.ModReplacements = modReplacements;
            FeedModKey(ModType.MaxChange, ModReplaceKeys.MAX, "Max");
            FeedModKey(ModType.RateChange, ModReplaceKeys.RATE, mgc.JControlData.RateLabel);
            FeedModKey(ModType.SpaceConsumption, ModReplaceKeys.SPACEOCCUPIED, mgc.JControlData.spaceOccuppiedLabel);
            FeedModKey(ModType.Speed, ModReplaceKeys.SPEED, mgc.JControlData.SpeedLabel);
            FeedModKey(ModType.SuccessRate, ModReplaceKeys.SUCCESSRATE, mgc.JControlData.SuccessRateLabel);
            // FeedModKey(ModType., ModReplaceKeys.MAXSPACE, mgc.JControlData.LabelMaxSpace);
            // modReplacements[ModType.MaxChange]

            void FeedModKey(ModType modType, string replaceKey, string replaceValue)
            {
                string valueWithColor;
                ColorData cd = null;

                if (mgc.JControlData.ColorForModType.TryGetValue(modType, out var c))
                {
                    cd = c;
                    valueWithColor = $"<color={c.CodeCache[mgc.JLayoutRuntime.CurrentColorSchemeId]}>{replaceValue}</color>";
                }
                else
                {
                    valueWithColor = replaceValue;
                }

                modReplacements[modType] = (replaceKey, replaceValue, valueWithColor, cd);
            }
        }
        #region main default setup of runtime units and separators
        for (int tabIndex = 0; tabIndex < jControlDataHolder.TabControlUnits.Count; tabIndex++)
        {
            var parentOfTabContent = jCanvas.children[tabIndex].LayoutRuntimeUnit;
            var tabControl = jControlDataHolder.TabControlUnits[tabIndex];
            JTabControlUnit jTabControl = tabControl;
            foreach (var separatorControl in tabControl.SeparatorControls)
            {
                #region Separator graphic instantiation
                {
                    var layoutD = layoutMaster.LayoutDatas.GetData("expandable_upper_header");
                    JLayoutRuntimeUnit layoutRU = JCanvasMaker.CreateLayout(layoutD, runtime);
                    parentOfTabContent.AddLayoutAsChild(layoutRU);
                    layoutRU.SetTextRaw(0, separatorControl.SepD.Name);
                    separatorControl.SeparatorLayout = layoutRU;
                    layoutRU.ImageChildren[0].Rect.transform.localScale = new Vector3(1, -1, 1);
                }
                #endregion
                if (separatorControl.SepD.ShowSpace)
                {
                    var child = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("above_button_title_with_value"), runtime);
                    tabControl.SpaceShowLayout = parentOfTabContent.AddLayoutAsChild(child).LayoutRU;
                }
                #region instantiating each unit in a separator
                foreach (var modelData in separatorControl.SepD.BoundRuntimeUnits)
                {
                    if (modelData.World != world) continue;
                    if (modelData.ForceInvisible) continue;
                    var jCU = new JRTControlUnit();
                    bool archiveMode = config.ArchiveMode;
                    // special types that don't have unit group controls are handled in a special way
                    UnitType unitType = modelData.ConfigBasic.UnitType;
                    if (!separatorControl.UnitGroupControls.TryGetValue(unitType, out var list)) continue;
                    list.Add(jCU);
                    jCU.Data = modelData;

                    var id = modelData.ConfigBasic.Id;
                    var layoutD = layoutMaster.LayoutDatas.GetData("content_holder_expandable");
                    JLayoutRuntimeUnit layoutRU = JCanvasMaker.CreateLayout(layoutD, runtime);
                    jCU.MainLayout = layoutRU;
                    layoutRU.RectTransform.gameObject.name += " " + id;
                    layoutRU.DefaultPositionModes = new PositionMode[] { PositionMode.LEFT_ZERO, PositionMode.SIBLING_DISTANCE };

                    var childOfParent = parentOfTabContent.AddLayoutAsChild(layoutRU);
                    if(modelData.Visible || archiveMode)
                        CreateUIOfControlUnit(runtime, jCU, archiveMode);
                }
                #endregion

            }
        }
        #endregion
    }

    public static void CreateUIOfControlUnit(JLayoutRuntimeData runtime, JRTControlUnit jCU, bool archiveMode)
    {
        jCU.initedUi = true;
        var layoutMaster = runtime.LayoutMaster;
        var modelData = jCU.Data;
        var unitType = modelData.ConfigBasic.UnitType;

        var layoutRU = jCU.MainLayout;
        if (!layoutRU.RectTransform.gameObject.TryGetComponent<UIUnit>(out var uiUnit)) 
        {
            uiUnit = layoutRU.RectTransform.gameObject.AddComponent<UIUnit>();
        }
        layoutRU.ChildSelf.UiUnit = uiUnit;

        var hasTaskButton = !archiveMode &&
            (unitType == UnitType.TASK || unitType == UnitType.CLASS || unitType == UnitType.SKILL || unitType == UnitType.HOUSE || unitType == UnitType.LOCATION);
        var hasTitleWithValue = unitType == UnitType.SKILL;
        var hasXPBar = !archiveMode && unitType == UnitType.SKILL;
        var hasResourceExpander = !hasTaskButton && ((unitType == UnitType.RESOURCE || unitType == UnitType.FURNITURE || unitType == UnitType.HINT) || archiveMode && unitType != UnitType.SKILL);
        var hasPlusMinusButton = !archiveMode && unitType == UnitType.FURNITURE;
        var showRequireOfTarget = unitType == UnitType.HINT;
        var valueVisible = !archiveMode && unitType != UnitType.HINT;
        var disableValueTextForcefully = archiveMode;

        if (hasTitleWithValue)
        {
            var titleRU = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("above_button_title_with_value"), runtime);
            var child = layoutRU.AddLayoutAsChild(titleRU);
            titleRU.SetTextRaw(0, modelData.Name);
            jCU.TitleWithValue = titleRU;
        }
        if (hasXPBar)
        {
            var child = layoutRU.AddLayoutAsChild(JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("xp_bar"), runtime));
            jCU.GaugeLayout = child.LayoutRU;
            jCU.GaugeProgressImage = new JImageAccessor(child.LayoutRU, 1);
        }

        if (hasTaskButton)
        {
            var buttonLayoutRU = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("expandable_task_main_buttons"), runtime);
            layoutRU.AddLayoutAsChild(buttonLayoutRU);
            buttonLayoutRU.ButtonChildren[0].Item1.SetTextRaw(0, modelData.Name);
            jCU.TitleText = new JLayTextAccessor(buttonLayoutRU.ButtonChildren[0].Item1, 0);
            jCU.MainExecuteButton = new JButtonAccessor(buttonLayoutRU, 0);
            jCU.ExpandButton = new JButtonAccessor(buttonLayoutRU, 1);
            jCU.ExpandButtonImage = new JImageAccessor(buttonLayoutRU.ButtonChildren[1].Item1, 0);
            jCU.ButtonImageMain = new JImageAccessor(buttonLayoutRU.ButtonChildren[0].Item1, 0);
            jCU.ButtonImageProgress = new JImageAccessor(buttonLayoutRU.ButtonChildren[0].Item1, 1);
            if ((!modelData.ConfigTask.Duration.HasValue || modelData.ConfigTask.Duration <= 0) && modelData.Skill == null)
            {
                // buttonLayoutRU.ButtonChildren[0].Item1.ImageChildren[1].UiUnit.ActiveSelf = modelData.ConfigBasic.UnitType == UnitType.HOUSE;
                buttonLayoutRU.ButtonChildren[0].Item1.ImageChildren[1].UiUnit.ActiveSelf = false;
            }
            {
                var quantityLay = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("quantity_task_text"), runtime);
                quantityLay.SetTextRaw(0, "0");
                jCU.TaskQuantityText = new JLayTextAccessor(quantityLay, 0);
                jCU.SuccessRateAndDurationText = new JLayTextAccessor(quantityLay, 1);
                AddToExpand(layoutRU, quantityLay, jCU);
            }
        }
        else if (hasResourceExpander)
        {
            var resourceLayoutRU = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("expandable_resource_text"), runtime);
            layoutRU.AddLayoutAsChild(resourceLayoutRU);
            resourceLayoutRU.SetTextRaw(0, modelData.Name);
            jCU.ValueText = new JLayTextAccessor(resourceLayoutRU, 1);
            jCU.ExpandButton = new JButtonAccessor(resourceLayoutRU, 0);
            jCU.ExpandButtonImage = new JImageAccessor(resourceLayoutRU.ButtonChildren[0].Item1, 0);
            jCU.ExpandWhenClickingLayout = resourceLayoutRU;
            if (disableValueTextForcefully)
            {
                resourceLayoutRU.TextChildren[1].UiUnit.text.enabled = false;
            }
        }

        if (hasPlusMinusButton)
        {
            var child = layoutRU.AddLayoutAsChild(JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("furniture_buttons"), runtime));
            jCU.PlusMinusLayout = child.LayoutRU;
            child.LayoutRU.ButtonChildren[0].Item1.SetTextRaw(0, "+");
            child.LayoutRU.ButtonChildren[1].Item1.SetTextRaw(0, "-");
        }

        // TODO description used to be here
        // TODO change list instantiation used to be here
        // MOD instantiation used to be here
        #region need
        {
            arcania.ConditionalExpression need = modelData.ConfigTask?.Need;
            if (need != null)
            {
                var needLay = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("quantity_task_text"), runtime);
                needLay.SetTextRaw(0, "Needs: " + need.humanExpression);
                AddToExpand(layoutRU, needLay, jCU);
            }
        }
        #endregion

        #region require of target (for hints and similar stuff)
        if (showRequireOfTarget)
        {
            arcania.ConditionalExpression require = modelData.ConfigHintData.hintTargetPointer.RuntimeUnit.ConfigBasic.Require;
            if (require != null)
            {
                var needLay = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("quantity_task_text"), runtime);
                needLay.SetTextRaw(0, Local.GetText("Requires", "used to show the requirement condition that needs to be met to do something. Usually used with a :, as in requires: blabla") + ": " + require.humanExpression);
                AddToExpand(layoutRU, needLay, jCU);
            }
        }
        #endregion
        #region tag text
        var hasNamedTag = false;
        int lastTagWithName = -1;
        for (int i = 0; i < modelData.ConfigBasic.Tags.Count; i++)
        {
            IDPointer tag = modelData.ConfigBasic.Tags[i];
            if (tag.Tag.RuntimeUnit == null) continue;
            hasNamedTag = true;
            lastTagWithName = i;
        }
        if (hasNamedTag)
        {
            var needLay = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("quantity_task_text"), runtime);
            var text = "";
            for (int i = 0; i < modelData.ConfigBasic.Tags.Count; i++)
            {
                IDPointer tag = modelData.ConfigBasic.Tags[i];
                if (tag.Tag.RuntimeUnit == null) continue;
                text += tag.Tag.tagName;
                if (i != lastTagWithName)
                {
                    text += ", ";
                }
            }
            needLay.SetTextRaw(0, text);
            AddToExpand(layoutRU, needLay, jCU);
        }
        #endregion
    }

    public static void SetupGameCanvasTabMenuInstantiation(MainGameControl mgc, bool isArchive)
    {
        JGameControlDataHolder jControlDataHolder = mgc.JControlData;
        var runtime = jControlDataHolder.LayoutRuntime;
        var arcaniaModel = mgc.arcaniaModel;
        var arcaniaDatas = arcaniaModel.arcaniaUnits;
        var jCanvas = runtime.jLayCanvas;
        var layoutMaster = runtime.LayoutMaster;
        #region tab menu layouts
        jCanvas.FixedMenus[Direction.WEST] = new();
        jCanvas.FixedMenus[Direction.SOUTH] = new();
        for (int i = 0; i < 2; i++)
        {
            var layoutD = layoutMaster.LayoutDatas.GetData(i == 0 ? "lower_tab_menu_mobile" : "left_tab_menu_desktop");
            JLayoutRuntimeUnit layoutRU = JCanvasMaker.CreateLayout(layoutD, runtime);
            Direction dir = i == 0 ? Direction.SOUTH : Direction.WEST;
            JCanvasMaker.AddFixedMenu(jCanvas, dir, layoutRU);
            // jCanvas.FixedMenus[dir].Add(layoutRU);
            jControlDataHolder.tabMenu[dir] = layoutRU;
            layoutRU.SetVisibleSelf(false);
        }
        #endregion
        // -------------------------------------------------
        // TAB BUTTON INSTANTIATING (not yet implemented button instantiation) AND OTHER SMALL SETUP
        // -------------------------------------------------
        #region tab button and other things
        #region overlay tab menu layout setup
        var overlay = jCanvas.Overlays[0];
        {
            var dialogLay = overlay.LayoutRuntimeUnit.AddLayoutAsChild(JCanvasMaker.CreateLayout("overlay_tab_button", runtime));
            dialogLay.LayoutRU.SetTextRaw(0, "Tabs");
            dialogLay.LayoutRU.DefaultPositionModes = new PositionMode[] { PositionMode.LEFT_ZERO, PositionMode.SIBLING_DISTANCE };
            jControlDataHolder.OverlayTabMenuLayout = dialogLay;
            dialogLay.LayoutRU.SetVisibleSelf(false);
        }
        #endregion
        for (int tabIndex = 0; tabIndex < arcaniaDatas.datas[UnitType.TAB].Count; tabIndex++)
        {
            RuntimeUnit item = arcaniaDatas.datas[UnitType.TAB][tabIndex];
            if (item.Tab.ArchiveOnly && !isArchive) continue;
            if (item.Tab.DisableOnArchive && isArchive) continue;
            var tcu = new JTabControlUnit();
            for (int buttonTypeIndex = 0; buttonTypeIndex < 3; buttonTypeIndex++)
            {
                var buttonLD = layoutMaster.LayoutDatas.GetData("tab_button_desktop_as_layout");
                var buttonLayRU = JCanvasMaker.CreateLayout(buttonLD, runtime);
                var parentLayout = jControlDataHolder.OverlayTabMenuLayout.LayoutRU;
                if (buttonTypeIndex <= 1)
                {
                    var d = buttonTypeIndex == 0 ? Direction.WEST : Direction.SOUTH;
                    parentLayout = jControlDataHolder.tabMenu[d];
                }
                var child = parentLayout.AddLayoutAsChild(buttonLayRU);
                if (buttonTypeIndex == 1)
                    child.PositionModeOverride = new PositionMode[] { PositionMode.SIBLING_DISTANCE, PositionMode.CENTER };

                var sprite = item.ConfigBasic.SpriteKey == null ? null : mgc.ResourceJson.spritesForLayout[item.ConfigBasic.SpriteKey];
                if (sprite == null)
                {
                    Debug.Log("Sprite key not found " + item.ConfigBasic.SpriteKey);
                }
                else
                {
                    buttonLayRU.ImageChildren[1].UiUnit.ChangeSprite(sprite);
                }
                switch (buttonTypeIndex)
                {
                    case 0:
                        tcu.DesktopButton = buttonLayRU;
                        break;
                    case 1:
                        tcu.MobileButton = buttonLayRU;
                        break;
                    case 2:
                        tcu.OverlayButton = buttonLayRU;
                        break;
                    default:
                        break;
                }
                tcu.TabToggleButtons.Add(buttonLayRU);
            }


            tcu.TabData = item;
            foreach (var sepD in item.Tab.Separators)
            {
                tcu.SeparatorControls.Add(new JTabControlUnit.JSeparatorControl(sepD));
            }
            jCanvas.children[tabIndex].LayoutRuntimeUnit.RectTransform.gameObject.name = $"tab_{item.Name}";
            jControlDataHolder.TabControlUnits.Add(tcu);
            foreach (var t in item.Tab.AcceptedUnitTypes)
            {
                switch (t)
                {
                    case UnitType.RESOURCE:
                    case UnitType.TASK:
                    case UnitType.CLASS:
                    case UnitType.SKILL:
                    case UnitType.HOUSE:
                    case UnitType.FURNITURE:
                    case UnitType.LOCATION:
                    case UnitType.HINT:
                        foreach (var separator in tcu.SeparatorControls)
                        {
                            separator.UnitGroupControls[t] = new();
                        }
                        //tcu.UnitGroupControls[t] = new();
                        break;
                    case UnitType.TAB:
                    // might have to do something with this if you ever create a "bestiary"
                    case UnitType.ENCOUNTER:
                    default:
                        break;
                }
            }
        }
        #region overlay tab menu close button
        {
            var closeButtonOverlayTab = JCanvasMaker.CreateLayout("centered_simple_button", runtime);
            jControlDataHolder.OverlayTabMenuLayout.LayoutRU.AddLayoutAsChild(closeButtonOverlayTab);
            closeButtonOverlayTab.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("Close"));
            JRTControlUnit jCU = new();
            jCU.MainLayout = closeButtonOverlayTab;
            closeButtonOverlayTab.ButtonChildren[0].Item1.ImageChildren[1].UiUnit.ActiveSelf = false;
            jCU.MainExecuteButton = new JButtonAccessor(closeButtonOverlayTab, 0);
            jControlDataHolder.TabOverlayCloseButtonJCU = jCU;
        }
        #endregion
        #endregion


    }

    private static void CreateModViews(LayoutDataMaster layoutMaster, JLayoutRuntimeData runtime, JRTControlUnit jCU, JLayoutRuntimeUnit layoutRU, List<ModRuntime> modList, string header, JRTControlUnitMods modControl, Dictionary<ModType, (string key, string value, string valueWithColor, ColorData colorD)> modReplacements, int mode)
    {
        if (modList.Count > 0)
        {
            modControl.Header = CreateMiniHeader(runtime, jCU, layoutRU, header);
            foreach (var mod in modList)
            {
                if (mod.ModType == ModType.Lock) continue;
                var mainText = mode == 0 ? mod.HumanText : (mode == 1 ? mod.HumanTextIntermediary : mod.HumanTextTarget);
                if (mainText == null) continue;



                var triple = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("in_header_triple_statistic"), runtime);
                if (modReplacements.TryGetValue(mod.ModType, out var values))
                {
                    mainText = mainText.Replace(values.key, values.valueWithColor);
                    if (values.colorD != null)
                    {
                        triple.TextChildren[1].OverwriteSingleColor(ColorSetType.NORMAL, values.colorD);
                    }
                }
                AddToExpand(layoutRU, triple, jCU);
                modControl.tripleTextViews.Add(triple);
                modControl.Mods.Add(mod);
                var value = mod.Value;

                triple.SetTextRaw(0, mainText);
                // activate mod has no number
                if (mod.ModType == ModType.Activate)
                {
                    triple.SetTextRaw(1, "");
                    continue;
                }
                string secondaryText;
                if (value > 0 && mod.ModType != ModType.SpaceConsumption)
                    secondaryText = $"+{value}";
                else
                    secondaryText = $"{value}";
                triple.SetTextRaw(1, secondaryText);

            }
        }
    }

    // this method has two uses, more or less
    // 1) create change list views for the first time, for static elements
    // 2) when model data changes (runtime unit change) for the same JCU, assure there are enough change lists in that JCU
    // Case 2 is mainly for exploration elements, currently
    public static void EnsureChangeListViewsAreCreated(JLayoutRuntimeData runtime, RuntimeUnit modelData, JRTControlUnit jCU, JLayoutRuntimeUnit layoutRU, JGameControlDataHolder controlData)
    {
        LayoutDataMaster layoutMaster = runtime.LayoutMaster;
        if (modelData.ConfigTask != null)
        {
            for (int rcgIndex = 0; rcgIndex < modelData.ConfigTask.ResourceChangeLists.Count; rcgIndex++)
            {
                List<ResourceChange> rcl = modelData.ConfigTask.ResourceChangeLists[rcgIndex];
                if (rcl == null) continue;
                if (rcl.Count == 0) continue;

                // it might be already instantiated
                jCU.ChangeGroups[rcgIndex] ??= new();
                var changeType = (ResourceChangeType)rcgIndex;

                ColorData changeTypeOverride = null;
                if (controlData.ColorForResourceChangeType.TryGetValue(changeType, out var v))
                {
                    changeTypeOverride = v;
                }

                // create mini header if necessary
                if (jCU.ChangeGroups[rcgIndex].Header == null)
                {

                    string textKey = changeType switch
                    {
                        ResourceChangeType.COST => controlData.LabelCost,
                        ResourceChangeType.RESULT => controlData.LabelResult,
                        ResourceChangeType.RUN => controlData.LabelRun,
                        ResourceChangeType.EFFECT => controlData.LabelEffect,
                        ResourceChangeType.RESULT_ONCE => controlData.LabelResultOnce,
                        ResourceChangeType.RESULT_FAIL => controlData.LabelResultFail,
                        ResourceChangeType.BUY => controlData.LabelBuy,
                        _ => null,
                    };

                    JLayoutRuntimeUnit miniHeader = CreateMiniHeader(runtime, jCU, layoutRU, textKey);
                    jCU.ChangeGroups[rcgIndex].Header = miniHeader;
                }
                AutoList<JLayoutRuntimeUnit> tripleTextViews = jCU.ChangeGroups[rcgIndex].tripleTextViews;
                for (int i = 0; i < rcl.Count; i++)
                {
                    if (i >= tripleTextViews.Count)
                    {
                        ResourceChange rcu = rcl[i];
                        var triple = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("in_header_triple_statistic"), runtime);
                        triple.SetTextRaw(0, rcu.IdPointer.RuntimeUnit?.Name);
                        triple.SetTextRaw(1, "" + rcu.valueChange.min);
                        triple.SetTextRaw(2, "0");
                        if (changeTypeOverride != null)
                        {
                            triple.TextChildren[0].OverwriteSingleColor(ColorSetType.NORMAL, changeTypeOverride);
                            triple.TextChildren[1].OverwriteSingleColor(ColorSetType.NORMAL, changeTypeOverride);
                        }
                        tripleTextViews.Add(triple);
                        AddToExpand(layoutRU, triple, jCU);
                    }
                    else
                    {
                        tripleTextViews[i].SetVisibleSelf(true);
                    }
                }
                // if there are more than enough triple text views, hide the excessive ones
                for (int i = rcl.Count; i < tripleTextViews.Count; i++)
                {
                    tripleTextViews[i].SetVisibleSelf(false);
                }
            }
        }
    }

    public static void AddToExpand(JLayoutRuntimeUnit layoutRU, JLayoutRuntimeUnit unit, JRTControlUnit jCU)
    {
        layoutRU.AddLayoutAsChild(unit);
        jCU.InsideExpandable.Add(unit);
        unit.SetParentShowing(false);
    }

    public static JLayoutRuntimeUnit CreateMiniHeader(JLayoutRuntimeData runtime, JRTControlUnit jCU, JLayoutRuntimeUnit layoutRU, string textKey)
    {
        var layoutMaster = runtime.LayoutMaster;
        var miniHeader = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("left_mini_header"), runtime);
        miniHeader.SetTextRaw(0, textKey);
        AddToExpand(layoutRU, miniHeader, jCU);
        return miniHeader;
    }

    internal static JLayoutRuntimeUnit CreateLogLayout(MainGameControl mgc, LogUnit logUnit)
    {
        var lay = mgc.JLayoutRuntime.LayoutMaster.LayoutDatas.GetData("log_lay");
        var cd = mgc.JLayoutRuntime.LayoutMaster.ColorDatas.GetData("resource_highlight_text");
        var layout = JCanvasMaker.CreateLayout(lay, mgc.JLayoutRuntime);
        var text = string.Empty;
        if (logUnit.logType == LogUnit.LogType.UNIT_UNLOCKED)
        {
            var ut = logUnit.Unit.ConfigBasic.UnitType;
            if (mgc.JControlData.LabelUnitTypeDescription.TryGetValue(ut, out var label))
            {
                text = $"{mgc.JControlData.LabelUnlocked}: <color={cd.CodeCache[mgc.JLayoutRuntime.CurrentColorSchemeId]}>{logUnit.Unit.ConfigBasic.name}</color>\n({label})";
            }
            else
            {
                text = $"{mgc.JControlData.LabelUnlocked}: {logUnit.Unit.ConfigBasic.name}";
            }

        }
        layout.SetTextRaw(0, text);
        return layout;
    }
}
