using arcania;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeUnit
{
    public ConfigBasic ConfigBasic;
    public ConfigTask ConfigTask;
    public List<ModData> ModsTargetingSelf = new();
    public bool RequireMet = false;

    public string Name => ConfigBasic.name;

    public int Max => CalculateMax();

    public bool Visible => CalculateVisible();

    private bool CalculateVisible()
    {
        RequireMet = RequireMet || MeetsCondition(ConfigBasic.Require);
        return RequireMet;
    }

    private bool MeetsCondition(ConditionalExpression expression)
    {
        
    }

    private int CalculateMax()
    {
        // has no max from the get go
        if (ConfigBasic.Max < 0) return -1;
        var sum = Mathf.FloorToInt(GetModSum(modType: ModType.MaxChange));
        return Mathf.Max(ConfigBasic.Max + sum, 0);
    }

    private float GetModSum(ModType modType)
    {
        var v = 0f;
        foreach (var mod in ModsTargetingSelf)
        {
            if (mod.ModType != modType) continue;
            v += mod.Source.Value * mod.Value;
        }
        return v;
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
    public bool IsMaxed => Value >= MaxForCeiling;

    public bool IsZero => Value == 0;
}
