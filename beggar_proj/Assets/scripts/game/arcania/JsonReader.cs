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
        if (!ct.Duration.HasValue)
        {
            if (ct.Effect.Count != 0 || ct.Run.Count != 0 || ct.Perpetual)
            {
                ct.Duration = 1;
            }
        }
        return ct;
    }

    private static void ReadChanges(List<ResourceChange> list, SimpleJSON.JSONNode value, ArcaniaUnits arcaniaUnits, int signalMultiplier)
    {
        foreach (var c in value)
        {
            var rc = new ResourceChange()
            {
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

    internal bool IsTaskComplete()
    {
        if (ConfigTask.Duration.HasValue) return TaskProgress > ConfigTask.Duration.Value;
        return false;
    }

    internal bool IsInstant() => !ConfigTask.Duration.HasValue;

    public int Value { get; internal set; } = 0;
    public int MaxForCeiling => Max <= 0 ? int.MaxValue : Max;

    public float TaskProgress { get; internal set; }
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
