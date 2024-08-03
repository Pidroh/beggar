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
    public ArcaniaModel arcaniaModel = new();

    // Start is called before the first frame update
    void Start()
    {
        var arcaniaDatas = arcaniaModel.arcaniaUnits;
        JsonReader.ReadJson(ResourceJson.text, arcaniaDatas);
        dynamicCanvas = CanvasMaker.CreateCanvas(1);
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
            
            
            for (int i = 0; i < 4; i++)
            {
                var arrayOfChanges = item.ConfigTask.GetResourceChangeList(i);
                var rcgIndex = i;
                if (i == 0) 
                {
                    var text = CanvasMaker.CreateTextUnit(ButtonObjectRequest.SecondaryColor, ButtonObjectRequest.font);
                    var image = CanvasMaker.CreateSimpleImage(ButtonObjectRequest.SecondaryColor);
                    var swl = new SeparatorWithLabel(text, image);
                    // layout.AddLayoutChildAndParentIt(swl.LayoutChild);
                    tcu.Add(swl);
                    swl.Text.SetTextRaw("Cost");
                }
                if (arrayOfChanges != null) tcu.ChangeGroups[rcgIndex] = new TaskControlUnit.ResourceChangeGroup();
                foreach (var changeU in arrayOfChanges)
                {
                    TripleTextView ttv = CanvasMaker.CreateTripleTextView(ButtonObjectRequest);
                    layout.AddLayoutChildAndParentIt(ttv.LayoutChild);
                    tcu.ChangeGroups[rcgIndex].tripleTextViews.Add(ttv);
                }
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
