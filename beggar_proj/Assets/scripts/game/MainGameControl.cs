using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameControl : MonoBehaviour
{

    public TextAsset ResourceJson;

    // Start is called before the first frame update
    void Start()
    {
        JsonReader.ReadJson(ResourceJson.text, new JsonReader.ArcaniaDatas());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
