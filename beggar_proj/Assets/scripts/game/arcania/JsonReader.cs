using HeartUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonReader
{

    public static void ReadJson(string json, ArcaniaUnits arcaniaDatas)
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

    private static void ReadArrayOwner(ArcaniaUnits arcaniaUnits, SimpleJSON.JSONNode parentNode)
    {
        var items = parentNode["items"];
        string typeS = parentNode["type"];
        if (!EnumHelper<UnitType>.TryGetEnumFromName(typeS, out var type)) Debug.LogError($"{typeS} not found in UnitType");
        if (!arcaniaUnits.datas.ContainsKey(type)) arcaniaUnits.datas[type] = new();
        foreach (var item in items.AsArray.Children)
        {
            var ru = new RuntimeUnit();
            ru.ConfigBasic = ReadBasicUnit(item, arcaniaUnits);
            arcaniaUnits.GetOrCreateIdPointer(ru.ConfigBasic.Id).RuntimeUnit = ru;
            if (type == UnitType.TASK) 
            {
                ru.ConfigTask = ReadTask(item, arcaniaUnits);
            }
            
            arcaniaUnits.datas[type].Add(ru);
            SimpleJSON.JSONNode id = item["id"];
            Debug.Log(id);
        }
    }

    private static ConfigTask ReadTask(SimpleJSON.JSONNode item, ArcaniaUnits arcaniaUnits)
    {
        var ct = new ConfigTask();
        foreach (var pair in item)
        {
            if (pair.Key == "cost") ReadChanges(ct.Cost, pair.Value, arcaniaUnits, -1);
            if (pair.Key == "result") ReadChanges(ct.Result, pair.Value, arcaniaUnits, 1);
            if (pair.Key == "effect") ReadChanges(ct.Effect, pair.Value, arcaniaUnits, 1);
            if (pair.Key == "perpetual") ct.Perpetual = pair.Value.AsBool;
        }
        return ct;
    }

    private static void ReadChanges(List<ResourceChange> list, SimpleJSON.JSONNode value, ArcaniaUnits arcaniaUnits, int signalMultiplier)
    {
        foreach (var c in value)
        {
            var rc = new ResourceChange() { 
                IdPointer = arcaniaUnits.GetOrCreateIdPointer(c.Key),
                valueChange = c.Value.AsInt * signalMultiplier
            };
            list.Add(rc);
        }
    }

    private static ConfigBasic ReadBasicUnit(SimpleJSON.JSONNode item, ArcaniaUnits arcaniaUnits)
    {
        string id = item["id"];
        string desc = item.GetValueOrDefault("desc", null);
        int max = item.GetValueOrDefault("max", -1);
        var bu = new ConfigBasic();
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

public class ConfigBasic
{
    public string Id;
    public string Desc;
    public int Max;
    public string name;
}

public enum UnitType
{
    RESOURCE, TASK,
}

public class ConfigTask
{
    public const int RESOURCE_CHANGE_LIST_COST = 0;
    public const int RESOURCE_CHANGE_LIST_RESULT = 1;
    public const int RESOURCE_CHANGE_LIST_RUN = 2;
    public const int RESOURCE_CHANGE_LIST_EFFECT = 3;

    public List<ResourceChange> Cost => ResourceChangeLists[RESOURCE_CHANGE_LIST_COST];
    public List<ResourceChange> Result => ResourceChangeLists[RESOURCE_CHANGE_LIST_RESULT];
    public List<ResourceChange> Run => ResourceChangeLists[RESOURCE_CHANGE_LIST_RUN];
    public List<ResourceChange> Effect => ResourceChangeLists[RESOURCE_CHANGE_LIST_EFFECT];
    public AutoNewList<List<ResourceChange>> ResourceChangeLists = new AutoNewList<List<ResourceChange>>();

    public bool Perpetual { get; internal set; }

    internal List<ResourceChange> GetResourceChangeList(int i)
    {
        return ResourceChangeLists[i];
    }
}

public class RuntimeUnit
{
    public ConfigBasic ConfigBasic;
    public ConfigTask ConfigTask;

    public string Name => ConfigBasic.name;

    public int Max => CalculateMax();

    private int CalculateMax()
    {
        return ConfigBasic.Max;
    }

    public object Value { get; internal set; } = 0;
}

public class ResourceChange
{
    public IDPointer IdPointer;
    public int valueChange;
}

public class IDPointer
{
    public RuntimeUnit RuntimeUnit;
    public string id;
}

public class ArcaniaUnits
{
    public Dictionary<UnitType, List<RuntimeUnit>> datas = new();
    public Dictionary<string, IDPointer> IdMapper = new();

    internal IDPointer GetOrCreateIdPointer(string key)
    {
        if (!IdMapper.TryGetValue(key, out var value))
        {
            value = new IDPointer()
            {
                id = key
            };
            IdMapper[key] = value;
        }
        return value;
    }
    //public List<BasicUnit> resources = new();
}
