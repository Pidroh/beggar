using arcania;
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
            if (arcaniaDatas.Mods[i].Target.id == "space")
            {
                arcaniaDatas.SpaceMods.Add(arcaniaDatas.Mods[i]);
                continue;
            }
            if (arcaniaDatas.Mods[i].Target.Tag != null)
            {
                foreach (var item in arcaniaDatas.Mods[i].Target.Tag.UnitsWithTag)
                {
                    item.RegisterModTargetingSelf(arcaniaDatas.Mods[i]);
                }
                return;
            }
            if (arcaniaDatas.Mods[i].Target.RuntimeUnit == null) Debug.Log($"Target not found {arcaniaDatas.Mods[i].Target.id}");
            arcaniaDatas.Mods[i].Target.RuntimeUnit.RegisterModTargetingSelf(arcaniaDatas.Mods[i]);
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
            ModType modType;
            string targetId = null;
            if (last == "space")
            {
                modType = ModType.SpaceConsumption;
            }
            else
            {
                targetId = splittedValues[splittedValues.Length - 2];
                modType = last == "max" ? ModType.MaxChange : ModType.RateChange;
            }
            CreateMod(owner, arcaniaUnits, value, modType, targetId);
        }
    }

    private static void CreateMod(RuntimeUnit owner, ArcaniaUnits arcaniaUnits, float value, ModType modType, string targetId)
    {
        var md = new ModRuntime();
        md.Source = owner;
        md.Value = value;
        md.ModType = modType;
        md.Target = targetId == null ? null : arcaniaUnits.GetOrCreateIdPointer(targetId);
        arcaniaUnits.Mods.Add(md);
    }

    private static ConfigBasic ReadBasicUnit(RuntimeUnit ru, SimpleJSON.JSONNode item, ArcaniaUnits arcaniaUnits)
    {
        string id = item["id"];
        string desc = item.GetValueOrDefault("desc", null);
        int max = item.GetValueOrDefault("max", -1);
        var bu = new ConfigBasic();
        ru.ConfigBasic = bu;
        bu.Id = id;
        bu.Desc = desc;
        bu.Max = max;
        foreach (var pair in item)
        {
            if (pair.Key == "name") bu.name = pair.Value;
            if (pair.Key == "mod") ReadMods(owner: ru, dataJsonMod: pair.Value, arcaniaUnits);
            if (pair.Key == "require") ru.ConfigBasic.Require = ConditionalExpressionParser.Parse(pair.Value.ToString(), arcaniaUnits);
            if (pair.Key == "tag") ReadTags(tags: ru.ConfigBasic.Tags, pair.Value.ToString(), arcaniaUnits);
            if (pair.Key == "lock") 
            {
                CreateMod(ru, arcaniaUnits, 1, ModType.Lock, pair.Value.ToString());
            }
        }
        foreach (var tag in ru.ConfigBasic.Tags)
        {
            tag.Tag.UnitsWithTag.Add(ru);
        }
        bu.name = bu.name == null ? char.ToUpper(id[0]) + id.Substring(1) : bu.name;
        
        return bu;
    }

    private static void ReadTags(List<IDPointer> tags, string v, ArcaniaUnits data)
    {
        if (v.Contains(","))
        {
            var tagSs = v.Split(',');
            foreach (var t in tagSs)
            {
                tags.Add(data.GetOrCreateIdPointerWithTag(t));
            }
            return;
        }
        tags.Add(data.GetOrCreateIdPointerWithTag(v));
    }
}

public class ConfigBasic
{
    public string Id;
    public string Desc;
    public int Max;
    public string name;

    public ConditionalExpression Require { get; internal set; }
    public List<IDPointer> Tags { get; } = new();
}

public enum UnitType
{
    RESOURCE, TASK, HOUSE, CLASS, SKILL, FURNITURE
}

public enum ModType
{
    MaxChange, 
    RateChange,
    SpaceConsumption,
    Lock
}

public class ModRuntime
{
    public ModType ModType;
    public float Value;
    public RuntimeUnit Source;
    public IDPointer Intermediary;
    public IDPointer Target;
}

public class ResourceChange
{
    public IDPointer IdPointer;
    public int valueChange;
}

public class TagRuntime
{
    public string tagName;
    public List<RuntimeUnit> UnitsWithTag = new();

    public TagRuntime(string tagName)
    {
        this.tagName = tagName;
    }
}
