using HeartUnity;
using HeartUnity.View;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class MainGameControlSetup
{
    public static void Setup(MainGameControl mgc)
    {
        var arcaniaModel = mgc.arcaniaModel;
        var arcaniaDatas = arcaniaModel.arcaniaUnits;
        JsonReader.ReadJson(mgc.ResourceJson, arcaniaDatas);

        arcaniaModel.FinishedSettingUpUnits();
        var config = HeartGame.GetConfig();
        var dynamicCanvas = CanvasMaker.CreateCanvas(Mathf.Max(arcaniaDatas.datas[UnitType.TAB].Count, 1), mgc.CanvasRequest, config.reusableCanvas);
        mgc.dynamicCanvas = dynamicCanvas;
        var lowerMenuLayout = dynamicCanvas.CreateLowerMenuLayout(60).SetStretchWidth(true).SetLayoutType(LayoutParent.LayoutType.HORIZONTAL);
        mgc.TabButtonLayout = lowerMenuLayout;

        mgc.EngineView = mgc.HeartGame.CreateEngineView(new EngineView.EngineViewInitializationParameter()
        {
            canvas = dynamicCanvas.Canvas
        }, 2);


        dynamicCanvas.AddDialog(CanvasMaker.CreateDialog(mgc.DialogObjectRequest, mgc.ButtonObjectRequest, mgc.ButtonRequest));

        // -------------------------------------------------
        // TAB BUTTON INSTANTIATING AND OTHER SMALL SETUP
        // -------------------------------------------------
        for (int tabIndex = 0; tabIndex < arcaniaDatas.datas[UnitType.TAB].Count; tabIndex++)
        {
            RuntimeUnit item = arcaniaDatas.datas[UnitType.TAB][tabIndex];
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
            dynamicCanvas.children[tabIndex].SelfChild.GameObject.name = $"tab_{item.Name}";
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

        // -------------------------------------------------
        // MAIN GRAPHIC ELEMENT INSTANTIATING
        // -------------------------------------------------
        #region MAIN GRAPHIC ELEMENT INSTANTIATING
        for (int tabIndex = 0; tabIndex < mgc.TabControlUnits.Count; tabIndex++)
        {
            TabControlUnit tabControl = mgc.TabControlUnits[tabIndex];
            // Tab separator instancing
            foreach (var sep in tabControl.Separators)
            {
                var unitGroupControls = tabControl.UnitGroupControls;

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
                    sep.Text = text;

                    if (!sep.Data.ShowSpace) goto END_OF_SEPARATOR_INSTANCE;
                    var spaceT = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.Font, 18);
                    dynamicCanvas.children[tabIndex].AddLayoutChildAndParentIt(spaceT);
                    sep.SpaceAmountText = spaceT;
                }
            // -------------------------------------------------
            END_OF_SEPARATOR_INSTANCE:
                foreach (var pair in unitGroupControls)
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
                            if (candidate.Data.RequireInstant && item.ConfigTask.Duration > 0) continue;
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

                        var rcu = new RTControlUnit();
                        rcu.ParentTabSeparator = unitSeparator;

                        if (hasBWE)
                        {
                            var button = CanvasMaker.CreateButton(item.ConfigBasic.name, mgc.ButtonObjectRequest, mgc.ButtonRequest);
                            var iconButton = CanvasMaker.CreateButtonWithIcon(mgc.ExpanderSprite);
                            var bwe = new ButtonWithExpandable(button, iconButton);
                            rcu.bwe = bwe;
                        }

                        if (!hasBWE)
                        {
                            var titleText = CanvasMaker.CreateTextUnitClickable(mgc.ButtonObjectRequest.SecondaryColor, mgc.ButtonObjectRequest.font, 16);
                            titleText.text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                            var iconButton = CanvasMaker.CreateButtonWithIcon(mgc.ExpanderSprite);
                            var lwe = new LabelWithExpandable(iconButton, titleText);

                            layout.AddLayoutChildAndParentIt(lwe.LayoutChild);
                            titleText.SetTextRaw(item.ConfigBasic.name);
                            rcu.lwe = lwe;
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
                            layoutAddRemove.SelfChild.SetPreferredHeightMM(10);
                            layout.AddLayoutAndParentIt(layoutAddRemove);
                            rcu.ButtonRemove = buttonRemove;
                            rcu.ButtonAdd = buttonAdd;
                        }
                        if (pair.Key == UnitType.SKILL)
                        {
                            {
                                var t = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.ButtonObjectRequest.font, mgc.SkillFontSize);
                                t.text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                                // bwe.ExpandTargets
                                rcu.MainTitle = new SimpleChild<UIUnit>(t, t.RectTransform);
                                rcu.MainTitle.RectOffset = new RectOffset(20, 20, 10, 0);
                                layout.AddLayoutChildAndParentIt(rcu.MainTitle.LayoutChild);
                            }
                            {
                                var t = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.ButtonObjectRequest.font, mgc.SkillFontSize);
                                t.text.horizontalAlignment = HorizontalAlignmentOptions.Right;
                                // bwe.ExpandTargets
                                rcu.SkillLevelText = t;
                                t.SetParent(rcu.MainTitle.ElementRectTransform);
                                t.RectTransform.FillParent();
                                // t.RectTransform.SetOffsets(new RectOffset(0, 0, 0, 0));

                            }
                            rcu.XPGauge = new Gauge(mgc.SkillXPGaugeRequest, 4);
                            layout.AddLayoutChildAndParentIt(rcu.XPGauge.layoutChild);
                        }
                        dynamicCanvas.children[tabIndex].AddLayoutAndParentIt(layout);

                        if (hasBWE)
                        {
                            layout.AddLayoutChildAndParentIt(rcu.bwe);
                            rcu.bwe.MainButton.SetTextRaw(item.ConfigBasic.name);
                        }

                        // value text instantiation
                        {
                            var t = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.ButtonObjectRequest.font, 16);
                            t.text.horizontalAlignment = HorizontalAlignmentOptions.Right;
                            // bwe.ExpandTargets
                            rcu.ValueText = t;
                            if (valueTextIsOnLWE)
                            {
                                t.SetParent(rcu.lwe.MainText.RectTransform);
                                t.RectTransform.FillParent();
                                // t.RectTransform.SetOffsets(new RectOffset(0, 0, 0, 0));
                            }
                            else
                            {
                                var lc = LayoutChild.Create(t.transform);
                                AddToExpands(lc, layout, rcu);
                                t.RectTransform.FillParent();
                                lc.RectTransform.SetHeight(20);
                            }

                        }

                        pair.Value.Add(rcu);
                        rcu.Data = item;
                        {
                            var addToExpands = hasBWE;
                            AddDescription(mgc, layout, rcu, addToExpands);

                        }

                        for (int i = 0; i < (int) ResourceChangeType.MAX; i++)
                        {
                            if (item.ConfigTask == null) break;
                            var arrayOfChanges = item.ConfigTask.GetResourceChangeList(i);
                            if (arrayOfChanges == null) continue;
                            int count = arrayOfChanges.Count;
                            
                            CreateResourceChangeViews(i, count, rcu, layout);


                        }

                        //-------------------------------------------------------
                        // Mod #mod
                        //-------------------------------------------------------
                        List<SeparatorWithLabel> separators = rcu.Separators;
                        var ModUnit = rcu.ModsUnit;
                        ExpandableManager expandManager = rcu.ExpandManager;
                        CreateModViews(item, layout, separators, ModUnit, expandManager);
                        CreateModIntermediaryViews(item, layout, separators, ModUnit, expandManager);
                        //-------------------------------------------------------
                        // Need #need #condition
                        //-------------------------------------------------------
                        {
                            var requirements = item.ConfigTask?.Need;
                            if (requirements != null)
                            {
                                var sepLabel = "Needs:";
                                var sepaNeed = CreateSeparator(layout, expandManager, sepLabel);
                                separators.Add(sepaNeed);
                                var ttv = CreateTripleTextView(layout, expandManager);
                                rcu.needConditionUnit.TTV = ttv;
                                rcu.needConditionUnit.TTV.MainText.SetTextKey(requirements.humanExpression);
                            }
                        }

                        //-------------------------------------------------------
                        // Duration #duration
                        //-------------------------------------------------------
                        if (hasBWE)
                        {
                            var t = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.ButtonObjectRequest.font, 16);
                            t.text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                            // bwe.ExpandTargets
                            rcu.DurationText = new SimpleChild<UIUnit>(t, t.RectTransform);
                            rcu.DurationText.RectOffset = new RectOffset(20, 20, 0, 0);
                            rcu.DurationText.LayoutChild.PreferredSizeMM[1] = 10;
                            rcu.DurationText.ManualUpdate();
                            AddToExpands(rcu.DurationText.LayoutChild, layout, rcu);
                        }

                        
                    }
                }
            }

        }
        #endregion

        #region EXPLORATION GRAPHICS INSTANTIATING
        for (int tabIndex = 0; tabIndex < mgc.TabControlUnits.Count; tabIndex++)
        {
            TabControlUnit tab = mgc.TabControlUnits[tabIndex];
            if (!tab.TabData.Tab.ExplorationActiveTab) continue;
            

            for (int indexExplorationElement = 0; indexExplorationElement < 4; indexExplorationElement++)
            {
                int numberOfEles = 1;
                bool isStressors = indexExplorationElement == 2;
                var fleeButton = indexExplorationElement == 3;
                if (isStressors) 
                {
                    numberOfEles = mgc.arcaniaModel.Exploration.Stressors.Count;
                }
                for (int eleIndex = 0; eleIndex < numberOfEles; eleIndex++)
                {
                    var layout = CanvasMaker.CreateLayout().SetFitHeight(true);
                    dynamicCanvas.children[tabIndex].AddLayoutAndParentIt(layout);
                    var hasBWE = fleeButton;
                    var rcu = new RTControlUnit();
                    if (isStressors)
                    {
                        rcu.Data = mgc.arcaniaModel.Exploration.Stressors[eleIndex];
                    }
                    rcu.ParentTabSeparator = null;
                    if (hasBWE)
                    {
                        var button = CanvasMaker.CreateButton("Flee", mgc.ButtonObjectRequest, mgc.ButtonRequest);
                        var iconButton = CanvasMaker.CreateButtonWithIcon(mgc.ExpanderSprite);
                        var bwe = new ButtonWithExpandable(button, iconButton);
                        rcu.bwe = bwe;
                        layout.AddLayoutChildAndParentIt(bwe.LayoutChild);
                        bwe.LayoutChild.GameObject.name = "WTF IS";
                    }

                    if (!hasBWE)
                    {
                        var titleText = CanvasMaker.CreateTextUnit(mgc.ButtonObjectRequest.SecondaryColor, mgc.ButtonObjectRequest.font, 16);
                        titleText.text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                        var iconButton = CanvasMaker.CreateButtonWithIcon(mgc.ExpanderSprite);
                        var lwe = new LabelWithExpandable(iconButton, titleText);
                        layout.AddLayoutChildAndParentIt(lwe.LayoutChild);
                        titleText.SetTextRaw("Location name");
                        rcu.lwe = lwe;
                        rcu.XPGauge = new Gauge(mgc.SkillXPGaugeRequest, 4);
                        layout.AddLayoutChildAndParentIt(rcu.XPGauge.layoutChild);
                    }
                    switch (indexExplorationElement)
                    {
                        case 0:
                            mgc.controlExploration.dataHolder.LocationRCU = rcu;
                            CreateReserveChangeViews(mgc, layout, rcu);
                            break;
                        case 1: // encounter
                            mgc.controlExploration.dataHolder.EncounterRCU = rcu;
                            AddDescription(mgc, layout, rcu, true);
                            CreateReserveChangeViews(mgc, layout, rcu);
                            break;
                        case 2:
                            mgc.controlExploration.dataHolder.StressorsRCU.Add(rcu);
                            break;
                        case 3:
                            mgc.controlExploration.dataHolder.FleeRCU = rcu;
                            break;
                        default:
                            break;
                    }
                }
                
                // if it's the enemy encounter one
                
            }
            mgc.controlExploration.dataHolder.FinishSetup();

        }
        #endregion

        // end game message related
        {
            mgc.EndGameRuntimeUnit = arcaniaModel.FindRuntimeUnit(UnitType.TASK, "ponderexistence");
            var endMessage = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.Font, 18);
            endMessage.rawText = $"GAME CLEARED \nYou have become one with existence. \n At least until more content is added. \n\n Let me know you finished the game by sending me: \"I'm the beggar's journey\".\n\n\n You can use Reddit, email, the Discord channel, etc";
            var lc = LayoutChild.Create(endMessage.transform);
            lc.AddTextDrivenHeight(endMessage, 10f);

            var settingB = CanvasMaker.CreateButton("Settings", mgc.ButtonObjectRequest, mgc.ButtonRequest);
            
            endMessage.RectTransform.FillParent();
            dynamicCanvas.OverlayMainLayout.AddLayoutChildAndParentIt(lc);
            dynamicCanvas.OverlayMainLayout.AddLayoutChildAndParentIt(LayoutChild.Create(settingB.Button.transform));
            settingB.Button.transform.localPosition = Vector3.zero;
            mgc.EndGameMessage = endMessage;
            mgc.SettingButtonEnding = settingB;
        }

        void CreateResourceChangeViews(int rcgIndex, int count, RTControlUnit rcu, LayoutParent layout)
        {
            rcu.ChangeGroups[rcgIndex] = new ResourceChangeGroup();
            string textKey = (ResourceChangeType)rcgIndex switch
            {
                ResourceChangeType.COST => "cost",
                ResourceChangeType.RESULT => "result",
                ResourceChangeType.RUN => "run",
                ResourceChangeType.EFFECT => "effect",
                ResourceChangeType.RESULT_ONCE => "first time",
                ResourceChangeType.RESULT_FAIL => "result failure",
                _ => null,
            };
            SeparatorWithLabel swl = CreateSeparator(layout, rcu.ExpandManager, textKey);

            rcu.ChangeGroupSeparators[rcgIndex] = swl;

            for (int arrayChangePos = 0; arrayChangePos < count; arrayChangePos++)
            {
                TripleTextView ttv = CreateTripleTextView(layout, rcu.ExpandManager);
                rcu.ChangeGroups[rcgIndex].tripleTextViews.Add(ttv);
            }
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
            List<TripleTextView> modTTVs = ModUnit.ModTTVs;
            var sep = CreateSeparator(layout, expandManager, "Mods:");
            separators.Add(sep);
            foreach (var md in item.ModsOwned)
            {

                var ttv = CreateTripleTextView(layout, expandManager);
                ttv.Mode = TripleTextView.TTVMode.PrimarySecondary;
                
                modTTVs.Add(ttv);
            }
        }

        void CreateModIntermediaryViews(RuntimeUnit item, LayoutParent layout, List<SeparatorWithLabel> separators, ModsControlUnit ModUnit, ExpandableManager expandManager)
        {
            if (item.ModsSelfAsIntermediary.Count == 0) return;
            List<TripleTextView> modTTVs = ModUnit.ModIntermediaryTTVs;

            var sep = CreateSeparator(layout, expandManager, "extra mods:");
            ModUnit.ExtraModSeparator = sep;
            separators.Add(sep);
            foreach (var md in item.ModsSelfAsIntermediary)
            {

                var ttv = CreateTripleTextView(layout, expandManager);
                ttv.Mode = TripleTextView.TTVMode.PrimarySecondary;

                modTTVs.Add(ttv);
            }
        }


        void CreateReserveChangeViews(MainGameControl mgc, LayoutParent layout, RTControlUnit rcu)
        {
            for (int i = 0; i < (int) ResourceChangeType.MAX; i++)
            {
                CreateResourceChangeViews(i, 5, rcu, layout);
            }
        }

        void AddToExpands(LayoutChild c, LayoutParent layout, RTControlUnit rcu)
        {
            rcu.ExpandManager.ExpandTargets.Add(c.GameObject);
            layout.AddLayoutChildAndParentIt(c);
        }

        void AddDescription(MainGameControl mgc, LayoutParent layout, RTControlUnit rcu, bool addToExpands)
        {
            var t = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.ButtonObjectRequest.font, 16);
            t.text.horizontalAlignment = HorizontalAlignmentOptions.Left;
            t.text.verticalAlignment = VerticalAlignmentOptions.Top;
            // bwe.ExpandTargets
            rcu.Description = new SimpleChild<UIUnit>(t, t.RectTransform);
            rcu.Description.RectOffset = new RectOffset(20, 20, 0, 10);
            if (addToExpands)
                AddToExpands(rcu.Description.LayoutChild, layout, rcu);
            else
                layout.AddLayoutChildAndParentIt(rcu.Description.LayoutChild);
        }
    }

    internal static void CreateLogControlUnit(MainGameControl mgc, TabControlUnit tabControl, LayoutParent lp, LogUnit logUnit)
    {
        var text = CanvasMaker.CreateTextUnit(mgc.ButtonObjectRequest.SecondaryColor, mgc.ButtonObjectRequest.font, 16);
        // var image = CanvasMaker.CreateSimpleImage(mgc.ButtonObjectRequest.SecondaryColor);
        var lc = LayoutChild.Create(text.transform);
        if (logUnit.logType == LogUnit.LogType.UNIT_UNLOCKED)
        {
            text.rawText = $"Unlocked {logUnit.Unit.ConfigBasic.name}";
        }
        lp.AddLayoutChildAndParentIt(lc);
        text.RectTransform.FillParent();
        lc.RectTransform.SetHeight(30);
        tabControl.LogControlUnits.Add(new LogControlUnit(lc, text));
    }
}
