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
        var lId = "";
        switch (pt)
        {
            case PoolType.DESC:
                lId = "lore_text";
                break;
            case PoolType.TRIPLE_TEXT_VIEW:
                break;
            case PoolType.LEFT_HEADER:
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
            case PoolType.LEFT_HEADER:
                break;
            default:
                break;
        }
        return pcu;
    }

    internal static void OnExpandChanged(MainGameControl mgc, JRTControlUnit unit, bool value)
    {
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

                unit.DescriptionCU = GetFreeUnit(mgc, PoolType.DESC);
                unit.DescriptionCU.TextAccessor.SetTextRaw(desc);
                unit.MainLayout.AddLayoutAsChild(unit.DescriptionCU.layout);
                unit.DescriptionCU.layout.SetParentShowing(true);
                // MainGameControlSetupJLayout.AddToExpand(unit.MainLayout, unit.DescriptionCU.layout, unit);
            } else if(!value && unit.DescriptionCU != null)
            {
                unit.MainLayout.RemoveLayoutAsChild(unit.DescriptionCU.layout);
                unit.DescriptionCU.layout.SetParentShowing(false);
                FreeUnit(mgc.JControlData.poolData, unit.DescriptionCU, PoolType.DESC);
                unit.DescriptionCU = null;
            }
        }
    }

    private static void FreeUnit(MainGameJLayoutPoolData poolData, PoolChildUnit descriptionCU, PoolType dESC)
    {
        poolData.pools[dESC].pool.Free(descriptionCU);
    }
}
public class MainGameJLayoutPoolData
{
    public Dictionary<PoolType, PoolListUnit> pools = new();
    public enum PoolType
    {
        DESC, TRIPLE_TEXT_VIEW, LEFT_HEADER, // more stuff for things like duration text and all
    }

    public class PoolListUnit
    {
        public Pool<PoolChildUnit> pool = new();
    }

    public class PoolChildUnit
    {
        public JLayoutRuntimeUnit layout;
        public JLayTextAccessor TextAccessor => TextAccessors.Length > 0 ? TextAccessors[0] : null;
        public JLayTextAccessor[] TextAccessors;
    }
}
