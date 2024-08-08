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
    public Dictionary<UnitType, List<TaskControlUnit>> UnitGroupControls = new()
    {
        { UnitType.TASK, new List<TaskControlUnit>() },
        { UnitType.CLASS, new List<TaskControlUnit>() },
        { UnitType.SKILL, new List<TaskControlUnit>() }
    };
    public Dictionary<UnitType, List<ResourceControlUnit>> UnitGroupResourceControls = new()
    {
        { UnitType.RESOURCE, new List<ResourceControlUnit>() },
    };
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
        dynamicCanvas = CanvasMaker.CreateCanvas(1, CanvasRequest);
        var lowerMenu = dynamicCanvas.CreateLowerMenu(60);
        var lowerMenuLayout = CanvasMaker.CreateLayout(lowerMenu);

        foreach (var item in arcaniaDatas.datas[UnitType.TAB])
        {
            var button = CanvasMaker.CreateButton("sss", ButtonObjectRequest, ButtonRequest);
            var lc = new LayoutChild() {
                RectTransform = button.Button.RectTransform
            };
            lowerMenuLayout.AddLayoutChildAndParentIt(lc);
            var tcu = new TabControlUnit() {
                SelectionButton = lc,
                TabData = item
            };
            TabControlUnits.Add(tcu);
            
        }
        
        foreach (var pair in UnitGroupResourceControls)
        {
            foreach (var item in arcaniaDatas.datas[pair.Key])
            {
                var layout = CanvasMaker.CreateLayout().SetFitHeight(true);
                var titleText = CanvasMaker.CreateTextUnit(ButtonObjectRequest.SecondaryColor, ButtonObjectRequest.font, 16);
                var iconButton = CanvasMaker.CreateButtonWithIcon(ExpanderSprite);
                var lwe = new LabelWithExpandable(iconButton, titleText);
                var rcu = new ResourceControlUnit();
                dynamicCanvas.children[0].AddLayoutAndParentIt(layout);
                layout.AddLayoutChildAndParentIt(lwe.LayoutChild);
                titleText.SetTextRaw(item.ConfigBasic.name);
                pair.Value.Add(rcu);
                rcu.lwe = lwe;
                rcu.Data = item;
                {
                    var t = CanvasMaker.CreateTextUnit(MainTextColor, ButtonObjectRequest.font, 16);
                    t.text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                    // bwe.ExpandTargets
                    rcu.Description = new SimpleChild<UIUnit>(t, t.RectTransform);
                    rcu.Description.RectOffset = new RectOffset(20, 20, 0, 0);
                    layout.AddLayoutChildAndParentIt(rcu.Description.LayoutChild);
                }
                {
                    var t = CanvasMaker.CreateTextUnit(MainTextColor, ButtonObjectRequest.font, 16);
                    t.text.horizontalAlignment = HorizontalAlignmentOptions.Right;
                    // bwe.ExpandTargets
                    rcu.ValueText = t;
                    t.SetParent(rcu.lwe.MainText.RectTransform);
                    t.RectTransform.FillParent();
                    t.RectTransform.SetOffsets(new RectOffset(20, 20, 10, 0));

                }
                CreateModViews(item, layout, rcu.Separators, rcu.ModsUnit, lwe.ExpandManager);
            }
        }
        foreach (var pair in UnitGroupControls)
        {
            foreach (var item in arcaniaDatas.datas[pair.Key])
            {
                var layout = CanvasMaker.CreateLayout().SetFitHeight(true);
                var button = CanvasMaker.CreateButton(item.ConfigBasic.name, ButtonObjectRequest, ButtonRequest);
                var iconButton = CanvasMaker.CreateButtonWithIcon(ExpanderSprite);
                var bwe = new ButtonWithExpandable(button, iconButton);
                var tcu = new TaskControlUnit();
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
                dynamicCanvas.children[0].AddLayoutAndParentIt(layout);
                layout.AddLayoutChildAndParentIt(bwe);
                button.Button.SetTextRaw(item.ConfigBasic.name);

                pair.Value.Add(tcu);
                tcu.bwe = bwe;
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
                    var arrayOfChanges = item.ConfigTask.GetResourceChangeList(i);
                    var rcgIndex = i;

                    if (arrayOfChanges != null)
                    {
                        tcu.ChangeGroups[rcgIndex] = new TaskControlUnit.ResourceChangeGroup();
                        string textKey = (ResourceChangeType)rcgIndex switch
                        {
                            ResourceChangeType.COST => "cost",
                            ResourceChangeType.RESULT => "result",
                            ResourceChangeType.RUN => "run",
                            ResourceChangeType.EFFECT => "effect",
                            _ => null,
                        };
                        SeparatorWithLabel swl = CreateSeparator(layout, tcu.bwe.ExpandManager, textKey);

                        tcu.ChangeGroupSeparators[rcgIndex] = swl;
                    }
                    foreach (var changeU in arrayOfChanges)
                    {
                        TripleTextView ttv = CreateTripleTextView(layout, bwe.ExpandManager);
                        tcu.ChangeGroups[rcgIndex].tripleTextViews.Add(ttv);
                    }
                }


                List<SeparatorWithLabel> separators = tcu.Separators;
                var ModUnit = tcu.ModsUnit;
                ExpandableManager expandManager = bwe.ExpandManager;
                CreateModViews(item, layout, separators, ModUnit, expandManager);

                void AddToExpands(LayoutChild c)
                {
                    tcu.bwe.ExpandTargets.Add(c.GameObject);
                    layout.AddLayoutChildAndParentIt(c);
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

    // Update is called once per frame
    void Update()
    {
        arcaniaModel.ManualUpdate(Time.deltaTime);
        dynamicCanvas.ManualUpdate();

        foreach (var pair in UnitGroupResourceControls)
        {
            foreach (var rcu in pair.Value)
            {
                var data = rcu.Data;
                rcu.ManualUpdate();
                bool visible = data.Visible;
                rcu.lwe.LayoutChild.RectTransform.parent.gameObject.SetActive(visible);
                if (!visible) continue;
                var modUnit = rcu.ModsUnit;
                FeedMods(data, modUnit);

            }
        }

        foreach (var pair in UnitGroupControls)
        {
            foreach (var tcu in pair.Value)
            {
                var data = tcu.Data;
                tcu.ManualUpdate();
                bool visible = data.Visible;
                tcu.bwe.LayoutChild.RectTransform.parent.gameObject.SetActive(visible);
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
                    case UnitType.RESOURCE:
                    case UnitType.TASK:
                    case UnitType.HOUSE:
                    case UnitType.CLASS:
                    case UnitType.FURNITURE:
                        {
                            tcu.bwe.MainButton.ButtonEnabled = arcaniaModel.Runner.CanStartAction(data);
                            if (tcu.TaskClicked)
                            {
                                arcaniaModel.Runner.StartAction(data);
                            }
                        }
                        break;
                    default:
                        break;
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
