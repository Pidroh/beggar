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
            pd.pools[pt] = poolH;
            poolH = new();
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
