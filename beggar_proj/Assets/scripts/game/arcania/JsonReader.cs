using arcania;
using HeartUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class JsonReader
{

    public static void ReadJson(ArcaniaGameConfigurationUnit config, ArcaniaUnits arcaniaDatas)
    {
        int modAmountBeforeReadingData = arcaniaDatas.Mods.Count;
        var jsonDatas = config.jsonDatas;
        foreach (var item in jsonDatas)
        {
            var parentNode = SimpleJSON.JSON.Parse(item.text);
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


        //--------------------------------------------------------------
        // POST PROCESSING #post-processing
        //--------------------------------------------------------------
        // MODS #mods #post-processing
        //--------------------------------------------------------------
        #region mods post processing
        for (int i = modAmountBeforeReadingData; i < arcaniaDatas.Mods.Count; i++)
        {
            ModRuntime mod = arcaniaDatas.Mods[i];
            mod.Source.ModsOwned.Add(mod);
            //--------------------------------------------------------------
            // MODS human text
            //--------------------------------------------------------------
            string targetTextKey = GetPointerTextKey(mod.Target);
            string intermediaryTextKey = GetPointerTextKey(mod.Intermediary);
            string GetPointerTextKey(IDPointer pointer)
            {
                string textKey = null;
                textKey = pointer?.RuntimeUnit?.Name;
                if (textKey == null)
                {
                    textKey = pointer?.Tag?.tagName;
                }
                return textKey;
            }
            if (mod.ModType == ModType.MaxChange)
            {
                // space max increasing has no target
                if (targetTextKey != null)
                {
                    if (intermediaryTextKey != null)
                    {
                        mod.HumanText = $"{Local.GetText(intermediaryTextKey)} Mod Max {Local.GetText(targetTextKey)}:";
                    }
                    else
                    {
                        mod.HumanText = $"Max {Local.GetText(targetTextKey)}:";
                    }

                }
                else mod.HumanText = $"Max Space:";
            }
            if (mod.ModType == ModType.RateChange)
            {
                mod.HumanText = $"{Local.GetText(targetTextKey)} Rate:";
            }
            if (mod.ModType == ModType.ResourceChangeChanger)
            {
                if (mod.ResourceChangeType == ResourceChangeType.EFFECT)
                    mod.HumanText = $"{Local.GetText(targetTextKey)} {Local.GetText(intermediaryTextKey)}:";
                else mod.HumanText = "RESOURCE CHANGE TYPE NOT SUPPORTED YET";
            }
            if (mod.ModType == ModType.SpaceConsumption)
            {
                mod.HumanText = "Space Occupied:";
            }
            if (mod.ModType == ModType.Lock)
            {
                mod.HumanText = "Currently Invisible";
            }
            if (mod.HumanText == null)
            {
                Debug.Log("Human text logic not implemented (use this for break points)");
            }
            mod.HumanText = mod.HumanText == null ? "HUMAN TEXT NEEDS TO BE IMPLEMENTED" : mod.HumanText;
            //--------------------------------------------------------------
            // MODS human text END
            //--------------------------------------------------------------
            if (mod.ModType == ModType.SpaceConsumption)
            {
                mod.Source.ConfigFurniture.SpaceConsumed = Mathf.FloorToInt(mod.Value);
                continue;
            }
            if (mod.Target == null)
            {
                Debug.Log($"No target for mod from {mod.Source.ConfigBasic.Id} {mod.ModType} {EnumHelper<ModType>.GetName(mod.ModType)}");
                continue;
            }
            if (mod.Target.id == "space")
            {
                arcaniaDatas.SpaceMods.Add(mod);
                mod.Source.ConfigHouse.AvailableSpace = Mathf.FloorToInt(mod.Value);
                continue;
            }

            if (mod.Target.Tag != null)
            {
                foreach (var item in mod.Target.Tag.UnitsWithTag)
                {
                    item.RegisterModTargetingSelf(mod);
                }
                continue;
            }
            if (mod.Target.RuntimeUnit == null)
            {
                Debug.Log($"Target not found {mod.Target.id}");
            }

            mod.Target.RuntimeUnit.RegisterModTargetingSelf(mod);
        }
        #endregion
        //--------------------------------------------------------------
        // Conditions #conditions #post-processing
        //--------------------------------------------------------------
        foreach (var item in arcaniaDatas.datas)
        {
            foreach (var u in item.Value)
            {
                if (u.ConfigTask?.Need == null) continue;
                u.ConfigTask.Need.humanExpression = ConditionalExpressionParser.ToHumanLanguage(u.ConfigTask.Need.expression);
            }
        }
        #region check broken pointers
        foreach (var item in arcaniaDatas.IdMapper.Values)
        {
            item.CheckValidity();
        }
        #endregion
    }

    private static void ReadArrayOwner(ArcaniaUnits arcaniaUnits, SimpleJSON.JSONNode parentNode)
    {
        var items = parentNode["items"];
        string typeS = parentNode["type"];
        if (!EnumHelper<UnitType>.TryGetEnumFromName(typeS, out var type)) Debug.LogError($"{typeS} not found in UnitType");
        if (type == UnitType.DIALOG)
        {
            foreach (var item in items.AsArray.Children)
            {
                var dr = new DialogRuntime()
                {
                    Title = item["content"],
                    Content = item["content"],
                    Id = item["id"],
                };
                foreach (var pair in item)
                {
                    if (pair.Key == "tag" || pair.Key == "tags") ReadTags(tags: dr.TagPointers, pair.Value.AsString, arcaniaUnits);
                }
                foreach (var tag in dr.TagPointers)
                {
                    tag.Tag.Dialogs.Add(dr);
                }
                arcaniaUnits.Dialogs.Add(dr);
            }
            // ReadDialog(arcaniaUnits, items.AsArray.Children);
            return;
        }
        if (!arcaniaUnits.datas.ContainsKey(type)) arcaniaUnits.datas[type] = new();
        foreach (var item in items.AsArray.Children)
        {
            var ru = new RuntimeUnit();
            ReadBasicUnit(ru, item, arcaniaUnits, type);

            IDPointer iDPointer = arcaniaUnits.GetOrCreateIdPointer(ru.ConfigBasic.Id);
            if (iDPointer.RuntimeUnit != null)
            {
                Debug.LogError($"Potential ID duplication: {iDPointer.id}");
            }
            iDPointer.RuntimeUnit = ru;
            if (type == UnitType.RESOURCE)
            {
                ru.ConfigResource = new ConfigResource()
                {
                    Stressor = item.GetValueOrDefault("stressor", false)
                };
            }
            if (type == UnitType.TAB)
            {
                var tr = new TabRuntime(ru);
                List<UnitType> acceptedUnitTypes = ru.Tab.AcceptedUnitTypes;
                var key = "unit_types";
                ReadUnitTypesToArray(item, acceptedUnitTypes, key);
                foreach (var pair in item)
                {
                    if (pair.Key == "exploration_active_tab") tr.ExplorationActiveTab = pair.Value.AsBool;
                    if (pair.Key == "contains_logs") tr.ContainsLogs = pair.Value.AsBool;
                    if (pair.Key == "open_settings") tr.OpenSettings = pair.Value.AsBool;
                }

                SimpleJSON.JSONNode separatorNode = item.GetValueOrDefault("separator", null);
                if (separatorNode != null)
                {
                    var children = separatorNode.AsArray.Children;
                    foreach (var c in children)
                    {
                        var sep = new TabRuntime.Separator();
                        ru.Tab.Separators.Add(sep);
                        ReadUnitTypesToArray(c, sep.AcceptedUnitTypes, key);
                        foreach (var pair in c)
                        {
                            if (pair.Key == "name") sep.Name = pair.Value.AsString;
                            if (pair.Key == "default") sep.Default = pair.Value.AsBool;
                            if (pair.Key == "require_max") sep.RequireMax = pair.Value.AsBool;
                            if (pair.Key == "show_space") sep.ShowSpace = pair.Value.AsBool;
                            if (pair.Key == "require_instant") sep.RequireInstant = pair.Value.AsBool;
                        }
                    }
                }

            }
            if (type == UnitType.ENCOUNTER)
            {
                ru.ConfigTask = ReadTask(ru, item, arcaniaUnits);
                ru.ConfigEncounter = new ConfigEncounter()
                {
                    Length = item.GetValueOrDefault("length", 5)
                };
            }
            if (type == UnitType.TASK)
            {
                ru.ConfigTask = ReadTask(ru, item, arcaniaUnits);
                if (ru.ConfigTask.SlotKey == "rest")
                {
                    arcaniaUnits.RestActionActive = ru;
                }
            }
            if (type == UnitType.LOCATION)
            {
                ru.ConfigTask = ReadTask(ru, item, arcaniaUnits);
                var cl = new ConfigLocation()
                {
                    Length = item.GetValueOrDefault("length", 10)
                };
                var lr = new LocationRuntime(ru, cl);
                var encounterIds = item["encs"].AsArray;
                foreach (var eId in encounterIds)
                {
                    var id = eId.Value.AsString;
                    lr.Encounters.Add(arcaniaUnits.GetOrCreateIdPointer(id));
                }
            }
            if (type == UnitType.CLASS)
            {
                ru.ConfigTask = ReadTask(ru, item, arcaniaUnits);
                ru.ConfigBasic.Max = 1;
            }
            if (type == UnitType.SKILL)
            {
                ru.ConfigTask = ReadTask(ru, item, arcaniaUnits);
                if (ru.ConfigBasic.Max < 0) ru.ConfigBasic.Max = 3;
                new SkillRuntime(ru);
            }
            if (type == UnitType.HOUSE)
            {
                ru.ConfigTask = ReadTask(ru, item, arcaniaUnits);
                ru.ConfigHouse = new ConfigHouse();
            }
            if (type == UnitType.FURNITURE)
            {
                ru.ConfigTask = ReadTask(ru, item, arcaniaUnits);
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

        static void ReadUnitTypesToArray(SimpleJSON.JSONNode item, List<UnitType> acceptedUnitTypes, string key)
        {
            SimpleJSON.JSONNode jSONNode = item.GetValueOrDefault(key, null);
            if (jSONNode == null) return;
            var unitTypeLabels = jSONNode.AsArray.Children;

            foreach (var labels in unitTypeLabels)
            {
                if (!EnumHelper<UnitType>.TryGetEnumFromName(labels, out var typeFilter)) Debug.LogError($"{labels} not found in UnitType");
                acceptedUnitTypes.Add(typeFilter);
            }
        }
    }

    private static ConfigTask ReadTask(RuntimeUnit ru, SimpleJSON.JSONNode item, ArcaniaUnits arcaniaUnits)
    {
        var ct = new ConfigTask();
        var explicitPerpetualDefinition = false;
        foreach (var pair in item)
        {
            if (pair.Key == "cost") ReadChanges(ct.Cost, pair.Value, arcaniaUnits, -1);
            if (pair.Key == "result") ReadChanges(ct.Result, pair.Value, arcaniaUnits, 1);
            if (pair.Key == "result_once") ReadChanges(ct.ResultOnce, pair.Value, arcaniaUnits, 1);
            if (pair.Key == "effect") ReadChanges(ct.Effect, pair.Value, arcaniaUnits, 1);
            if (pair.Key == "run") ReadChanges(ct.Run, pair.Value, arcaniaUnits, -1);
            if (pair.Key == "perpetual") ct.Perpetual = pair.Value.AsBool;
            if (pair.Key == "perpetual") explicitPerpetualDefinition = true;
            if (pair.Key == "duration") ct.Duration = pair.Value.AsInt;
            if (pair.Key == "slot") ct.SlotKey = pair.Value.AsString;
            if (pair.Key == "need") ct.Need = ConditionalExpressionParser.Parse(pair.Value.AsString, arcaniaUnits);
        }
        if (!ct.Duration.HasValue)
        {
            if (ct.Effect.Count != 0 || ct.Run.Count != 0 || ct.Perpetual)
            {
                ct.Duration = 1;
            }
        }
        if (ct.Duration.HasValue && !ct.Perpetual && !explicitPerpetualDefinition && !ru.HasMax && ru.ConfigBasic.UnitType != UnitType.LOCATION)
        {
            ct.Perpetual = true;
        }
        return ct;
    }

    private static void ReadChanges(List<ResourceChange> list, SimpleJSON.JSONNode value, ArcaniaUnits arcaniaUnits, int signalMultiplier)
    {
        foreach (var c in value)
        {
            var min = 0f;
            var max = 0f;
            if (c.Value.IsNumber)
            {
                var number = c.Value.AsFloat;
                min = number;
                max = number;
            }
            else
            {
                if (c.Value.IsString)
                {
                    var values = c.Value.AsString.Split("~");
                    min = float.Parse(values[0]);
                    max = float.Parse(values[1]);
                }
            }

            string header = c.Key;
            var changeType = ResourceChange.ResourceChangeModificationType.NormalChange;
            if (header.Contains(".xp"))
            {
                header = header.Replace(".xp", "");
                changeType = ResourceChange.ResourceChangeModificationType.XpChange;
            }
            var rc = new ResourceChange()
            {
                IdPointer = arcaniaUnits.GetOrCreateIdPointer(header),
                valueChange = new FloatRange(min * signalMultiplier, max * signalMultiplier),
                ModificationType = changeType
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
            var key = pair.Key.Replace("\"", "");
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

                // EXAMPLES:
                //   crakedvase.mod.clarity.max
                if (splittedValues.Length == 4)
                {
                    secondary = splittedValues[splittedValues.Length - 4];
                }

                // if still undecided
                if (modType == ModType.Invalid)
                {
                    // TODO make this be an dictionary between string and ResourceChangeType, so you can handle every case without hard coding
                    // EXAMPLE: pleafocus.effect.supplication
                    if (oneBeforeLast == "effect")
                    {
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
            mod.SourceJsonKey = pair.Key;
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
            if (pair.Key == "mod" || pair.Key == "mods") ReadMods(owner: ru, dataJsonMod: pair.Value, arcaniaUnits);
            if (pair.Key == "require") ru.ConfigBasic.Require = ConditionalExpressionParser.Parse(pair.Value.AsString, arcaniaUnits);
            if (pair.Key == "tag" || pair.Key == "tags") ReadTags(tags: ru.ConfigBasic.Tags, pair.Value.AsString, arcaniaUnits);
            if (pair.Key == "lock")
            {
                CreateMod(ru, arcaniaUnits, 1, ModType.Lock, pair.Value.AsString, null);
            }
        }
        // default require for RESOURCE
        if (ru.ConfigBasic.UnitType == UnitType.RESOURCE && ru.ConfigBasic.Require == null)
        {
            ru.ConfigBasic.Require = ConditionalExpressionParser.Parse($"{ru.ConfigBasic.Id}>0", arcaniaUnits);
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
        else
        {
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
    RESOURCE, TASK, HOUSE, CLASS, SKILL, FURNITURE, TAB, DIALOG, LOCATION, ENCOUNTER,
}

public class ModRuntime
{
    public ModType ModType;
    public float Value;
    public RuntimeUnit Source;
    public IDPointer Intermediary;
    public IDPointer Target;

    public ResourceChangeType? ResourceChangeType { get; internal set; }
    public string SourceJsonKey { get; internal set; }
    public string HumanText { get; internal set; }
}

public class ResourceChange
{
    public IDPointer IdPointer;
    public FloatRange valueChange;
    public ResourceChangeModificationType ModificationType = ResourceChangeModificationType.NormalChange;
    public enum ResourceChangeModificationType
    {
        NormalChange,
        XpChange
    }
}



public class DialogRuntime
{
    public string Id;
    public string Title;
    public string Content;
    public List<IDPointer> TagPointers = new();
}

public class TagRuntime
{
    public string tagName;
    public List<RuntimeUnit> UnitsWithTag = new();

    public List<DialogRuntime> Dialogs = new();

    public TagRuntime(string tagName)
    {
        this.tagName = tagName;
    }
}
