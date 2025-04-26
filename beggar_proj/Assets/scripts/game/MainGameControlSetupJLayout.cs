using HeartUnity;
using HeartUnity.View;
using JLayout;
using System.Collections.Generic;
using UnityEngine;

public class MainGameControlSetupJLayout
{
    internal static void SetupCanvas(MainGameControl mgc)
    {
        LayoutDataMaster layoutMaster = new LayoutDataMaster();
        JsonInterpreter.ReadJson(mgc.ResourceJson.layoutJson.text, layoutMaster);
        var arcaniaModel = mgc.arcaniaModel;
        var arcaniaDatas = arcaniaModel.arcaniaUnits;
        var config = HeartGame.GetConfig();
        JLayoutRuntimeData runtime = new();
        runtime.DefaultFont = mgc.Font;
        runtime.ImageSprites = mgc.ResourceJson.spritesForLayout;
        var jCanvas = JCanvasMaker.CreateCanvas(Mathf.Max(arcaniaDatas.datas[UnitType.TAB].Count, 1), mgc.CanvasRequest, config.reusableCanvas);
        runtime.jLayCanvas = jCanvas;
        mgc.JLayoutRuntime = runtime;
        runtime.LayoutMaster = layoutMaster;
        JGameControlDataHolder jControlDataHolder = new();
        jControlDataHolder.LayoutRuntime = runtime;
        // var dynamicCanvas = CanvasMaker.CreateCanvas(Mathf.Max(arcaniaDatas.datas[UnitType.TAB].Count, 1), mgc.CanvasRequest, config.reusableCanvas);
        //mgc.dynamicCanvas = dynamicCanvas;

        mgc.EngineView = mgc.HeartGame.CreateEngineView(new EngineView.EngineViewInitializationParameter()
        {
            canvas = jCanvas.Canvas,
            DisableAutoScaling = true

        }, 2);
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
        }
        #endregion
        // -------------------------------------------------
        // TAB BUTTON INSTANTIATING (not yet implemented button instantiation) AND OTHER SMALL SETUP
        // -------------------------------------------------
        #region tab button and other things
        for (int tabIndex = 0; tabIndex < arcaniaDatas.datas[UnitType.TAB].Count; tabIndex++)
        {
            RuntimeUnit item = arcaniaDatas.datas[UnitType.TAB][tabIndex];
            var tcu = new JTabControlUnit();
            for (int buttonTypeIndex = 0; buttonTypeIndex < 2; buttonTypeIndex++)
            {
                var buttonLD = layoutMaster.LayoutDatas.GetData("tab_button_desktop_as_layout");
                var d = buttonTypeIndex == 0 ? Direction.WEST : Direction.SOUTH;
                var buttonLayRU = JCanvasMaker.CreateLayout(buttonLD, runtime);
                var child = jControlDataHolder.tabMenu[d].AddLayoutAsChild(buttonLayRU);
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
                if (buttonTypeIndex == 0)
                    tcu.DesktopButton = buttonLayRU;
                else
                    tcu.MobileButton = buttonLayRU;
                tcu.TabToggleButtons.Add(buttonLayRU);
            }


            var taskParent = jCanvas.children[tabIndex];

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
        #endregion

        mgc.JControlData = jControlDataHolder;

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
                    var jCU = new JRTControlUnit();
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

                    var hasTaskButton = unitType == UnitType.TASK || unitType == UnitType.CLASS || unitType == UnitType.SKILL || unitType == UnitType.HOUSE || unitType == UnitType.LOCATION;
                    var hasTitleWithValue = unitType == UnitType.SKILL || unitType == UnitType.FURNITURE;
                    var hasXPBar = unitType == UnitType.SKILL;
                    var hasResourceExpander = !hasTaskButton && (unitType == UnitType.RESOURCE || unitType == UnitType.FURNITURE);
                    var hasPlusMinusButton = unitType == UnitType.FURNITURE;

                    if (hasTitleWithValue)
                    {
                        var titleRU = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("above_button_title_with_value"), runtime);
                        var child = layoutRU.AddLayoutAsChild(titleRU);
                        titleRU.SetTextRaw(0, modelData.ConfigBasic.name);
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
                        jCU.MainExecuteButton = new JButtonAccessor(buttonLayoutRU, 0);
                        jCU.ExpandButton = new JButtonAccessor(buttonLayoutRU, 1);
                        jCU.ExpandButtonImage = new JImageAccessor(buttonLayoutRU.ButtonChildren[1].Item1, 0);
                        if (!modelData.ConfigTask.Duration.HasValue || modelData.ConfigTask.Duration <= 0)
                        {
                            // buttonLayoutRU.ButtonChildren[0].Item1.ImageChildren[1].UiUnit.ActiveSelf = modelData.ConfigBasic.UnitType == UnitType.HOUSE;
                            buttonLayoutRU.ButtonChildren[0].Item1.ImageChildren[1].UiUnit.ActiveSelf = false;
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
                    }

                    if (hasPlusMinusButton)
                    {
                        var child = layoutRU.AddLayoutAsChild(JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("furniture_buttons"), runtime));
                        jCU.PlusMinusLayout = child.LayoutRU;
                        child.LayoutRU.ButtonChildren[0].Item1.SetTextRaw(0, "+");
                        child.LayoutRU.ButtonChildren[1].Item1.SetTextRaw(0, "-");
                    }

                    if (!string.IsNullOrWhiteSpace(modelData.ConfigBasic.Desc))
                    {
                        var descLayout = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("lore_text"), runtime);

                        descLayout.SetTextRaw(0, modelData.ConfigBasic.Desc);
                        jCU.Description = new JLayTextAccessor(descLayout, 0);
                        AddToExpand(layoutRU, descLayout, jCU);
                    }
                    #region change list instantiation
                    EnsureChangeListViewsAreCreated(runtime, modelData, jCU, layoutRU);
                    #endregion

                    #region Mods
                    var modList = modelData.ModsOwned;
                    var header = "modifications";
                    var modControl = jCU.OwnedMods;
                    if (modList.Count > 0)
                    {
                        modControl.Header = CreateMiniHeader(runtime, jCU, layoutRU, header);
                        foreach (var mod in modList)
                        {
                            if (mod.ModType == ModType.Lock) continue;
                            var triple = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("in_header_triple_statistic"), runtime);
                            AddToExpand(layoutRU, triple, jCU);
                            modControl.tripleTextViews.Add(triple);
                            modControl.Mods.Add(mod);
                            var value = mod.Value;
                            triple.SetTextRaw(0, mod.HumanText);
                            string secondaryText = null;
                            if (value > 0 && mod.ModType != ModType.SpaceConsumption)
                                secondaryText = $"+{value}";
                            else
                                secondaryText = $"{value}";
                            triple.SetTextRaw(1, secondaryText);
                        }
                    }
                    #endregion



                }
                #endregion

            }
        }
        #endregion


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
                jCU.GaugeProgressImage = new JImageAccessor(expandableTextWithBar.LayoutRU.Children[0].LayoutRU.Children[1].LayoutRU, 1);
                jCU.Name = new JLayTextAccessor(layoutThatHasName, 0);
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
                fleeButtonLayout.ButtonChildren[0].Item1.SetTextRaw(0, "Flee");
                JRTControlUnit jCU = new();
                jCU.MainLayout = fleeButtonLayout;
                jCU.MainExecuteButton = new JButtonAccessor(fleeButtonLayout, 0);
                jControlDataHolder.Exploration.FleeButtonJCU = jCU;
            }
        }

        #endregion


    }

    // this method has two uses, more or less
    // 1) create change list views for the first time, for static elements
    // 2) when model data changes for the same JCU, assure there are enough change lists in that JCU
    // Case 2 is mainly for exploration elements, currently
    public static void EnsureChangeListViewsAreCreated(JLayoutRuntimeData runtime, RuntimeUnit modelData, JRTControlUnit jCU, JLayoutRuntimeUnit layoutRU)
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
                // create mini header if necessary
                if (jCU.ChangeGroups[rcgIndex].Header == null)
                {
                    var changeType = (ResourceChangeType)rcgIndex;
                    string textKey = changeType switch
                    {
                        ResourceChangeType.COST => "cost",
                        ResourceChangeType.RESULT => "result",
                        ResourceChangeType.RUN => "run",
                        ResourceChangeType.EFFECT => "effect",
                        ResourceChangeType.RESULT_ONCE => "first time",
                        ResourceChangeType.RESULT_FAIL => "result failure",
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
                        tripleTextViews.Add(triple);
                        AddToExpand(layoutRU, triple, jCU);
                    } else {
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
        var layout = JCanvasMaker.CreateLayout(lay, mgc.JLayoutRuntime);
        var text = string.Empty;
        if (logUnit.logType == LogUnit.LogType.UNIT_UNLOCKED)
        {
            text = $"Unlocked {logUnit.Unit.ConfigBasic.name}";
        }
        layout.SetTextRaw(0, text);
        return layout;
    }
}
