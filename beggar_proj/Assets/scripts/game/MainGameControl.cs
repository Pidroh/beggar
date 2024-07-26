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

    // Start is called before the first frame update
    void Start()
    {
        ArcaniaUnits arcaniaDatas = new ArcaniaUnits();
        JsonReader.ReadJson(ResourceJson.text, arcaniaDatas);
        dynamicCanvas = CanvasMaker.CreateCanvas(2);
        foreach (var item in arcaniaDatas.datas[UnitType.TASK])
        {
            
            var button = CanvasMaker.CreateButton(item.ConfigBasic.name, Font);
            var iconButton = CanvasMaker.CreateButtonWithIcon(ExpanderSprite);
            var bwe = new ButtonWithExpandable(button, iconButton);
            dynamicCanvas.children[0].AddLayoutChildAndParentIt(bwe.LayoutChild);
            button.SetTextRaw(item.ConfigBasic.name);
            ButtonsWithExpandables.Add(bwe);
        }
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
