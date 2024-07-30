using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class JsonReader
{

    public static void ReadJson(string json, ArcaniaUnits arcaniaDatas)
    {
        var parentNode = SimpleJSON.JSON.Parse(json);
        int currentModAmount = arcaniaDatas.Mods.Count;
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
        for (int i = currentModAmount; i < arcaniaDatas.Mods.Count; i++)
        {
            arcaniaDatas.Mods[i].Source.RegisterModTargetingSelf(arcaniaDatas.Mods[i]);
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
            ReadBasicUnit(ru, item, arcaniaUnits);
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

    private static void ReadMods(RuntimeUnit owner, SimpleJSON.JSONNode dataJsonMod, ArcaniaUnits arcaniaUnits)
    {
        using var _1 = ListPool<string>.Get(out var strList);

        foreach (var pair in dataJsonMod)
        {
            strList.Clear();
            var key = pair.Key;
            var value = pair.Value.AsFloat;
            var splittedValues = key.Split('.');
            var last = splittedValues[splittedValues.Length - 1];
            var targetId = splittedValues[splittedValues.Length - 2];
            var md = new ModData();
            md.Source = owner;
            md.Target = arcaniaUnits.GetOrCreateIdPointer(targetId);
            md.ModType = last == "max" ? ModType.MaxChange : ModType.RateChange;
            md.Value = value;
            arcaniaUnits.Mods.Add(md);

        }
    }

    private static ConfigBasic ReadBasicUnit(RuntimeUnit ru, SimpleJSON.JSONNode item, ArcaniaUnits arcaniaUnits)
    {
        string id = item["id"];
        string desc = item.GetValueOrDefault("desc", null);
        int max = item.GetValueOrDefault("max", -1);
        var bu = new ConfigBasic();
        bu.Id = id;
        bu.Desc = desc;
        bu.Max = max;
        foreach (var pair in item)
        {
            if (pair.Key == "name") bu.name = pair.Value;
            if (pair.Key == "mod") ReadMods(owner:ru, dataJsonMod:pair.Value, arcaniaUnits);
        }
        bu.name = bu.name == null ? char.ToUpper(id[0]) + id.Substring(1) : bu.name;
        ru.ConfigBasic = bu;
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

public enum ModType
{
    MaxChange, RateChange
}

public class ModData
{
    public ModType ModType;
    public float Value;
    public RuntimeUnit Source;
    public IDPointer Intermediary;
    public IDPointer Target;
}

public class RuntimeUnit
{
    public ConfigBasic ConfigBasic;
    public ConfigTask ConfigTask;
    public List<ModData> ModsTargetingSelf = new();

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

    internal bool CanFullyAcceptChange(int valueChange)
    {
        if (valueChange < 0) return Mathf.Abs(valueChange) <= Value;
        // no max
        if (Max < 0) return true;
        if (valueChange > 0)
        {
            if (Max == 0) return false;
            int ValueToReachMax = Max - Value;
            return ValueToReachMax >= valueChange;
        }
        return true;
    }

    internal void RegisterModTargetingSelf(ModData modData)
    {
        ModsTargetingSelf.Add(modData);
    }

    public int Value { get; internal set; } = 0;
    public int MaxForCeiling => Max < 0 ? int.MaxValue : Max;

    public float TaskProgress { get; internal set; }
    public bool IsMaxed => Value >= Max;

    public bool IsZero => Value == 0;
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
