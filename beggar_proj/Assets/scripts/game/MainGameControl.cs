using HeartUnity.View;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainGameControl : MonoBehaviour
{

    public TextAsset ResourceJson;
    private DynamicCanvas dynamicCanvas;
    public List<TaskControlUnit> TaskControls = new();
    [SerializeField]
    public TMP_FontAsset Font;
    public Sprite ExpanderSprite;
    public CanvasMaker.CreateObjectRequest ButtonObjectRequest;
    public CanvasMaker.CreateCanvasRequest CanvasRequest;
    public ArcaniaModel arcaniaModel = new();

    public Color MainTextColor;


    // Start is called before the first frame update
    void Start()
    {
        var arcaniaDatas = arcaniaModel.arcaniaUnits;
        JsonReader.ReadJson(ResourceJson.text, arcaniaDatas);
        dynamicCanvas = CanvasMaker.CreateCanvas(1, CanvasRequest);
        foreach (var item in arcaniaDatas.datas[UnitType.TASK])
        {
            var layout = CanvasMaker.CreateLayout().SetFitHeight(true);
            var button = CanvasMaker.CreateButton(item.ConfigBasic.name, ButtonObjectRequest);
            var iconButton = CanvasMaker.CreateButtonWithIcon(ExpanderSprite);
            var bwe = new ButtonWithExpandable(button, iconButton);
            dynamicCanvas.children[0].AddLayoutAndParentIt(layout);
            layout.AddLayoutChildAndParentIt(bwe);
            button.SetTextRaw(item.ConfigBasic.name);
            var tcu = new TaskControlUnit();
            TaskControls.Add(tcu);
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
                if (i == 0) 
                {
                    
                }
                if (arrayOfChanges != null) {
                    tcu.ChangeGroups[rcgIndex] = new TaskControlUnit.ResourceChangeGroup();
                    var text = CanvasMaker.CreateTextUnit(ButtonObjectRequest.SecondaryColor, ButtonObjectRequest.font);
                    var image = CanvasMaker.CreateSimpleImage(ButtonObjectRequest.SecondaryColor);
                    var swl = new SeparatorWithLabel(text, image);
                    layout.AddLayoutChildAndParentIt(swl.LayoutChild);
                    tcu.ChangeGroupSeparators[rcgIndex] = swl;
                    string textKey = (ResourceChangeType)rcgIndex switch
                    {
                        ResourceChangeType.COST => "cost",
                        ResourceChangeType.RESULT => "result",
                        ResourceChangeType.RUN => "run",
                        ResourceChangeType.EFFECT => "effect",
                        _ => null,
                    };
                    swl.Text.SetTextRaw(textKey);
                    bwe.ExpandTargets.Add(swl.LayoutChild.GameObject);
                }
                foreach (var changeU in arrayOfChanges)
                {
                    TripleTextView ttv = CanvasMaker.CreateTripleTextView(ButtonObjectRequest);
                    layout.AddLayoutChildAndParentIt(ttv.LayoutChild);
                    tcu.ChangeGroups[rcgIndex].tripleTextViews.Add(ttv);
                    bwe.ExpandTargets.Add(ttv.LayoutChild.RectTransform.gameObject);
                }
            }

            void AddToExpands(LayoutChild c) 
            {
                tcu.bwe.ExpandTargets.Add(c.GameObject);
                layout.AddLayoutChildAndParentIt(c);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        arcaniaModel.ManualUpdate(Time.deltaTime);
        dynamicCanvas.ManualUpdate();

        foreach (var tcu in TaskControls)
        {
            var data = tcu.Data;
            tcu.ManualUpdate();
            bool visible = data.Visible;
            tcu.bwe.LayoutChild.RectTransform.parent.gameObject.SetActive(visible);
            if (!visible) continue;
            tcu.bwe.MainButton.ButtonEnabled = arcaniaModel.Runner.CanStartAction(data);
            
            if (tcu.TaskClicked) 
            {
                arcaniaModel.Runner.StartAction(data);
            }
        }
    }
}
