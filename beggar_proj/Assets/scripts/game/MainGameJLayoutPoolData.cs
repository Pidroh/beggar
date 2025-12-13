using HeartEngineCore;
using HeartUnity;
using JLayout;
using System;
using System.Collections.Generic;
using static MainGameJLayoutPoolData;

public static class MainGameJLayoutPoolExecuter
{
    public static PoolChildUnit GetFreeUnit(MainGameControl mgc, PoolType pt)
    {
        var pd = mgc.JControlData.poolData;
        if (!pd.pools.TryGetValue(pt, out var poolH))
        {
            poolH = new();
            pd.pools[pt] = poolH;
        }

        if (poolH.pool.hasFree())
        {
            // do nothing
        }
        else
        {
            PoolChildUnit free = Create(mgc, pt);
            poolH.pool.AddFree(free);
        }
        return poolH.pool.Activate();
    }

    private static PoolChildUnit Create(MainGameControl mgc, PoolType pt)
    {
        var pcu = new PoolChildUnit();
        pcu.PoolType = pt;
        var lId = "";
        switch (pt)
        {
            case PoolType.DESC:
                lId = "lore_text";
                break;
            case PoolType.TRIPLE_TEXT_VIEW:
                lId = "in_header_triple_statistic";
                break;
            case PoolType.MINI_HEADER:
                lId = "left_mini_header";
                break;
            default:
                break;
        }
        var layout = JCanvasMaker.CreateLayout(lId, mgc.JLayoutRuntime);
        pcu.layout = layout;

        switch (pt)
        {
            case PoolType.DESC:
                pcu.TextAccessors = new JLayTextAccessor[1]
                {
                    new JLayTextAccessor(layout, 0)
        };
                break;
            case PoolType.TRIPLE_TEXT_VIEW:
                break;
            case PoolType.MINI_HEADER:
                break;
            default:
                break;
        }
        return pcu;
    }

    internal static void OnExpandChanged(MainGameControl mgc, JRTControlUnit unit, bool value)
    {
        var controlData = mgc.JControlData;
        var modelData = unit.Data;

        if (unit.Data?.ConfigBasic.HasDesc ?? false)
        {
            if (value && unit.DescriptionCU == null)
            {
                // get description of the hint target if it exists, if not, own description
                string desc = modelData.ConfigHintData?.hintTargetPointer.RuntimeUnit.ConfigBasic.Desc ?? modelData.ConfigBasic.Desc;
                if (modelData.ConfigHintData != null)
                {
                    Logger.Log("Hint data present");
                }

                PoolChildUnit pUnit = GetFreeUnitBoundToRuntimeUnit(mgc, unit,PoolType.DESC);
                pUnit.TextAccessor.SetTextRaw(desc);
                unit.DescriptionCU = pUnit;
                // MainGameControlSetupJLayout.AddToExpand(unit.MainLayout, unit.DescriptionCU.layout, unit);
            }
            else if (!value && unit.DescriptionCU != null)
            {
                PoolChildUnit unitToRemove = unit.DescriptionCU;
                FreePooledUnitFromRuntimeUnit(mgc, unit, unitToRemove);
                unit.DescriptionCU = null;
            }
        }
        #region change list
        if (value)
        {
            // MainGameControlSetupJLayout.EnsureChangeListViewsAreCreated(mgc.JLayoutRuntime, modelData, unit, unit.MainLayout, mgc.JControlData);

            if (modelData.ConfigTask != null)
            {
                for (int rcgIndex = 0; rcgIndex < modelData.ConfigTask.ResourceChangeLists.Count; rcgIndex++)
                {
                    List<ResourceChange> rcl = modelData.ConfigTask.ResourceChangeLists[rcgIndex];
                    if (rcl == null) continue;
                    if (rcl.Count == 0) continue;

                    // it might be already instantiated
                    unit.ChangeGroups[rcgIndex] ??= new();
                    var changeType = (ResourceChangeType)rcgIndex;

                    ColorData changeTypeOverride = null;
                    if (controlData.ColorForResourceChangeType.TryGetValue(changeType, out var v))
                    {
                        changeTypeOverride = v;
                    }

                    string textRaw = changeType switch
                    {
                        ResourceChangeType.COST => controlData.LabelCost,
                        ResourceChangeType.RESULT => controlData.LabelResult,
                        ResourceChangeType.RUN => controlData.LabelRun,
                        ResourceChangeType.EFFECT => controlData.LabelEffect,
                        ResourceChangeType.RESULT_ONCE => controlData.LabelResultOnce,
                        ResourceChangeType.RESULT_FAIL => controlData.LabelResultFail,
                        ResourceChangeType.BUY => controlData.LabelBuy,
                        _ => null,
                    };

                    var headerUnit = GetFreeUnitBoundToRuntimeUnit(mgc, unit, PoolType.MINI_HEADER);
                    headerUnit.layout.SetTextRaw(0, textRaw);
                    unit.ChangeListMixedPoolCache.Add(headerUnit);
                    unit.ChangeGroups[rcgIndex].Header = headerUnit.layout;
                    AutoList<JLayoutRuntimeUnit> tripleTextViews = unit.ChangeGroups[rcgIndex].tripleTextViews;
                    for (int i = 0; i < rcl.Count; i++)
                    {
                        ResourceChange rcu = rcl[i];
                        var ttvUnit = GetFreeUnitBoundToRuntimeUnit(mgc, unit, PoolType.TRIPLE_TEXT_VIEW);
                        var triple = ttvUnit.layout;
                        triple.SetTextRaw(0, rcu.IdPointer.RuntimeUnit?.Name);
                        triple.SetTextRaw(1, "" + rcu.valueChange.min);
                        triple.SetTextRaw(2, "0");
                        if (changeTypeOverride != null)
                        {
                            triple.TextChildren[0].OverwriteSingleColor(ColorSetType.NORMAL, changeTypeOverride);
                            triple.TextChildren[1].OverwriteSingleColor(ColorSetType.NORMAL, changeTypeOverride);
                        }
                        unit.ChangeListMixedPoolCache.Add(ttvUnit);
                        tripleTextViews.Add(triple);
                    }
                }
            }
        }
        else
        {
            foreach (var item in unit.ChangeListMixedPoolCache)
            {
                item.layout.ClearOverwriteColorOfTextChildren();
                FreePooledUnitFromRuntimeUnit(mgc, unit, item);
            }
            foreach (var item in unit.ChangeGroups)
            {
                if (item == null) continue;
                item.Header = null;
                item.tripleTextViews.Clear();
            }
            unit.ChangeListMixedPoolCache.Clear();
        }
        #endregion

        #region mod
        if (value)
        {
            for (int i = 0; i < 4; i++)
            {
                // mode is 1 on the fourth i though, set in the switch
                int mode = i;
                var unitForOwnedMods = modelData.DotRU == null ? modelData : modelData.DotRU;
                var unitForOtherMods = modelData;
                var modsOwned = unitForOwnedMods.ModsOwned;
                List<ModRuntime> modList = null;
                var miniHeaderLabel = "";
                JRTControlUnitMods jrtMod = null;
                switch (i)
                {
                    case 0:
                        {
                            modList = modsOwned;
                            miniHeaderLabel = controlData.LabelModifications;
                            jrtMod = unit.OwnedMods;
                            break;
                        }
                    case 1:
                        {
                            modList = unitForOtherMods.ModsSelfAsIntermediary;
                            miniHeaderLabel = controlData.LabelModificationsExtra;
                            jrtMod = unit.IntermediaryMods;
                            break;
                        }
                    case 2:
                        {
                            modList = unitForOtherMods.ModsTargetingSelf;
                            miniHeaderLabel = controlData.LabelModificationsTargeting;
                            jrtMod = unit.TargetingThisMods;
                            break;
                        }
                    case 3:
                        {
                            modList = modelData.DotRU?.ModsSelfAsIntermediary;
                            miniHeaderLabel = controlData.LabelModificationsExtraEffect;
                            jrtMod = unit.TargetingThisEffectMods;
                            mode = 1;
                            break;
                        }
                    default:
                        break;
                }
                if ((modList?.Count ?? 0) > 0)
                {
                    var headerUnit = GetFreeUnitBoundToRuntimeUnit(mgc, unit, PoolType.MINI_HEADER);
                    headerUnit.layout.SetTextRaw(0, miniHeaderLabel);
                    unit.ModMixedPoolCache.Add(headerUnit);
                    jrtMod.Header = headerUnit.layout;
                    foreach (var mod in modList)
                    {
                        if (mod.ModType == ModType.Lock) continue;
                        var mainText = mode == 0 ? mod.HumanText : (mode == 1 ? mod.HumanTextIntermediary : mod.HumanTextTarget);
                        if (mainText == null) continue;

                        var ttvUnit = GetFreeUnitBoundToRuntimeUnit(mgc, unit, PoolType.TRIPLE_TEXT_VIEW);
                        unit.ModMixedPoolCache.Add(ttvUnit);
                        var triple = ttvUnit.layout;
                        //var triple = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("in_header_triple_statistic"), runtime);
                        /*
                        if (modReplacements.TryGetValue(mod.ModType, out var values))
                        {
                            mainText = mainText.Replace(values.key, values.valueWithColor);
                            if (values.colorD != null)
                            {
                                triple.TextChildren[1].OverwriteSingleColor(ColorSetType.NORMAL, values.colorD);
                            }
                        }*/
                        var modControl = jrtMod;
                        modControl.tripleTextViews.Add(triple);
                        modControl.Mods.Add(mod);
                        var modValue = mod.Value;

                        triple.SetTextRaw(0, mainText);
                        // activate mod has no number
                        if (mod.ModType == ModType.Activate)
                        {
                            triple.SetTextRaw(1, "");
                            continue;
                        }
                        string secondaryText;
                        if (modValue > 0 && mod.ModType != ModType.SpaceConsumption)
                            secondaryText = $"+{modValue}";
                        else
                            secondaryText = $"{modValue}";
                        triple.SetTextRaw(1, secondaryText);

                    }
                }
            }
        }
        else 
        {
            foreach (var item in unit.ModMixedPoolCache)
            {
                item.layout.ClearOverwriteColorOfTextChildren();
                FreePooledUnitFromRuntimeUnit(mgc, unit, item);
            }
            void ClearModList(JRTControlUnitMods ownedMods) 
            {
                ownedMods.tripleTextViews.Clear();
                ownedMods.Header = null;
            }
            ClearModList(unit.OwnedMods);
            ClearModList(unit.TargetingThisEffectMods);
            ClearModList(unit.TargetingThisMods);
            ClearModList(unit.IntermediaryMods);
            unit.ModMixedPoolCache.Clear();
        }
        
        #endregion
    }

    private static PoolChildUnit GetFreeUnitBoundToRuntimeUnit(MainGameControl mgc, JRTControlUnit unit, PoolType pt)
    {
        var pUnit = GetFreeUnit(mgc, pt);
        unit.MainLayout.AddLayoutAsChild(pUnit.layout);
        pUnit.layout.SetParentShowing(true);
        return pUnit;
    }

    private static void FreePooledUnitFromRuntimeUnit(MainGameControl mgc, JRTControlUnit unit, PoolChildUnit unitToRemove)
    {
        unit.MainLayout.RemoveLayoutAsChild(unitToRemove.layout);
        unitToRemove.layout.SetParentShowing(false);
        FreeUnit(mgc.JControlData.poolData, unitToRemove);
    }

    private static void FreeUnit(MainGameJLayoutPoolData poolData, PoolChildUnit unit)
    {
        poolData.pools[unit.PoolType].pool.Free(unit);
    }
}
public class MainGameJLayoutPoolData
{
    public Dictionary<PoolType, PoolListUnit> pools = new();
    public enum PoolType
    {
        DESC, TRIPLE_TEXT_VIEW, MINI_HEADER, // more stuff for things like duration text and all
    }

    public class PoolListUnit
    {
        public Pool<PoolChildUnit> pool = new();
    }

    public class PoolChildUnit
    {
        public JLayoutRuntimeUnit layout;
        public JLayTextAccessor TextAccessor => TextAccessors.Length > 0 ? TextAccessors[0] : null;

        public PoolType PoolType { get; internal set; }

        public JLayTextAccessor[] TextAccessors;
    }
}
