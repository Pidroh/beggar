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

    public enum UnitType 
    { 
        RESOURCE, ACTION, 
    }

    public class TaskUnit {
        public BasicUnit basicUnit;
    }

    public class RuntimeUnit 
    {
        public BasicUnit BasicUnit;
        public TaskUnit TaskUnit;
    }

    public class ResourceChange 
    {
        public IDPointer IdPointer;
        public int valueChange;
    }

    public struct IDPointer 
    {
        public RuntimeUnit RuntimeUnit;
        public string id;
    }

    public class ArcaniaDatas
    {
        public Dictionary<UnitType, List<BasicUnit>> datas = new();
        //public List<BasicUnit> resources = new();
    }
    public static void ReadJson(string json, ArcaniaDatas arcaniaDatas)
    {
        var parentNode = SimpleJSON.JSON.Parse(json);
        if (parentNode.IsArray)
        {
            foreach (var c in parentNode.Children)
            {
                ReadArrayOwner(arcaniaDatas, c);
            }
        }
        else
        {
            ReadArrayOwner(arcaniaDatas, parentNode);
        }


    }

    private static void ReadArrayOwner(ArcaniaDatas arcaniaDatas, SimpleJSON.JSONNode parentNode)
    {
        var items = parentNode["items"];
        string typeS = parentNode["type"];
        if (!EnumHelper<UnitType>.TryGetEnumFromName(typeS, out var type)) Debug.LogError($"{typeS} not found in UnitType");
        if (!arcaniaDatas.datas.ContainsKey(type)) arcaniaDatas.datas[type] = new();
        foreach (var item in items.AsArray.Children)
        {
            BasicUnit basicInfo = ReadBasicUnit(item);
            arcaniaDatas.datas[type].Add(basicInfo);
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
