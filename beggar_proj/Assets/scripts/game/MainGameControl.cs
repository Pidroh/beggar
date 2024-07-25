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
    private readonly TMP_FontAsset font;

    // Start is called before the first frame update
    void Start()
    {
        ArcaniaUnits arcaniaDatas = new ArcaniaUnits();
        JsonReader.ReadJson(ResourceJson.text, arcaniaDatas);
        dynamicCanvas = CanvasMaker.CreateCanvas(2);
        foreach (var item in arcaniaDatas.datas[UnitType.TASK])
        {
            var bwe = new ButtonWithExpandable();
            var button = CanvasMaker.CreateButton(item.ConfigBasic.name, font);
            button.gameObject.transform.SetParent(dynamicCanvas.children[0].transform);
            button.SetTextRaw(item.ConfigBasic.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        dynamicCanvas.ManualUpdate();
    }
}
