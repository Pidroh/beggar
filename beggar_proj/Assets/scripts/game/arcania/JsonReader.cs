using arcania;
using HeartUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.Pool;

public class JsonReader
{
    private static readonly Dictionary<string, ResourceChangeType> DictionaryOfChanges = new()
    {
        { "result", ResourceChangeType.RESULT },
        { "effect", ResourceChangeType.EFFECT },
        { "cost", ResourceChangeType.COST },
        { "run", ResourceChangeType.RUN },
        { "result_fail", ResourceChangeType.RESULT_FAIL },
        { "buy", ResourceChangeType.BUY },
    };
    public static void ReadJsonAllAtOnce(ArcaniaGameConfigurationUnit config, ArcaniaModel arcaniaModel, bool localizeNameDescription)
    {
        JsonReaderState? state = null;
        while ((state?.readerState != JsonReaderState.JsonReaderStateMode.OVER)) 
        {
            state = ReadJsonStepByStep(config, arcaniaModel, localizeNameDescription, state);
        }
    }

    public struct JsonReaderState
    {
        public JsonReaderStateMode readerState;
        public int jsonIndex;
        internal int modAmountBeforeReadingData;

        public WorldType CurrentWorld { get; internal set; }

        public enum JsonReaderStateMode
        {
            READ_JSON,
            OVER,
        }
    }

    public static JsonReaderState ReadJsonStepByStep(ArcaniaGameConfigurationUnit config, ArcaniaModel arcaniaModel, bool localizeNameDescription, JsonReaderState? stateRef)
    {
        var arcaniaDatas = arcaniaModel.arcaniaUnits;
        JsonReaderState state;
        if (stateRef == null)
        {
            state = new();
            state.readerState = JsonReaderState.JsonReaderStateMode.READ_JSON;
            state.modAmountBeforeReadingData = arcaniaDatas.Mods.Count;
        }
        else
        {
            state = stateRef.Value;
        }
        if (arcaniaModel.SaveSlotOnlyMode) 
        {
            // read only the save slot only mode
            ReadJsonSingleFile(arcaniaDatas, localizeNameDescription, state, config.saveSlotOnlyJsonData);
            return FinishingUpState(arcaniaDatas, state);
        }
        switch (state.readerState)
        {
            case JsonReaderState.JsonReaderStateMode.READ_JSON:
                if (state.jsonIndex < config.jsonDatas.Count)
                {

                    state.CurrentWorld = WorldType.DEFAULT_CHARACTER;
                    state = ReadJsonSingleFile(arcaniaDatas, config.jsonDatas, localizeNameDescription, state, state.jsonIndex);
                }
                else
                {
                    // since normal json is already over
                    // (might be best to refactor the code to use a list of lists of json
                    var tweakedIndex = state.jsonIndex - config.jsonDatas.Count;
                    if (tweakedIndex < config.jsonDatasPrestigeWorld.Count)
                    {
                        state.CurrentWorld = WorldType.PRESTIGE_WORLD;
                        state = ReadJsonSingleFile(arcaniaDatas, config.jsonDatasPrestigeWorld, localizeNameDescription, state, tweakedIndex);
                    }
                    else
                    {
                        state = FinishingUpState(arcaniaDatas, state);
                    }
                }
                break;
            default:
                break;
        }

        return state;
    }

    private static JsonReaderState FinishingUpState(ArcaniaUnits arcaniaDatas, JsonReaderState state)
    {
        ModPostProcessing(arcaniaDatas, state.modAmountBeforeReadingData);
        ConditionProcessing(arcaniaDatas);
        BrokenPointerCheck(arcaniaDatas);
        state.readerState = JsonReaderState.JsonReaderStateMode.OVER;
        return state;
    }

    private static void BrokenPointerCheck(ArcaniaUnits arcaniaDatas)
    {
        #region check broken pointers
        foreach (var item in arcaniaDatas.IdMapper.Values)
        {
            item.CheckValidity();
        }
        #endregion
    }

    private static void ConditionProcessing(ArcaniaUnits arcaniaDatas)
    {
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
        foreach (var item in arcaniaDatas.PointersThatHaveHintsTargetingThem)
        {
            ConfigBasic configBasic = item.RuntimeUnit.ConfigBasic;
            if (configBasic.Require == null) continue;
            configBasic.Require.humanExpression = ConditionalExpressionParser.ToHumanLanguage(configBasic.Require.expression);
        }
        #region assign each runtime unit to a separator
        foreach (var dataList in arcaniaDatas.datas)
        {
            if (dataList.Key == UnitType.TAB) continue;
            if (dataList.Key == UnitType.ENCOUNTER) continue;
            if (dataList.Key == UnitType.DOT) continue;
            foreach (var item in dataList.Value)
            {
                bool added = false;
                // this code doesn't handle well having multiple tabs accepting the same unit type
                // EXAMPLE NON_SUPPORTED:
                // - Tab 1: Holy Resources
                // - Tab 2: Dark Resources
                foreach (var tabCandidates in arcaniaDatas.datas[UnitType.TAB])
                {
                    if (!tabCandidates.Tab.AcceptedUnitTypes.Contains(dataList.Key)) continue;
                    TabRuntime.Separator unitSeparator = null;
                    foreach (var separatorCandidate in tabCandidates.Tab.Separators)
                    {
                        // don't try to look at lower priority separators
                        if (unitSeparator != null && unitSeparator.Priority >= separatorCandidate.Priority) continue;
                        if (separatorCandidate.AcceptedUnitTypes.Count > 0 && !separatorCandidate.AcceptedUnitTypes.Contains(dataList.Key)) continue;

                        if (separatorCandidate.RequireMax && !item.HasMax) continue;
                        if (separatorCandidate.Tags != null && separatorCandidate.Tags.Count > 0)
                        {
                            var hasTag = false;
                            foreach (var tag in separatorCandidate.Tags)
                            {
                                if (tag.RuntimeUnits.Contains(item))
                                {
                                    hasTag = true;
                                    break;
                                }
                            }
                            if (!hasTag) continue;
                        }
                        if (separatorCandidate.RequireInstant && item.ConfigTask.Duration > 0) continue;
                        unitSeparator = separatorCandidate;
                    }
                    if (unitSeparator == null) continue;
                    unitSeparator.BoundRuntimeUnits.Add(item);
                    added = true;
                    break;
                }
                if (added) continue;
                if (!added)
                {
                    Debug.Log("NOT ADDED " + item.ConfigBasic.Id);
                }
            }
        }
        #endregion
    }

    private static void ModPostProcessing(ArcaniaUnits arcaniaDatas, int modAmountBeforeReadingData)
    {

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
            string sourceNameKey = mod.Source.Name;
            var SpeedLabel = ModReplaceKeys.SPEED;
            var LabelMaxSpace = ModReplaceKeys.MAXSPACE;
            var RateLabel = ModReplaceKeys.RATE;
            var LabelMax = ModReplaceKeys.MAX;
            var SuccessRateLabel = ModReplaceKeys.SUCCESSRATE;
            var spaceOccuppiedLabel = ModReplaceKeys.SPACEOCCUPIED;
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
            switch (mod.ModType)
            {
                case ModType.Speed:
                    mod.HumanText = $"{SpeedLabel} % {targetTextKey}:";
                    mod.HumanTextTarget = $"{SpeedLabel} % ({sourceNameKey}):";
                    break;

                case ModType.MaxChange:
                    if (targetTextKey != null)
                    {
                        if (intermediaryTextKey != null)
                        {
                            mod.HumanText = $"{intermediaryTextKey} Mod {LabelMax} {targetTextKey}:";
                            mod.HumanTextIntermediary = $" Mod {LabelMax} {targetTextKey} ({sourceNameKey})";
                            mod.HumanTextTarget = $"Max ({sourceNameKey} x {intermediaryTextKey}):";
                        }
                        else
                        {
                            mod.HumanText = $"{LabelMax} {targetTextKey}:";
                            mod.HumanTextTarget = $"{LabelMax} ({sourceNameKey}):";
                        }
                    }
                    else
                    {
                        mod.HumanText = $"{LabelMaxSpace}:";
                    }
                    break;

                case ModType.RateChange:
                    if (intermediaryTextKey != null)
                    {
                        mod.HumanText = $"{intermediaryTextKey} Mod {targetTextKey} {RateLabel}:";
                        mod.HumanTextIntermediary = $" Mod {RateLabel} {targetTextKey} ({sourceNameKey}):";
                        mod.HumanTextTarget = $"{RateLabel} ({sourceNameKey} x {intermediaryTextKey}):";
                    }
                    else
                    {
                        mod.HumanText = $"{targetTextKey} {RateLabel}:";
                        mod.HumanTextTarget = $"{RateLabel} ({sourceNameKey}):";
                    }

                    break;

                case ModType.ResourceChangeChanger:
                    if (mod.ResourceChangeType == ResourceChangeType.EFFECT || mod.ResourceChangeType == ResourceChangeType.RESULT)
                    {
                        mod.HumanText = $"{targetTextKey} {intermediaryTextKey}:";
                        mod.HumanTextIntermediary = $"{targetTextKey} ({sourceNameKey}):";
                        mod.HumanTextTarget = null;
                    }
                    else
                    {
                        mod.HumanText = "RESOURCE CHANGE TYPE NOT SUPPORTED YET";
                    }
                    break;

                case ModType.SpaceConsumption:

                    mod.HumanText = spaceOccuppiedLabel + ":";
                    break;

                case ModType.Lock:
                    mod.HumanText = "(Currently Invisible: error)";
                    break;
                case ModType.SuccessRate:

                    mod.HumanText = $"{targetTextKey} {SuccessRateLabel}:";
                    mod.HumanTextTarget = $"{SuccessRateLabel} ({sourceNameKey}):";
                    break;
                case ModType.Activate:
                    mod.HumanText = Local.GetText("Enables other tasks", "Describing a characteristing of a certain effect, in that it makes other tasks possible (enabled)");
                    mod.HumanTextTarget = Local.GetText($"Enabled by an effect", "describing something that is enabled by another effect") + $" ({sourceNameKey})";
                    break;
                default:
                    break;
            }
            if (mod.HumanText == null)
            {
                Debug.Log("Human text logic not implemented (use this for break points)");
            }
            mod.HumanText = mod.HumanText == null ? "HUMAN TEXT NEEDS TO BE IMPLEMENTED" : mod.HumanText;
            //--------------------------------------------------------------
            // MODS human text END
            //--------------------------------------------------------------
            //--------------------------------------------------------------
            // MODS misc
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

            if (mod.Intermediary != null)
            {
                foreach (var ru in mod.Intermediary.RuntimeUnits)
                {
                    ru.RegisterModWithSelfAsIntermediary(mod);
                }
            }

            foreach (var ru in mod.Target.RuntimeUnits)
            {
                ru.RegisterModTargetingSelf(mod);
                if (mod.ModType == ModType.Activate)
                {
                    ru.Activatable = true;
                }
            }
        }
        #endregion
    }

    private static JsonReaderState ReadJsonSingleFile(ArcaniaUnits arcaniaDatas, List<TextAsset> jsonDatas, bool localizeNameDescription, JsonReaderState readerState, int arrayIndex)
    {
        TextAsset item = jsonDatas[arrayIndex];
        return ReadJsonSingleFile(arcaniaDatas, localizeNameDescription, readerState, item);
    }

    private static JsonReaderState ReadJsonSingleFile(ArcaniaUnits arcaniaDatas, bool localizeNameDescription, JsonReaderState readerState, TextAsset item)
    {
        var parentNode = SimpleJSON.JSON.Parse(item.text);
        if (parentNode.IsArray)
        {
            foreach (var c in parentNode.Children)
            {
                ReadArrayOwner(arcaniaDatas, c, localizeNameDescription, readerState);
            }
        }
        else
        {
            ReadArrayOwner(arcaniaDatas, parentNode, localizeNameDescription, readerState);
        }
        readerState.jsonIndex++;
        return readerState;
    }

    private static void ReadArrayOwner(ArcaniaUnits arcaniaUnits, SimpleJSON.JSONNode parentNode, bool localizeNameDescription, JsonReaderState readerState)
    {
        var items = parentNode["items"];
        string typeS = parentNode["type"];
        if (typeS == null)
        {

        }
        if (!EnumHelper<UnitType>.TryGetEnumFromName(typeS, out var type)) Debug.LogError($"{typeS} not found in UnitType");
        if (type == UnitType.DIALOG)
        {
            foreach (var item in items.AsArray.Children)
            {
                var dr = new DialogRuntime()
                {
                    // use name as the key for the dialog title localized string
                    Title = localizeNameDescription ? Local.GetText(item["id"] + "_name") : item["title"],
                    Content = localizeNameDescription ? Local.GetText(item["id"] + "_desc") : item["content"],
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
        var isTag = type == UnitType.TAG;
        foreach (var item in items.AsArray.Children)
        {
            var ru = new RuntimeUnit();
            ru.World = readerState.CurrentWorld;
            ReadBasicUnit(ru, item, arcaniaUnits, type, localizeNameDescription);

            IDPointer iDPointer;
            if (isTag)
            {
                iDPointer = arcaniaUnits.GetOrCreateIdPointerWithTag(ru.ConfigBasic.Id);
            }
            else
            {
                iDPointer = arcaniaUnits.GetOrCreateIdPointer(ru.ConfigBasic.Id);
            }

            if (iDPointer.RuntimeUnit != null)
            {
                Debug.LogError($"Potential ID duplication: {iDPointer.id}");
            }
            if (!isTag) 
            {
                iDPointer.RuntimeUnit = ru;
            }
            
            if (type == UnitType.RESOURCE)
            {
                ru.ConfigResource = new ConfigResource()
                {
                    Stressor = item.GetValueOrDefault("stressor", false),
                    HeuristicIntegration = item.GetValueOrDefault("heuristic_integration", null)
                };
                if (ru.ConfigResource.HeuristicIntegration != null) 
                {
                    arcaniaUnits.UnitsIntegratedWithHeuristic.Add(ru);
                }
            }
            if (isTag)
            {
                iDPointer.Tag.RuntimeUnit = ru;
            }
            if (type == UnitType.HINT)
            {
                SimpleJSON.JSONNode key = item.GetValueOrDefault("target_id", null);
                IDPointer idPointerHintTarget = arcaniaUnits.GetOrCreateIdPointer(key);
                arcaniaUnits.PointersThatHaveHintsTargetingThem.Add(idPointerHintTarget);
                ru.ConfigHintData = new ConfigHint()
                {
                    hintTargetPointer = idPointerHintTarget
                };
            };
            if (type == UnitType.TAB)
            {
                var tr = new TabRuntime(ru);
                List<UnitType> acceptedUnitTypes = ru.Tab.AcceptedUnitTypes;
                var key = "unit_types";
                ReadUnitTypesToArray(item, acceptedUnitTypes, key);
                foreach (var pair in item)
                {
                    if (pair.Key == "exploration_active_tab") tr.ExplorationActiveTab = pair.Value.AsBool;
                    if (pair.Key == "archive_only") tr.ArchiveOnly = pair.Value.AsBool;
                    if (pair.Key == "disable_on_archive") tr.DisableOnArchive = pair.Value.AsBool;
                    if (pair.Key == "contains_logs") tr.ContainsLogs = pair.Value.AsBool;
                    if (pair.Key == "necessary_for_desktop_and_thinnable") tr.NecessaryForDesktopAndThinnable = pair.Value.AsBool;
                    if (pair.Key == "open_settings") tr.OpenSettings = pair.Value.AsBool;
                    if (pair.Key == "open_other_tabs") tr.OpenOtherTabs = pair.Value.AsBool;
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
                            if (!localizeNameDescription && pair.Key == "name") sep.Name = pair.Value.AsString;
                            if (pair.Key == "id") sep.Id = pair.Value.AsString;
                            if (pair.Key == "priority") sep.Priority = pair.Value.AsInt;
                            if (pair.Key == "require_max") sep.RequireMax = pair.Value.AsBool;
                            if (pair.Key == "show_space") sep.ShowSpace = pair.Value.AsBool;
                            if (pair.Key == "require_instant") sep.RequireInstant = pair.Value.AsBool;
                            if (pair.Key == "contains_save_slots") sep.ContainsSaveSlots = pair.Value.AsBool;
                            if (pair.Key == "archive_main_ui") sep.ArchiveMainUI = pair.Value.AsBool;
                            if (pair.Key == "tags" || pair.Key == "tag")
                            {
                                sep.Tags ??= new();
                                ReadTags(sep.Tags, pair.Value.AsString, arcaniaUnits);
                            }
                        }
                        if (localizeNameDescription) sep.Name = Local.GetText(sep.Id + "_name");
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
            if (pair.Key == "buy")
            {
                ReadChanges(ct.Buy, pair.Value, arcaniaUnits, -1);
                ru.BuyStatus = BuyStatus.NeedsBuy;
            }
            if (pair.Key == "result_fail") ReadChanges(ct.ResultFail, pair.Value, arcaniaUnits, 1);
            if (pair.Key == "perpetual") ct.Perpetual = pair.Value.AsBool;
            if (pair.Key == "perpetual") explicitPerpetualDefinition = true;
            if (pair.Key == "duration") ct.Duration = pair.Value.AsInt;
            if (pair.Key == "slot") ct.SlotKey = pair.Value.AsString;
            if (pair.Key == "success_rate") ct.SuccessRatePercent = pair.Value.AsInt;
            if (pair.Key == "need") ct.Need = ConditionalExpressionParser.Parse(pair.Value.AsString, arcaniaUnits);
            if (pair.Key == "dot") ReadDot(ru, pair.Value, arcaniaUnits);
        }
        if (!ct.Duration.HasValue)
        {
            if (ct.Effect.Count != 0 || ct.Run.Count != 0 || ct.Perpetual)
            {
                ct.Duration = 1;
            }
        }
        if (ct.Duration.HasValue
            && !ct.Perpetual
            && !explicitPerpetualDefinition
            && !ru.HasMax
            && ru.ConfigBasic.UnitType != UnitType.LOCATION
            && ru.DotRU == null)
        {
            ct.Perpetual = true;
        }
        return ct;
    }

    private static object ReadDot(RuntimeUnit owner, SimpleJSON.JSONNode value, ArcaniaUnits arcaniaUnits)
    {
        var ru = new RuntimeUnit();
        ru.ConfigBasic = new();
        ru.ConfigBasic.name = owner.Name;
        string id = owner.ConfigBasic.Id + "_dot";
        ru.ConfigBasic.name += " (effect)";
        var pointer = arcaniaUnits.GetOrCreateIdPointer(id);
        ru.ConfigBasic.Id = id;
        ru.ConfigBasic.Max = 1;
        pointer.RuntimeUnit = ru;
        owner.DotRU = pointer.RuntimeUnit;
        pointer.RuntimeUnit.ParentRU = owner;
        arcaniaUnits.datas[UnitType.DOT].Add(ru);
        var dc = new DotConfig();
        ru.DotConfig = dc;
        foreach (var c in value)
        {
            switch (c.Key)
            {
                case "duration":
                    {
                        dc.Duration = c.Value.AsInt;
                    }
                    break;
                case "toggle":
                    {
                        dc.Toggle = c.Value.AsBool;
                    }
                    break;
                case "mods":
                case "mod":
                    {
                        ReadMods(ru, c.Value, arcaniaUnits);
                    }
                    break;
                default:
                    break;
            }
        }
        return dc;
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

    private static void ReadMods(
        RuntimeUnit owner,
        SimpleJSON.JSONNode dataJsonMod, ArcaniaUnits arcaniaUnits)
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
                var oneBeforeLast = splittedValues.Length >= 2 ? splittedValues[splittedValues.Length - 2] : null;

                if (last == "max") modType = ModType.MaxChange;
                if (last == "rate") modType = ModType.RateChange;
                if (last == "speed") modType = ModType.Speed;
                if (last == "success_rate") modType = ModType.SuccessRate;
                if (last == "activate") modType = ModType.Activate;

                // EXAMPLES:
                //   crakedvase.mod.clarity.max
                if (splittedValues.Length == 4)
                {
                    secondary = splittedValues[splittedValues.Length - 4];
                }

                // if still undecided
                if (modType == ModType.Invalid)
                {
                    if (oneBeforeLast != null && JsonReader.DictionaryOfChanges.TryGetValue(oneBeforeLast, out var v))
                    {
                        target = last;
                        modType = ModType.ResourceChangeChanger;
                        changeType = v;
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
        List<ModRuntime> mods = arcaniaUnits.Mods;
        mods.Add(md);
        return md;
    }

    private static ConfigBasic ReadBasicUnit(RuntimeUnit ru, SimpleJSON.JSONNode item, ArcaniaUnits arcaniaUnits, UnitType type, bool localizeNameDescription)
    {
        string id = item["id"];
        string desc = localizeNameDescription ? Local.GetText(id + "_desc") : item.GetValueOrDefault("desc", null);
        int max = item.GetValueOrDefault("max", -1);
        var bu = new ConfigBasic();
        if (localizeNameDescription)
        {
            bu.name = Local.GetText(id + "_name");
        }
        ru.ConfigBasic = bu;
        bu.Id = id;
        bu.Desc = desc;
        bu.Max = max;
        bu.UnitType = type;
        foreach (var pair in item)
        {
            if (pair.Key == "initial") ru.SetValue(pair.Value.AsInt);
            if (pair.Key == "above_max") bu.AboveMax = true;
            if (pair.Key == "name" && !localizeNameDescription) bu.name = pair.Value;
            if (pair.Key == "mod" || pair.Key == "mods") ReadMods(owner: ru, dataJsonMod: pair.Value, arcaniaUnits);
            if (pair.Key == "require") ru.ConfigBasic.Require = ConditionalExpressionParser.Parse(pair.Value.AsString, arcaniaUnits);
            if ((pair.Key == "tag" || pair.Key == "tags")) ReadTags(tags: ru.ConfigBasic.Tags, pair.Value.AsString, arcaniaUnits);
            if (pair.Key == "invisible") ru.ForceInvisible = true;
            if (pair.Key == "lock")
            {
                var lockTargetString = pair.Value.AsString;
                if (lockTargetString.Contains(","))
                {
                    var values = lockTargetString.Split(",");
                    foreach (var v in values)
                    {
                        CreateMod(ru, arcaniaUnits, 1, ModType.Lock, v, null);
                    }
                }
                else
                {
                    CreateMod(ru, arcaniaUnits, 1, ModType.Lock, pair.Value.AsString, null);
                }

            }
            if (pair.Key == "icon") bu.SpriteKey = pair.Value.AsString;
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
    public string SpriteKey { get; internal set; }
    public bool AboveMax { get; internal set; }
}

public enum UnitType
{
    RESOURCE, TASK, HOUSE, CLASS, SKILL, FURNITURE, TAB, DIALOG, LOCATION, ENCOUNTER,
    DOT, HINT, TAG
}

public static class ModReplaceKeys 
{
    public const string MAX = "$MAX$";
    public const string RATE = "$RATE$";
    public const string SPEED = "$SPEED$";
    public const string MAXSPACE = "$MAXSPACE$";
    public const string SPACEOCCUPIED = "$SPACEOCCUPIED$";
    public const string SUCCESSRATE = "$SUCCESSRATE$";
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
    public string HumanTextIntermediary { get; internal set; }
    public string HumanTextTarget { get; internal set; }
}

public class DotConfig
{
    public int Duration { get; internal set; }
    public bool Toggle { get; internal set; }
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
    private string _tagName;
    public string tagName
    {
        get => RuntimeUnit?.Name ?? _tagName;
        set
        {
            _tagName = value;
        }
    }
    public List<RuntimeUnit> UnitsWithTag = new();

    public List<DialogRuntime> Dialogs = new();

    public TagRuntime(string tagName)
    {
        this.tagName = tagName;
    }

    public RuntimeUnit RuntimeUnit { get; internal set; }
}
