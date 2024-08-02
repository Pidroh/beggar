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
            ModRuntime mod = arcaniaDatas.Mods[i];
            if (mod.Target.id == "space")
            {
                arcaniaDatas.SpaceMods.Add(mod);
                mod.Source.ConfigHouse.AvailableSpace = Mathf.FloorToInt(mod.Value);
                continue;
            }
            if (mod.ModType == ModType.SpaceConsumption) 
            {
                mod.Source.ConfigFurniture.SpaceConsumed = Mathf.FloorToInt(mod.Value);
                continue;
            }
            if (mod.Target.Tag != null)
            {
                foreach (var item in mod.Target.Tag.UnitsWithTag)
                {
                    item.RegisterModTargetingSelf(mod);
                }
                return;
            }
            if (mod.Target.RuntimeUnit == null) Debug.Log($"Target not found {mod.Target.id}");
            mod.Target.RuntimeUnit.RegisterModTargetingSelf(mod);
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
            ReadBasicUnit(ru, item, arcaniaUnits, type);
            arcaniaUnits.GetOrCreateIdPointer(ru.ConfigBasic.Id).RuntimeUnit = ru;
            if (type == UnitType.TASK)
            {
                ru.ConfigTask = ReadTask(item, arcaniaUnits);
            }
            if (type == UnitType.CLASS)
            {
                ru.ConfigTask = ReadTask(item, arcaniaUnits);
            }
            if (type == UnitType.SKILL)
            {
                ru.ConfigTask = ReadTask(item, arcaniaUnits);
                new SkillRuntime(ru);
            }
            if (type == UnitType.HOUSE)
            {
                ru.ConfigTask = ReadTask(item, arcaniaUnits);
                ru.ConfigHouse = new ConfigHouse();
            }
            if (type == UnitType.FURNITURE)
            {
                ru.ConfigTask = ReadTask(item, arcaniaUnits);
                ru.ConfigFurniture = new ConfigFurniture();
                var repeat = item.GetValueOrDefault("repeat", null);
                // if has repeat, then having no max is fine
                if (repeat != null && repeat.AsBool)
                {
                    ru.ConfigBasic.Max = -1;
                }
                else 
                {
                    // any furniture which is NOT repeat and does not have an explicit max has a default value of 1
                    // this is different from tasks, which have a default value of -1 (no max)
                    if (!ru.HasMax) ru.ConfigBasic.Max = 1;
                }
            }

            arcaniaUnits.datas[type].Add(ru);
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
            if (pair.Key == "run") ReadChanges(ct.Run, pair.Value, arcaniaUnits, -1);
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
            ModType modType = ModType.Invalid;
            string target = null;
            string secondary = null;
            ResourceChangeType? changeType = null;
            
            if (last == "space")
            {
                modType = ModType.SpaceConsumption;
            }
            else
            {
                var oneBeforeLast = splittedValues[splittedValues.Length - 2];
                
                if (last == "max") modType = ModType.MaxChange;
                if (last == "rate") modType = ModType.RateChange;
                // if still undecided
                if (modType == ModType.Invalid)
                {
                    // TODO make this be an dictionary between string and ResourceChangeType, so you can handle every case without hard coding
                    if (oneBeforeLast == "effect") {
                        target = last;
                        modType = ModType.ResourceChangeChanger;
                        changeType = ResourceChangeType.EFFECT;
                        secondary = splittedValues[splittedValues.Length - 3];
                    }
                }
                else 
                {
                    target = oneBeforeLast;
                }
            }
            var mod = CreateMod(owner, arcaniaUnits, value, modType, target, secondaryId: secondary);
            mod.ResourceChangeType = changeType;
            
        }
    }

    private static ModRuntime CreateMod(RuntimeUnit owner, ArcaniaUnits arcaniaUnits, float value, ModType modType, string targetId, string secondaryId)
    {
        var md = new ModRuntime
        {
            Source = owner,
            Value = value,
            ModType = modType,
            Target = targetId == null ? null : arcaniaUnits.GetOrCreateIdPointer(targetId),
            Intermediary = secondaryId == null ? null : arcaniaUnits.GetOrCreateIdPointer(secondaryId)
        };
        arcaniaUnits.Mods.Add(md);
        return md;
    }

    private static ConfigBasic ReadBasicUnit(RuntimeUnit ru, SimpleJSON.JSONNode item, ArcaniaUnits arcaniaUnits, UnitType type)
    {
        string id = item["id"];
        string desc = item.GetValueOrDefault("desc", null);
        int max = item.GetValueOrDefault("max", -1);
        var bu = new ConfigBasic();
        ru.ConfigBasic = bu;
        bu.Id = id;
        bu.Desc = desc;
        bu.Max = max;
        bu.UnitType = type;
        foreach (var pair in item)
        {
            if (pair.Key == "initial") ru.SetValue(pair.Value.AsInt);
            if (pair.Key == "name") bu.name = pair.Value;
            if (pair.Key == "mod") ReadMods(owner: ru, dataJsonMod: pair.Value, arcaniaUnits);
            if (pair.Key == "require") ru.ConfigBasic.Require = ConditionalExpressionParser.Parse(pair.Value.ToString(), arcaniaUnits);
            if (pair.Key == "tag") ReadTags(tags: ru.ConfigBasic.Tags, pair.Value.ToString(), arcaniaUnits);
            if (pair.Key == "tags") ReadTags(tags: ru.ConfigBasic.Tags, pair.Value.ToString(), arcaniaUnits);
            if (pair.Key == "lock") 
            {
                CreateMod(ru, arcaniaUnits, 1, ModType.Lock, pair.Value.ToString(), null);
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
        // handler for multiple tags in the string
        if (v.Contains(","))
        {
            var tagSs = v.Split(',');
            foreach (var t in tagSs)
            {
                tags.Add(data.GetOrCreateIdPointerWithTag(t));
            }
        }
        else {
            // if no commas, that means it's a single tag that can be added directly
            tags.Add(data.GetOrCreateIdPointerWithTag(v));
        }
        
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
    public UnitType UnitType { get; internal set; }
}

public enum UnitType
{
    RESOURCE, TASK, HOUSE, CLASS, SKILL, FURNITURE
}

public enum ModType
{
    Invalid,
    MaxChange, 
    RateChange,
    SpaceConsumption,
    Lock,
    ResourceChangeChanger
}

public class ModRuntime
{
    public ModType ModType;
    public float Value;
    public RuntimeUnit Source;
    public IDPointer Intermediary;
    public IDPointer Target;

    public ResourceChangeType? ResourceChangeType { get; internal set; }
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
