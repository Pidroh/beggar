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
        mgc.LayoutRuntime = runtime;
        JGameControlDataHolder jControlDataHolder = new();
        // var dynamicCanvas = CanvasMaker.CreateCanvas(Mathf.Max(arcaniaDatas.datas[UnitType.TAB].Count, 1), mgc.CanvasRequest, config.reusableCanvas);
        //mgc.dynamicCanvas = dynamicCanvas;

        mgc.EngineView = mgc.HeartGame.CreateEngineView(new EngineView.EngineViewInitializationParameter()
        {
            canvas = jCanvas.Canvas,
            DisableAutoScaling = true

        }, 2);

        // -------------------------------------------------
        // TAB BUTTON INSTANTIATING AND OTHER SMALL SETUP
        // -------------------------------------------------
        for (int tabIndex = 0; tabIndex < arcaniaDatas.datas[UnitType.TAB].Count; tabIndex++)
        {
            RuntimeUnit item = arcaniaDatas.datas[UnitType.TAB][tabIndex];
            var tcu = new JTabControlUnit();
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
                        tcu.UnitGroupControls[t] = new();
                        break;
                    case UnitType.TAB:
                    case UnitType.ENCOUNTER:
                    default:
                        break;
                }
            }
        }
        mgc.JControlData = jControlDataHolder;
        for (int tabIndex = 0; tabIndex < jControlDataHolder.TabControlUnits.Count; tabIndex++)
        {
            var taskParent = jCanvas.children[tabIndex];
            var tabControl = jControlDataHolder.TabControlUnits[tabIndex];
            JTabControlUnit jTabControl = tabControl;
            var unitGroupControls = tabControl.UnitGroupControls;
            foreach (var pair in unitGroupControls)
            {
                foreach (var item in arcaniaDatas.datas[pair.Key])
                {
                    var modelData = item;
                    var jCU = new JRTControlUnit();
                    pair.Value.Add(jCU);
                    jCU.Data = modelData;
                    var id = modelData.ConfigBasic.Id;
                    var layoutD = layoutMaster.LayoutDatas.GetData("content_holder_expandable");
                    JLayoutRuntimeUnit layoutRU = JCanvasMaker.CreateLayout(layoutD, runtime);
                    jCU.MainLayout = layoutRU;
                    layoutRU.RectTransform.gameObject.name += " " + id;
                    layoutRU.DefaultPositionModes = new PositionMode[] { PositionMode.LEFT_ZERO, PositionMode.SIBLING_DISTANCE };
                    
                    var childOfParent = taskParent.AddLayoutAsChild(layoutRU);

                    var hasTaskButton = modelData.ConfigBasic.UnitType == UnitType.TASK;
                    var hasResourceExpander = !hasTaskButton && modelData.ConfigBasic.UnitType == UnitType.RESOURCE;

                    if (hasTaskButton)
                    {
                        var buttonLayoutRU = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("expandable_task_main_buttons"), runtime);
                        layoutRU.AddLayoutAsChild(buttonLayoutRU);
                        buttonLayoutRU.ButtonChildren[0].Item1.SetText(0, modelData.Name);
                        jCU.MainExecuteButton = new JButtonAccessor(buttonLayoutRU, 0);
                        jCU.ExpandButton = new JButtonAccessor(buttonLayoutRU, 1);
                        jCU.ExpandButtonImage = new JImageAccessor(buttonLayoutRU.ButtonChildren[1].Item1, 0);
                    }
                    else if (hasResourceExpander)
                    {
                        var resourceLayoutRU = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("expandable_resource_text"), runtime);
                        layoutRU.AddLayoutAsChild(resourceLayoutRU);
                        resourceLayoutRU.SetText(0, modelData.Name);
                        jCU.ExpandButton = new JButtonAccessor(resourceLayoutRU, 0);
                        jCU.ExpandButtonImage = new JImageAccessor(resourceLayoutRU.ButtonChildren[0].Item1, 0);
                        jCU.ExpandWhenClickingLayout = resourceLayoutRU;
                    }

                    if (!string.IsNullOrWhiteSpace(modelData.ConfigBasic.Desc))
                    {
                        var descLayout = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("lore_text"), runtime);

                        descLayout.SetText(0, modelData.ConfigBasic.Desc);
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
                            miniHeader.SetText(0, textKey);
                            AddToExpand(miniHeader);
                            foreach (var rcu in rcl)
                            {
                                var triple = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("in_header_triple_statistic"), runtime);
                                triple.SetText(0, rcu.IdPointer.RuntimeUnit?.Name);
                                triple.SetText(1, "" + rcu.valueChange.min);
                                triple.SetText(2, "0");
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
}
