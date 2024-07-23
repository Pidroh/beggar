using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonReader
{
    public class ArcaniaDatas 
    { 

    }
    public static void ReadJson(string json, ArcaniaDatas arcaniaDatas) 
    {
        var parentNode = SimpleJSON.JSON.Parse(json);
        var items = parentNode["items"];
        string type = parentNode["type"];
        var isResource = type == "resource";
        foreach (var item in items.AsArray.Children)
        {
            SimpleJSON.JSONNode id = item["id"];
            Debug.Log(id);

        }

    }
}
