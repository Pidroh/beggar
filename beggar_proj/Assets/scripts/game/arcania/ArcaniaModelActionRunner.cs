using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using static ArcaniaModel;

public class ArcaniaModelHousing : ArcaniaModelSubmodule
{

    public ArcaniaModelHousing(ArcaniaModel arcaniaModel) : base(arcaniaModel)
    {
    }

    public bool CanChangeHouse(RuntimeUnit ru)
    {
        // already in the house, so cannot change to it
        if (ru.Value != 0) return false;
        if (ru.ConfigHouse.AvailableSpace < CalculateConsumedSpace()) return false;
        if (!_model.CanAfford(ru.ConfigTask.Cost)) return false;
        return true;
    }

    public void ChangeHouse(RuntimeUnit ru) 
    {
        var houses = _model.arcaniaUnits.datas[UnitType.HOUSE];
        // unequip all houses first
        foreach (var f in houses)
        {
            f.SetValue(0);
        }
        ru.SetValue(1);
    }

    private int CalculateConsumedSpace()
    {
        var space = 0;
        var furnitures = _model.arcaniaUnits.datas[UnitType.FURNITURE];
        foreach (var f in furnitures)
        {
            space += f.ConfigFurniture.SpaceConsumed * f.Value;
        }
        return space;
    }
}


public class ArcaniaModelActionRunner : ArcaniaModelSubmodule
{

    public List<RuntimeUnit> RunningTasks = new();

    public ArcaniaModelActionRunner(ArcaniaModel arcaniaModel) : base(arcaniaModel)
    {
    }

    public bool CanStudySkill(RuntimeUnit data) 
    {
        // skill only needs to pay the cost when acquiring it
        return !data.IsMaxed && _model.CanAfford(data.ConfigTask.Run);
    }

    public bool CanStartAction(RuntimeUnit data) 
    {
        // once you refactor this so that you don't need to pay the cost every time (only when starting for 'the first time')
        // make CanStudySkill also use this
        if (!_model.CanAfford(data.ConfigTask.Cost)) return false;
        if (data.IsMaxed) return false;
        if (data.IsInstant()) return true;
        return _model.CanAfford(data.ConfigTask.Run);
    }


    public void StudySkill(RuntimeUnit data)
    {
        // studying a skill is always continuous
        RunContinuously(data);
    }


    internal void StartAction(RuntimeUnit data)
    {
        _model.ApplyResourceChanges(data, ResourceChangeType.COST);
        if (data.IsInstant()) CompleteTask(data);
        if (data.IsInstant()) return;
        RunContinuously(data);
    }

    private bool RunContinuously(RuntimeUnit data)
    {
        bool alreadyRunning = RunningTasks.Contains(data);
        RunningTasks.Clear();
        if (alreadyRunning) return false;
        // start running if not instant and not already started
        RunningTasks.Add(data);

        return true;
    }

    private void CompleteTask(RuntimeUnit data)
    {
        _model.ApplyResourceChanges(data, ResourceChangeType.RESULT);
        RunningTasks.Remove(data);
        if (data.ConfigTask.Perpetual)
        {
            data.TaskProgress = 0;
            StartAction(data);
        }
    }

    public void ManualUpdate(float dt) {
        using var _1 = ListPool<RuntimeUnit>.Get(out var list);
        list.AddRange(RunningTasks);
        foreach (var run in list)
        {
            var taskContinue = _model.CanAfford(run.ConfigTask.Run);
            if (run.ConfigBasic.UnitType == UnitType.SKILL)
            {
                // even if result and effect are redudant, skills still run to get XP, so nothing to do here
            }
            else 
            {
                taskContinue = taskContinue && (_model.DoChangesMakeADifference(run.ConfigTask.Result) || _model.DoChangesMakeADifference(run.ConfigTask.Effect));
            }
            
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
                _model.ApplyResourceChanges(run, ResourceChangeType.RUN);
                _model.ApplyResourceChanges(run, ResourceChangeType.EFFECT);
                if (run.ConfigBasic.UnitType == UnitType.SKILL) 
                {
                    run.Skill.StudySkillTick();
                }
            }
            if (run.IsTaskComplete())
            {
                CompleteTask(run);
            }
        }
    }
}
