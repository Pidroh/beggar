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


        foreach (var item in arcaniaDatas.datas[UnitType.TAB])
        {
            var button = CanvasMaker.CreateButton("sss", mgc.ButtonObjectRequest, mgc.ButtonRequest);
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

        for (int tabIndex = 0; tabIndex < mgc.TabControlUnits.Count; tabIndex++)
        {
            TabControlUnit tabControl = mgc.TabControlUnits[tabIndex];
            var UnitGroupControls = tabControl.UnitGroupControls;

            foreach (var pair in UnitGroupControls)
            {
                foreach (var item in arcaniaDatas.datas[pair.Key])
                {
                    var layout = CanvasMaker.CreateLayout().SetFitHeight(true);
                    var hasBWE = pair.Key != UnitType.RESOURCE && pair.Key != UnitType.FURNITURE;

                    var tcu = new RTControlUnit();

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

                        // value text instantiation
                        {
                            var t = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.ButtonObjectRequest.font, 16);
                            t.text.horizontalAlignment = HorizontalAlignmentOptions.Right;
                            // bwe.ExpandTargets
                            tcu.ValueText = t;
                            t.SetParent(tcu.lwe.MainText.RectTransform);
                            t.RectTransform.FillParent();
                            t.RectTransform.SetOffsets(new RectOffset(20, 20, 10, 0));
                        }
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


                    pair.Value.Add(tcu);
                    tcu.Data = item;
                    {
                        var t = CanvasMaker.CreateTextUnit(mgc.MainTextColor, mgc.ButtonObjectRequest.font, 16);
                        t.text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                        // bwe.ExpandTargets
                        tcu.Description = new SimpleChild<UIUnit>(t, t.RectTransform);
                        tcu.Description.RectOffset = new RectOffset(20, 20, 0, 0);
                        if(hasBWE)
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



        SeparatorWithLabel CreateSeparator(LayoutParent layout, ExpandableManager expand, string textKey)
        {
            // Separator instancing
            var text = CanvasMaker.CreateTextUnit(mgc.ButtonObjectRequest.SecondaryColor, mgc.ButtonObjectRequest.font, 16);
            var image = CanvasMaker.CreateSimpleImage(mgc.ButtonObjectRequest.SecondaryColor);
            var swl = new SeparatorWithLabel(text, image);
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
}
