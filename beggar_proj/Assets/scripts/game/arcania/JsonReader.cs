using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonReader
{
    public class BasicUnit
    {
        public string Id;
        public string Desc;
        public int Max;
        public string name;
    }
    public class ArcaniaDatas 
    {
        public List<BasicUnit> resources = new();
    }
    public static void ReadJson(string json, ArcaniaDatas arcaniaDatas) 
    {
        var parentNode = SimpleJSON.JSON.Parse(json);
        var items = parentNode["items"];
        string type = parentNode["type"];
        var isResource = type == "resource";
        foreach (var item in items.AsArray.Children)
        {
            BasicUnit basicInfo = ReadBasicUnit(item);
            arcaniaDatas.resources.Add(basicInfo);
            SimpleJSON.JSONNode id = item["id"];
            Debug.Log(id);

        }

    }

    private static BasicUnit ReadBasicUnit(SimpleJSON.JSONNode item)
    {
        string id = item["id"];
        string desc = item.GetValueOrDefault("desc", null);
        int max = item.GetValueOrDefault("max", -1);
        var bu = new BasicUnit();
        bu.Id = id;
        bu.Desc = desc;
        bu.Max = max;
        if (item.HasKey("name"))
        {
            bu.name = item["name"];
        }
        else 
        {
            bu.name = char.ToUpper(id[0]) + id.Substring(1);
        }
        return bu;
    }
}
