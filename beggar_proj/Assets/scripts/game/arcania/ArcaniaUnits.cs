using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ArcaniaModel
{
    public ArcaniaUnits arcaniaUnits = new ArcaniaUnits();
    public List<RuntimeUnit> RunningTasks = new();

    internal void TryStartAction(RuntimeUnit data)
    {
        ApplyResourceChanges(data.ConfigTask.Cost);
        if (data.IsInstant()) CompleteTask(data);
        if (data.IsInstant()) return;


    }

    private void CompleteTask(RuntimeUnit data)
    {
        ApplyResourceChanges(data.ConfigTask.Result);
        if (data.ConfigTask.Perpetual) 
        {
            data.TaskProgress = 0;
            TryStartAction(data);
        }
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

    public void ManualUpdate(float dt)
    {
        using var _1 = ListPool<RuntimeUnit>.Get(out var list);
        list.AddRange(RunningTasks);
        foreach (var run in list)
        {
            float beforeProg = run.TaskProgress;
            run.TaskProgress += dt;
            // reached a new second in progress
            if (Mathf.FloorToInt(run.TaskProgress) > Mathf.FloorToInt(beforeProg))
            {
                ApplyResourceChanges(run.ConfigTask.Run);
                ApplyResourceChanges(run.ConfigTask.Effect);
            }
            if (run.IsTaskComplete())
            {
                CompleteTask(run);
            }
        }
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
