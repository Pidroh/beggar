using HeartUnity.View;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainGameControl : MonoBehaviour
{

    public TextAsset ResourceJson;
    private DynamicCanvas dynamicCanvas;
    public List<TabControlUnit> TabControlUnits = new();

    [SerializeField]
    public TMP_FontAsset Font;
    public Sprite ExpanderSprite;
    public CanvasMaker.CreateObjectRequest ButtonObjectRequest;
    public CanvasMaker.CreateButtonRequest ButtonRequest;
    public CanvasMaker.CreateCanvasRequest CanvasRequest;
    public CanvasMaker.CreateGaugeRequest SkillXPGaugeRequest;
    public ArcaniaModel arcaniaModel = new();

    public Color MainTextColor;


    // Start is called before the first frame update
    void Start()
    {
        var arcaniaDatas = arcaniaModel.arcaniaUnits;
        JsonReader.ReadJson(ResourceJson.text, arcaniaDatas);
        dynamicCanvas = CanvasMaker.CreateCanvas(Mathf.Max(arcaniaDatas.datas[UnitType.TAB].Count, 1), CanvasRequest);
        var lowerMenuLayout = dynamicCanvas.CreateLowerMenuLayout(60).SetStretchWidth(true).SetLayoutType(LayoutParent.LayoutType.HORIZONTAL);


        foreach (var item in arcaniaDatas.datas[UnitType.TAB])
        {
            var button = CanvasMaker.CreateButton("sss", ButtonObjectRequest, ButtonRequest);
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
            TabControlUnits.Add(tcu);
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

        for (int tabIndex = 0; tabIndex < TabControlUnits.Count; tabIndex++)
        {
            TabControlUnit tabControl = TabControlUnits[tabIndex];
            var UnitGroupControls = tabControl.UnitGroupControls;
           
            foreach (var pair in UnitGroupControls)
            {
                foreach (var item in arcaniaDatas.datas[pair.Key])
                {
                    var layout = CanvasMaker.CreateLayout().SetFitHeight(true);
                    var hasBWE = pair.Key != UnitType.RESOURCE && pair.Key != UnitType.FURNITURE;

                    
                    
                    SimpleChild<UIUnit> secondaryButton = null;
                    var tcu = new RTControlUnit();

                    if (hasBWE)
                    {
                        var button = CanvasMaker.CreateButton(item.ConfigBasic.name, ButtonObjectRequest, ButtonRequest);
                        var iconButton = CanvasMaker.CreateButtonWithIcon(ExpanderSprite);
                        var bwe = new ButtonWithExpandable(button, iconButton);
                        tcu.bwe = bwe;
                    }

                    if(!hasBWE)
                    {
                        var titleText = CanvasMaker.CreateTextUnit(ButtonObjectRequest.SecondaryColor, ButtonObjectRequest.font, 16);
                        var iconButton = CanvasMaker.CreateButtonWithIcon(ExpanderSprite);
                        var lwe = new LabelWithExpandable(iconButton, titleText);

                        layout.AddLayoutChildAndParentIt(lwe.LayoutChild);
                        titleText.SetTextRaw(item.ConfigBasic.name);
                        tcu.lwe = lwe;
                    }

                    if (pair.Key == UnitType.FURNITURE) 
                    {
                        var buttonRemove = CanvasMaker.CreateButton("-", ButtonObjectRequest, ButtonRequest);
                        secondaryButton = new SimpleChild<UIUnit>(buttonRemove.Button, buttonRemove.Button.RectTransform);
                    }
                    if (pair.Key == UnitType.SKILL)
                    {
                        {
                            var t = CanvasMaker.CreateTextUnit(MainTextColor, ButtonObjectRequest.font, 30);
                            t.text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                            // bwe.ExpandTargets
                            tcu.MainTitle = new SimpleChild<UIUnit>(t, t.RectTransform);
                            tcu.MainTitle.RectOffset = new RectOffset(20, 20, 10, 0);
                            layout.AddLayoutChildAndParentIt(tcu.MainTitle.LayoutChild);
                        }
                        {
                            var t = CanvasMaker.CreateTextUnit(MainTextColor, ButtonObjectRequest.font, 30);
                            t.text.horizontalAlignment = HorizontalAlignmentOptions.Right;
                            // bwe.ExpandTargets
                            tcu.SkillLevelText = t;
                            t.SetParent(tcu.MainTitle.ElementRectTransform);
                            t.RectTransform.FillParent();
                            t.RectTransform.SetOffsets(new RectOffset(20, 20, 10, 0));

                        }
                        tcu.XPGauge = new Gauge(SkillXPGaugeRequest);
                        layout.AddLayoutChildAndParentIt(tcu.XPGauge.layoutChild);
                    }
                    dynamicCanvas.children[tabIndex].AddLayoutAndParentIt(layout);

                    if (hasBWE) { 
                        layout.AddLayoutChildAndParentIt(tcu.bwe); 
                        tcu.bwe.MainButton.SetTextRaw(item.ConfigBasic.name);
                    }
                    

                    if (secondaryButton != null)
                    {
                        layout.AddLayoutChildAndParentIt(secondaryButton.LayoutChild);
                        tcu.SecondaryButton = secondaryButton;
                    }

                    pair.Value.Add(tcu);
                    tcu.Data = item;
                    {
                        var t = CanvasMaker.CreateTextUnit(MainTextColor, ButtonObjectRequest.font, 16);
                        t.text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                        // bwe.ExpandTargets
                        tcu.Description = new SimpleChild<UIUnit>(t, t.RectTransform);
                        tcu.Description.RectOffset = new RectOffset(20, 20, 0, 0);
                        AddToExpands(tcu.Description.LayoutChild);

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
            var text = CanvasMaker.CreateTextUnit(ButtonObjectRequest.SecondaryColor, ButtonObjectRequest.font, 16);
            var image = CanvasMaker.CreateSimpleImage(ButtonObjectRequest.SecondaryColor);
            var swl = new SeparatorWithLabel(text, image);
            layout.AddLayoutChildAndParentIt(swl.LayoutChild);
            swl.Text.SetTextRaw(textKey);
            expand.ExpandTargets.Add(swl.LayoutChild.GameObject);
            return swl;
        }

        TripleTextView CreateTripleTextView(LayoutParent layout, ExpandableManager expand)
        {
            TripleTextView ttv = CanvasMaker.CreateTripleTextView(ButtonObjectRequest);
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

    private TripleTextView CreateTripleTextView(LayoutParent layout, object expandManager)
    {
        throw new System.NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        arcaniaModel.ManualUpdate(Time.deltaTime);
        dynamicCanvas.ManualUpdate();
        // hide lower menu if all the tabs are visible
        dynamicCanvas.LowerMenus[0].SelfChild.Visible = dynamicCanvas.CalculateNumberOfVisibleHorizontalChildren() < arcaniaModel.arcaniaUnits.datas[UnitType.TAB].Count;


        for (int tabIndex = 0; tabIndex < TabControlUnits.Count; tabIndex++)
        {
            TabControlUnit tabControl = TabControlUnits[tabIndex];
            
            if (tabControl.SelectionButton.Button.Clicked)
            {
                dynamicCanvas.ShowChild(tabIndex);
            }
            if (!dynamicCanvas.children[tabIndex].SelfChild.Visible) continue;
            var UnitGroupControls = tabControl.UnitGroupControls;
           

            foreach (var pair in UnitGroupControls)
            {
                foreach (var tcu in pair.Value)
                {
                    var data = tcu.Data;
                    tcu.ManualUpdate();
                    bool visible = data.Visible;
                    tcu.bwe?.LayoutChild.RectTransform.parent.gameObject.SetActive(visible);
                    tcu.lwe?.LayoutChild.RectTransform.parent.gameObject.SetActive(visible);
                    if (!visible) continue;
                    var modUnit = tcu.ModsUnit;
                    FeedMods(data, modUnit);    

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
                                if(!arcaniaModel.Housing.IsLivingInHouse(data)) arcaniaModel.Housing.ChangeHouse(data);
                            }
                            
                            break;
                        case UnitType.RESOURCE:
                        case UnitType.TASK:
                        
                        case UnitType.CLASS:
                        case UnitType.FURNITURE:
                            {
                                // tcu.bwe.MainButton.ButtonEnabled = arcaniaModel.Runner.CanStartAction(data);
                                /*
                                if (tcu.TaskClicked)
                                {
                                    arcaniaModel.Runner.StartAction(data);
                                }
                                */
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

        }

        static void FeedMods(RuntimeUnit data, ModsControlUnit modUnit)
        {
            for (int i = 0; i < data.ModsOwned.Count; i++)
            {
                ModRuntime md = data.ModsOwned[i];
                var ttv = modUnit.ModTTVs[i];
                ttv.MainText.rawText = md.SourceJsonKey;
                ttv.SecondaryText.rawText = $"{md.Value}";
                ttv.TertiaryText.rawText = string.Empty;
                ttv.ManualUpdate();
            }
        }
    }
}
