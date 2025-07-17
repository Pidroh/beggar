public static class ArcaniaModelDotCode
{
    internal static void DotActionSuccess(RuntimeUnit data)
    {
        data.DotRU.TaskProgress = 0;
        data.DotRU.SetValue(1);
    }
    internal static void DotActionStopExternally(RuntimeUnit data)
    {
        data.DotRU.SetValue(0);
    }

    public static void Update(ArcaniaModel model, float dt) 
    {
        foreach (var dot in model.arcaniaUnits.datas[UnitType.DOT])
        {
            if (dot.Value == 0) continue;
            dot.TaskProgress += dt;
            if (dot.DotConfig.Duration <= dot.TaskProgress) 
            {
                dot.SetValue(0);
            }
        }
        
    }
}
