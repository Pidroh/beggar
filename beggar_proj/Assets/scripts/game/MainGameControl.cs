using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainGameControl : MonoBehaviour
{

    public TextAsset ResourceJson;
    private DynamicCanvas dynamicCanvas;
    public List<ButtonWithExpandable> ButtonsWithExpandables = new();
    [SerializeField]
    public TMP_FontAsset Font;
    public Sprite ExpanderSprite;
    public CanvasMaker.CreateObjectRequest ButtonObjectRequest;

    // Start is called before the first frame update
    void Start()
    {
        ArcaniaUnits arcaniaDatas = new ArcaniaUnits();
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
            ButtonsWithExpandables.Add(bwe);
            var arrayOfChanges = item.ConfigTask.Cost;
            foreach (var cost in arrayOfChanges)
            {
                TripleTextView ttv = CanvasMaker.CreateTripleTextView(ButtonObjectRequest);
                layout.AddLayoutChildAndParentIt(ttv.LayoutChild);
            }
        }
    }

    public class TaskControlUnit { 

    }

    // Update is called once per frame
    void Update()
    {
        dynamicCanvas.ManualUpdate();
        foreach (var bwe in ButtonsWithExpandables)
        {
            bwe.ManualUpdate();
        }
    }
}
