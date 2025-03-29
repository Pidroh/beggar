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
        for (int tabIndex = 0; tabIndex < arcaniaDatas.datas[UnitType.TAB].Count; tabIndex++)
        {
            RuntimeUnit item = arcaniaDatas.datas[UnitType.TAB][tabIndex];
            var tcu = new JTabControlUnit();
            {
                var buttonLD = layoutMaster.LayoutDatas.GetData("tab_button_desktop_as_layout");
                var d = Direction.WEST;
                var buttonLayRU = JCanvasMaker.CreateLayout(buttonLD, runtime);
                jControlDataHolder.tabMenu[d].AddLayoutAsChild(buttonLayRU);

                var sprite = item.ConfigBasic.SpriteKey == null ? null : mgc.ResourceJson.spritesForLayout[item.ConfigBasic.SpriteKey];
                if (sprite == null)
                {
                    Debug.Log("Sprite key not found "+ item.ConfigBasic.SpriteKey);
                }
                else 
                {
                    buttonLayRU.ImageChildren[1].UiUnit.ChangeSprite(sprite);
                }
                tcu.DesktopButton = buttonLayRU;
            }
            

            var taskParent = jCanvas.children[tabIndex];

            tcu.TabData = item;
            foreach (var sepD in item.Tab.Separators)
            {
                tcu.SeparatorControls.Add(new JTabControlUnit.JSeparatorControl(sepD));
            }
            jCanvas.children[tabIndex].RectTransform.gameObject.name = $"tab_{item.Name}";
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
        mgc.JControlData = jControlDataHolder;
        for (int tabIndex = 0; tabIndex < jControlDataHolder.TabControlUnits.Count; tabIndex++)
        {
            var parentOfTabContent = jCanvas.children[tabIndex];
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


                foreach (var modelData in separatorControl.SepD.BoundRuntimeUnits)
                {
                    var jCU = new JRTControlUnit();
                    // special types that don't have unit group controls are handled in a special way
                    if (!separatorControl.UnitGroupControls.TryGetValue(modelData.ConfigBasic.UnitType, out var list)) continue;
                    list.Add(jCU);
                    jCU.Data = modelData;
                    var id = modelData.ConfigBasic.Id;
                    var layoutD = layoutMaster.LayoutDatas.GetData("content_holder_expandable");
                    JLayoutRuntimeUnit layoutRU = JCanvasMaker.CreateLayout(layoutD, runtime);
                    jCU.MainLayout = layoutRU;
                    layoutRU.RectTransform.gameObject.name += " " + id;
                    layoutRU.DefaultPositionModes = new PositionMode[] { PositionMode.LEFT_ZERO, PositionMode.SIBLING_DISTANCE };

                    var childOfParent = parentOfTabContent.AddLayoutAsChild(layoutRU);

                    var hasTaskButton = modelData.ConfigBasic.UnitType == UnitType.TASK;
                    var hasResourceExpander = !hasTaskButton && modelData.ConfigBasic.UnitType == UnitType.RESOURCE;

                    if (hasTaskButton)
                    {
                        var buttonLayoutRU = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("expandable_task_main_buttons"), runtime);
                        layoutRU.AddLayoutAsChild(buttonLayoutRU);
                        buttonLayoutRU.ButtonChildren[0].Item1.SetTextRaw(0, modelData.Name);
                        jCU.MainExecuteButton = new JButtonAccessor(buttonLayoutRU, 0);
                        jCU.ExpandButton = new JButtonAccessor(buttonLayoutRU, 1);
                        jCU.ExpandButtonImage = new JImageAccessor(buttonLayoutRU.ButtonChildren[1].Item1, 0);
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

                    if (!string.IsNullOrWhiteSpace(modelData.ConfigBasic.Desc))
                    {
                        var descLayout = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("lore_text"), runtime);

                        descLayout.SetTextRaw(0, modelData.ConfigBasic.Desc);
                        jCU.Description = new JLayTextAccessor(descLayout, 0);
                        AddToExpand(descLayout);
                    }
                    if (modelData.ConfigTask != null)
                    {
                        for (int rcgIndex = 0; rcgIndex < modelData.ConfigTask.ResourceChangeLists.Count; rcgIndex++)
                        {
                            List<ResourceChange> rcl = modelData.ConfigTask.ResourceChangeLists[rcgIndex];
                            if (rcl == null) continue;
                            if (rcl.Count == 0) continue;
                            jCU.ChangeGroups[rcgIndex] = new();
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
                            var miniHeader = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("left_mini_header"), runtime);
                            miniHeader.SetTextRaw(0, textKey);
                            AddToExpand(miniHeader);
                            foreach (var rcu in rcl)
                            {
                                var triple = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("in_header_triple_statistic"), runtime);
                                triple.SetTextRaw(0, rcu.IdPointer.RuntimeUnit?.Name);
                                triple.SetTextRaw(1, "" + rcu.valueChange.min);
                                triple.SetTextRaw(2, "0");
                                jCU.ChangeGroups[rcgIndex].tripleTextViews.Add(triple);
                                AddToExpand(triple);
                            }

                        }
                    }

                    void AddToExpand(JLayoutRuntimeUnit unit)
                    {
                        layoutRU.AddLayoutAsChild(unit);
                        jCU.InsideExpandable.Add(unit);
                        unit.SetParentShowing(false);
                    }
                }
            }
        }
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
