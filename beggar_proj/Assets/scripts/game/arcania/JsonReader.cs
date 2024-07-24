using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonReader
{
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

    public class ConfigTask {
        public List<ResourceChange> Cost = new();
        public List<ResourceChange> Result = new();
    }

    public class RuntimeUnit 
    {
        public ConfigBasic ConfigBasic;
        public ConfigTask ConfigTask;
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

    public class ArcaniaUnits
    {
        public Dictionary<UnitType, List<RuntimeUnit>> datas = new();
        public Dictionary<string, IDPointer> IdMapper = new();

        internal IDPointer GetOrCreateIdPointer(string key)
        {
            if (!IdMapper.TryGetValue(key, out var value)) 
            {
                value = new IDPointer() { 
                    id = key
                };
                IdMapper[key] = value;
            }
            return value;
        }
        //public List<BasicUnit> resources = new();
    }
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
            if (!arcaniaUnits.IdMapper.TryGetValue(ru.ConfigBasic.Id, out var pointer)) 
            {
                pointer = new IDPointer();
                arcaniaUnits.IdMapper[ru.ConfigBasic.Id] = pointer;
            }
            pointer.RuntimeUnit = ru;
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
            if (pair.Key == "result") ReadChanges(ct.Cost, pair.Value, arcaniaUnits, 1);
        }
        return ct;
    }

    private static void ReadChanges(List<ResourceChange> resourceChange, SimpleJSON.JSONNode value, ArcaniaUnits arcaniaUnits, int signalMultiplier)
    {
        foreach (var c in value)
        {
            var rc = new ResourceChange() { 
                IdPointer = arcaniaUnits.GetOrCreateIdPointer(c.Key),
                valueChange = c.Value.AsInt * signalMultiplier
            };
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
