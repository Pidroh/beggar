using HeartUnity.View;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainGameControlSetup
{
    public static void Setup(MainGameControl mgc) {
        var arcaniaModel = mgc.arcaniaModel;
        var arcaniaDatas = arcaniaModel.arcaniaUnits;
        JsonReader.ReadJson(mgc.ResourceJson.text, arcaniaDatas);
        var dynamicCanvas = CanvasMaker.CreateCanvas(Mathf.Max(arcaniaDatas.datas[UnitType.TAB].Count, 1), mgc.CanvasRequest);
        mgc.dynamicCanvas = dynamicCanvas;
        var lowerMenuLayout = dynamicCanvas.CreateLowerMenuLayout(60).SetStretchWidth(true).SetLayoutType(LayoutParent.LayoutType.HORIZONTAL);
        mgc.TabButtonLayout = lowerMenuLayout;

        mgc.EngineView = EngineView.CreateEngineViewThroughCode(new EngineView.EngineViewInitializationParameter() {
            canvas = dynamicCanvas.Canvas
        });
        mgc.EngineView.Init(2);

        // -------------------------------------------------
        // TAB BUTTON INSTANTIATING AND OTHER SMALL SETUP
        // -------------------------------------------------
        foreach (var item in arcaniaDatas.datas[UnitType.TAB])
        {
            
            var button = CanvasMaker.CreateButton(item.Tab.RuntimeUnit.ConfigBasic.Id, mgc.ButtonObjectRequest, mgc.ButtonRequest);

            var lc = new LayoutChild()
            {
                RectTransform = button.Button.RectTransform
            };
            lowerMenuLayout.AddLayoutChildAndParentIt(lc);
            var tcu = new TabControlUnit()
            {
                SelectionButtonLayoutChild = lc,
                SelectionButton = button,
                TabData = item
            };
            mgc.TabControlUnits.Add(tcu);
            foreach (var sepD in item.Tab.Separators)
            {
                tcu.Separators.Add(new TabControlUnit.SeparatorInTab(sepD));
            }
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
                        tcu.UnitGroupControls[t] = new();
                        break;
                    case UnitType.TAB:
                        break;
                    default:
                        break;
                }
            }
        }

        // -------------------------------------------------
        // MAIN GRAPHIC ELEMENT INSTANTIATING
        // -------------------------------------------------
        for (int tabIndex = 0; tabIndex < mgc.TabControlUnits.Count; tabIndex++)
        {
            TabControlUnit tabControl = mgc.TabControlUnits[tabIndex];
            // Tab separator instancing
            foreach (var sep in tabControl.Separators)
            {
                var UnitGroupControls = tabControl.UnitGroupControls;

                // -------------------------------------------------
                // SEPARATOR GRAPHIC INSTANCING
                // ALSO INSTANTIATES SPACE AMOUNT LABEL
                // -------------------------------------------------
                {
                    var image = CanvasMaker.CreateSimpleImage(new Color(0.2f, 0.2f, 0.2f));
                    var text = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.Font, 12);
                    text.SetParent(image.gameObject.transform);
                    text.RectTransform.FillParent();
                    text.RectTransform.SetOffsetMinByIndex(0, 0);
                    text.rawText = sep.Data.Name;
                    LayoutChild layoutChild = new LayoutChild()
                    {
                        RectTransform = image.RectTransform
                    };
                    dynamicCanvas.children[tabIndex].AddLayoutChildAndParentIt(layoutChild);
                    dynamicCanvas.children[tabIndex].InvertChildrenPositionIndex = tabControl.TabData.Tab.ContainsLogs;
                    sep.SeparatorLC = layoutChild;

                    if (!sep.Data.ShowSpace) goto END_OF_SEPARATOR_INSTANCE;
                    var spaceT = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.Font, 18);
                    dynamicCanvas.children[tabIndex].AddLayoutChildAndParentIt(spaceT);
                    sep.SpaceAmountText = spaceT;
                }
                // -------------------------------------------------
                END_OF_SEPARATOR_INSTANCE:
                foreach (var pair in UnitGroupControls)
                {
                    foreach (var item in arcaniaDatas.datas[pair.Key])
                    {
                        // -----------------------------------------------------------
                        // IDENTIFYING SEPARATOR FOR UNIT
                        // -----------------------------------------------------------
                        TabControlUnit.SeparatorInTab unitSeparator = null;
                        foreach (var candidate in tabControl.Separators)
                        {
                            if (candidate.Data.AcceptedUnitTypes.Count > 0 && !candidate.Data.AcceptedUnitTypes.Contains(item.ConfigBasic.UnitType)) continue;
                            if (candidate.Data.RequireMax && !item.HasMax) continue;
                            unitSeparator = candidate;
                            // if not default, just use it like that
                            if (!unitSeparator.Data.Default) break;
                        }
                        // -----------------------------------------------------------
                        // handling inappropriate separator
                        if (unitSeparator == null && tabControl.Separators.Count > 0) continue;
                        if (unitSeparator != sep) continue;
                        // -----------------------------------------------------------

                        var layout = CanvasMaker.CreateLayout().SetFitHeight(true);
                        var hasBWE = pair.Key != UnitType.RESOURCE && pair.Key != UnitType.FURNITURE;
                        bool taskWithMax = pair.Key == UnitType.TASK && item.HasMax;
                        var hasValueText = !hasBWE || taskWithMax;
                        var valueTextIsOnLWE = !hasBWE;

                        var tcu = new RTControlUnit();
                        tcu.ParentTabSeparator = unitSeparator;

                        if (hasBWE)
                        {
                            var button = CanvasMaker.CreateButton(item.ConfigBasic.name, mgc.ButtonObjectRequest, mgc.ButtonRequest);
                            var iconButton = CanvasMaker.CreateButtonWithIcon(mgc.ExpanderSprite);
                            var bwe = new ButtonWithExpandable(button, iconButton);
                            tcu.bwe = bwe;
                        }

                        if (!hasBWE)
                        {
                            var titleText = CanvasMaker.CreateTextUnit(mgc.ButtonObjectRequest.SecondaryColor, mgc.ButtonObjectRequest.font, 16);
                            var iconButton = CanvasMaker.CreateButtonWithIcon(mgc.ExpanderSprite);
                            var lwe = new LabelWithExpandable(iconButton, titleText);

                            layout.AddLayoutChildAndParentIt(lwe.LayoutChild);
                            titleText.SetTextRaw(item.ConfigBasic.name);
                            tcu.lwe = lwe;
                        }


                        if (pair.Key == UnitType.FURNITURE)
                        {
                            var buttonAdd = CanvasMaker.CreateButton("+", mgc.ButtonObjectRequest, mgc.ButtonRequest);
                            var buttonRemove = CanvasMaker.CreateButton("-", mgc.ButtonObjectRequest, mgc.ButtonRequest);
                            var layoutAddRemove = CanvasMaker.CreateLayout();
                            layoutAddRemove.SetStretchHeight(true);
                            layoutAddRemove.SelfChild.RectTransform.SetHeightMilimeters(10);
                            layoutAddRemove.TypeLayout = LayoutParent.LayoutType.HORIZONTAL;
                            layoutAddRemove.AddLayoutChildAndParentIt(buttonAdd.Button);
                            layoutAddRemove.AddLayoutChildAndParentIt(buttonRemove.Button);
                            layout.AddLayoutAndParentIt(layoutAddRemove);
                            tcu.ButtonRemove = buttonRemove;
                            tcu.ButtonAdd = buttonAdd;
                        }
                        if (pair.Key == UnitType.SKILL)
                        {
                            {
                                var t = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.ButtonObjectRequest.font, 30);
                                t.text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                                // bwe.ExpandTargets
                                tcu.MainTitle = new SimpleChild<UIUnit>(t, t.RectTransform);
                                tcu.MainTitle.RectOffset = new RectOffset(20, 20, 10, 0);
                                layout.AddLayoutChildAndParentIt(tcu.MainTitle.LayoutChild);
                            }
                            {
                                var t = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.ButtonObjectRequest.font, 30);
                                t.text.horizontalAlignment = HorizontalAlignmentOptions.Right;
                                // bwe.ExpandTargets
                                tcu.SkillLevelText = t;
                                t.SetParent(tcu.MainTitle.ElementRectTransform);
                                t.RectTransform.FillParent();
                                t.RectTransform.SetOffsets(new RectOffset(20, 20, 10, 0));

                            }
                            tcu.XPGauge = new Gauge(mgc.SkillXPGaugeRequest);
                            layout.AddLayoutChildAndParentIt(tcu.XPGauge.layoutChild);
                        }
                        dynamicCanvas.children[tabIndex].AddLayoutAndParentIt(layout);

                        if (hasBWE)
                        {
                            layout.AddLayoutChildAndParentIt(tcu.bwe);
                            tcu.bwe.MainButton.SetTextRaw(item.ConfigBasic.name);
                        }

                        // value text instantiation
                        {
                            var t = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.ButtonObjectRequest.font, 16);
                            t.text.horizontalAlignment = HorizontalAlignmentOptions.Right;
                            // bwe.ExpandTargets
                            tcu.ValueText = t;
                            if (valueTextIsOnLWE)
                            {
                                t.SetParent(tcu.lwe.MainText.RectTransform);
                                t.RectTransform.FillParent();
                                t.RectTransform.SetOffsets(new RectOffset(20, 20, 10, 0));
                            }
                            else
                            {
                                var lc = LayoutChild.Create(t.transform);
                                AddToExpands(lc);
                                t.RectTransform.FillParent();
                                lc.RectTransform.SetHeight(20);
                            }

                        }

                        pair.Value.Add(tcu);
                        tcu.Data = item;
                        {
                            var t = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.ButtonObjectRequest.font, 16);
                            t.text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                            // bwe.ExpandTargets
                            tcu.Description = new SimpleChild<UIUnit>(t, t.RectTransform);
                            tcu.Description.RectOffset = new RectOffset(20, 20, 0, 0);
                            if (hasBWE)
                                AddToExpands(tcu.Description.LayoutChild);
                            else
                                layout.AddLayoutChildAndParentIt(tcu.Description.LayoutChild);

                        }

                        for (int i = 0; i < 4; i++)
                        {
                            if (item.ConfigTask == null) break;
                            var arrayOfChanges = item.ConfigTask.GetResourceChangeList(i);
                            var rcgIndex = i;

                            if (arrayOfChanges != null)
                            {
                                tcu.ChangeGroups[rcgIndex] = new ResourceChangeGroup();
                                string textKey = (ResourceChangeType)rcgIndex switch
                                {
                                    ResourceChangeType.COST => "cost",
                                    ResourceChangeType.RESULT => "result",
                                    ResourceChangeType.RUN => "run",
                                    ResourceChangeType.EFFECT => "effect",
                                    _ => null,
                                };
                                SeparatorWithLabel swl = CreateSeparator(layout, tcu.ExpandManager, textKey);

                                tcu.ChangeGroupSeparators[rcgIndex] = swl;
                            }
                            foreach (var changeU in arrayOfChanges)
                            {
                                TripleTextView ttv = CreateTripleTextView(layout, tcu.ExpandManager);
                                tcu.ChangeGroups[rcgIndex].tripleTextViews.Add(ttv);
                            }
                        }


                        List<SeparatorWithLabel> separators = tcu.Separators;
                        var ModUnit = tcu.ModsUnit;
                        ExpandableManager expandManager = tcu.ExpandManager;
                        CreateModViews(item, layout, separators, ModUnit, expandManager);

                        void AddToExpands(LayoutChild c)
                        {
                            tcu.ExpandManager.ExpandTargets.Add(c.GameObject);
                            layout.AddLayoutChildAndParentIt(c);
                        }
                    }
                }
            }

        }

        // end game message related
        {
            mgc.EndGameRuntimeUnit = arcaniaModel.FindRuntimeUnit(UnitType.TASK, "ponderexistence");
            var endMessage = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.Font, 18);
            endMessage.rawText = $"GAME CLEARED \nYou have become one with existence. \n At least until more content is added. \n\n Comment on the reddit post: \"I'm the beggar's journey\"";

            var lc = LayoutChild.Create(endMessage.transform);
            endMessage.RectTransform.FillParent();
            dynamicCanvas.OverlayMainLayout.AddLayoutChildAndParentIt(lc);
            mgc.EndGameMessage = endMessage;
        }

        

        SeparatorWithLabel CreateSeparator(LayoutParent layout, ExpandableManager expand, string textKey)
        {
            // Separator instancing
            var text = CanvasMaker.CreateTextUnit(mgc.ButtonObjectRequest.SecondaryColor, mgc.ButtonObjectRequest.font, 16);
            var image = CanvasMaker.CreateSimpleImage(mgc.ButtonObjectRequest.SecondaryColor);
            var swl = new SeparatorWithLabel(text, image);
            swl.LayoutChild.RectTransform.gameObject.name = $"SEP_{textKey}";
            layout.AddLayoutChildAndParentIt(swl.LayoutChild);
            swl.Text.SetTextRaw(textKey);
            expand.ExpandTargets.Add(swl.LayoutChild.GameObject);
            return swl;
        }

        TripleTextView CreateTripleTextView(LayoutParent layout, ExpandableManager expand)
        {
            TripleTextView ttv = CanvasMaker.CreateTripleTextView(mgc.ButtonObjectRequest);
            layout.AddLayoutChildAndParentIt(ttv.LayoutChild);
            expand.ExpandTargets.Add(ttv.LayoutChild.RectTransform.gameObject);
            return ttv;
        }

        void CreateModViews(RuntimeUnit item, LayoutParent layout, List<SeparatorWithLabel> separators, ModsControlUnit ModUnit, ExpandableManager expandManager)
        {
            if (item.ModsOwned.Count == 0) return;
            var sep = CreateSeparator(layout, expandManager, "Mods:");
            separators.Add(sep);
            foreach (var md in item.ModsOwned)
            {

                var ttv = CreateTripleTextView(layout, expandManager);
                ModUnit.ModTTVs.Add(ttv);
            }
        }
    }

    internal static void CreateLogControlUnit(MainGameControl mgc, TabControlUnit tabControl, LayoutParent lp, LogUnit logUnit)
    {
        var text = CanvasMaker.CreateTextUnit(mgc.ButtonObjectRequest.SecondaryColor, mgc.ButtonObjectRequest.font, 16);
        // var image = CanvasMaker.CreateSimpleImage(mgc.ButtonObjectRequest.SecondaryColor);
        var lc = LayoutChild.Create(text.transform);
        if (logUnit.logType == LogUnit.LogType.UNIT_UNLOCKED) {
            text.rawText = $"Unlocked {logUnit.Unit.ConfigBasic.name}";
        }
        lp.AddLayoutChildAndParentIt(lc);
        text.RectTransform.FillParent();
        lc.RectTransform.SetHeight(30);
        tabControl.LogControlUnits.Add(new LogControlUnit());
    }
}
