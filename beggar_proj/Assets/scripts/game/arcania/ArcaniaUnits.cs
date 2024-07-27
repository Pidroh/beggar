using System;
using System.Collections.Generic;
using UnityEngine;

public class ArcaniaModel {
    public ArcaniaUnits arcaniaUnits = new ArcaniaUnits();

    internal void TryStartAction(RuntimeUnit data)
    {
        ApplyResourceChanges(data.ConfigTask.Cost);
        ApplyResourceChanges(data.ConfigTask.Result);
    }

    private void ApplyResourceChanges(List<ResourceChange> changes)
    {
        foreach (var c in changes)
        {
            ChangeValue(c.IdPointer.RuntimeUnit, c.valueChange);
        }
    }

    private void ChangeValue(RuntimeUnit runtimeUnit, int valueChange)
    {
        var v = runtimeUnit.Value;
        runtimeUnit.Value = Mathf.Clamp(v + valueChange, 0, runtimeUnit.MaxForCeiling);
    }
}

public class ArcaniaUnits
{
    public Dictionary<UnitType, List<RuntimeUnit>> datas = new();
    public Dictionary<string, IDPointer> IdMapper = new();

    internal IDPointer GetOrCreateIdPointer(string key)
    {
        if (!IdMapper.TryGetValue(key, out var value))
        {
            value = new IDPointer()
            {
                id = key
            };
            IdMapper[key] = value;
        }
        return value;
    }
    //public List<BasicUnit> resources = new();
}
