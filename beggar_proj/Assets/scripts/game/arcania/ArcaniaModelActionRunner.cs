using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using static ArcaniaModel;

public class ArcaniaModelActionRunner : ArcaniaModelSubmodule
{

    public List<RuntimeUnit> RunningTasks = new();

    public ArcaniaModelActionRunner(ArcaniaModel arcaniaModel) : base(arcaniaModel)
    {
    }

    internal bool TryStartAction(RuntimeUnit data)
    {
        if (!_model.CanAfford(data.ConfigTask.Cost)) return false;
        if (data.IsMaxed) return false;
        _model.ApplyResourceChanges(data.ConfigTask.Cost);
        if (data.IsInstant()) CompleteTask(data);
        if (data.IsInstant()) return true;
        bool alreadyRunning = RunningTasks.Contains(data);
        RunningTasks.Clear();
        if (alreadyRunning) return false;
        // start running if not instant and not already started
        RunningTasks.Add(data);

        return true;
    }

    private void CompleteTask(RuntimeUnit data)
    {
        _model.ApplyResourceChanges(data.ConfigTask.Result);
        RunningTasks.Remove(data);
        if (data.ConfigTask.Perpetual)
        {
            data.TaskProgress = 0;
            TryStartAction(data);
        }
    }

    public void ManualUpdate(float dt) {
        using var _1 = ListPool<RuntimeUnit>.Get(out var list);
        list.AddRange(RunningTasks);
        foreach (var run in list)
        {

            var taskContinue = _model.CanAfford(run.ConfigTask.Run);
            taskContinue = taskContinue && (_model.DoChangesMakeADifference(run.ConfigTask.Result) || _model.DoChangesMakeADifference(run.ConfigTask.Effect));
            if (!taskContinue)
            {
                RunningTasks.Remove(run);
                continue;
            }

            float beforeProg = run.TaskProgress;
            run.TaskProgress += dt;
            // reached a new second in progress
            if (Mathf.FloorToInt(run.TaskProgress) > Mathf.FloorToInt(beforeProg))
            {
                _model.ApplyResourceChanges(run.ConfigTask.Run);
                _model.ApplyResourceChanges(run.ConfigTask.Effect);
            }
            if (run.IsTaskComplete())
            {
                CompleteTask(run);
            }
        }
    }
}
