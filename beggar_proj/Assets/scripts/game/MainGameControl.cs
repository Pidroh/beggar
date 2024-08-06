using HeartUnity.View;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainGameControl : MonoBehaviour
{

    public TextAsset ResourceJson;
    private DynamicCanvas dynamicCanvas;
    public Dictionary<UnitType, List<TaskControlUnit>> UnitGroupControls = new()
    {
        { UnitType.TASK, new List<TaskControlUnit>() },
        { UnitType.CLASS, new List<TaskControlUnit>() },
        { UnitType.SKILL, new List<TaskControlUnit>() }
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
                    var t = CanvasMaker.CreateTextUnit(MainTextColor, ButtonObjectRequest.font);
                    // bwe.ExpandTargets
                    tcu.Description = new SimpleChild<UIUnit>(t, t.RectTransform);
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
                        SeparatorWithLabel swl = CreateSeparator(layout, tcu, textKey);

                        tcu.ChangeGroupSeparators[rcgIndex] = swl;
                    }
                    foreach (var changeU in arrayOfChanges)
                    {
                        TripleTextView ttv = CreateTripleTextView(layout, bwe);
                        tcu.ChangeGroups[rcgIndex].tripleTextViews.Add(ttv);
                    }
                }

                var sep = CreateSeparator(layout, tcu, "Mods:");
                tcu.Separators.Add(sep);
                foreach (var md in item.ModsOwned)
                {
                    var ttv = CreateTripleTextView(layout, bwe);
                    tcu.ModTTVs.Add(ttv);
                }

                void AddToExpands(LayoutChild c)
                {
                    tcu.bwe.ExpandTargets.Add(c.GameObject);
                    layout.AddLayoutChildAndParentIt(c);
                }
            }
        }


        SeparatorWithLabel CreateSeparator(LayoutParent layout, TaskControlUnit tcu, string textKey)
        {
            // Separator instancing
            var text = CanvasMaker.CreateTextUnit(ButtonObjectRequest.SecondaryColor, ButtonObjectRequest.font);
            var image = CanvasMaker.CreateSimpleImage(ButtonObjectRequest.SecondaryColor);
            var swl = new SeparatorWithLabel(text, image);
            layout.AddLayoutChildAndParentIt(swl.LayoutChild);
            swl.Text.SetTextRaw(textKey);
            tcu.bwe.ExpandTargets.Add(swl.LayoutChild.GameObject);
            return swl;
        }

        TripleTextView CreateTripleTextView(LayoutParent layout, ButtonWithExpandable bwe)
        {
            TripleTextView ttv = CanvasMaker.CreateTripleTextView(ButtonObjectRequest);
            layout.AddLayoutChildAndParentIt(ttv.LayoutChild);
            bwe.ExpandTargets.Add(ttv.LayoutChild.RectTransform.gameObject);
            return ttv;
        }
    }

    // Update is called once per frame
    void Update()
    {
        arcaniaModel.ManualUpdate(Time.deltaTime);
        dynamicCanvas.ManualUpdate();

        foreach (var pair in UnitGroupControls)
        {
            foreach (var tcu in pair.Value)
            {
                var data = tcu.Data;
                tcu.ManualUpdate();
                bool visible = data.Visible;
                tcu.bwe.LayoutChild.RectTransform.parent.gameObject.SetActive(visible);
                if (!visible) continue;
                for (int i = 0; i < data.ModsOwned.Count; i++)
                {
                    ModRuntime md = data.ModsOwned[i];
                    var ttv = tcu.ModTTVs[i];
                    ttv.MainText.rawText = md.SourceJsonKey;
                    ttv.SecondaryText.rawText = $"{md.Value}";
                    ttv.TertiaryText.rawText = string.Empty;
                    ttv.ManualUpdate();
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

    }
}
