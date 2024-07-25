using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameControl : MonoBehaviour
{

    public TextAsset ResourceJson;
    private DynamicCanvas dynamicCanvas;

    // Start is called before the first frame update
    void Start()
    {
        JsonReader.ReadJson(ResourceJson.text, new JsonReader.ArcaniaUnits());
        dynamicCanvas = CanvasMaker.CreateCanvas();
    }

    // Update is called once per frame
    void Update()
    {
        dynamicCanvas.ManualUpdate();
    }
}
