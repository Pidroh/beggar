using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using static ArcaniaModel;

public class ArcaniaModelActionRunner : ArcaniaModelSubmodule
{

    public List<RuntimeUnit> RunningTasks = new();
    private RuntimeUnit _dataWaitingForDialog;

    public RuntimeUnit InterruptedAction { get; private set; }

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
        if (!data.NeedMet) return false;
        if (data.BuyStatus == BuyStatus.NeedsBuy) return _model.CanAfford(data.ConfigTask.Buy);
        // once you refactor this so that you don't need to pay the cost every time (only when starting for 'the first time')
        // make CanStudySkill also use this
        if (!data.IsTaskHalfWay) if (!_model.CanAfford(data.ConfigTask.Cost)) return false;

        if (!CheckIfActionIsMeaningful(data)) return false;

        if (data.IsMaxed) return false;
        if (data.IsInstant()) return true;
        return _model.CanAfford(data.ConfigTask.Run);
    }


    public void StudySkill(RuntimeUnit data)
    {
        // studying a skill is always continuous
        RunContinuously(data);
    }

    public void StartActionExternally(RuntimeUnit data)
    {
        if (!RunningTasks.Contains(data))
        {
            foreach (var tag in data.ConfigBasic.Tags)
            {
                if (tag.Tag.Dialogs.Count > 0)
                {
                    _dataWaitingForDialog = data;
                    this._model.Dialog.ShowDialog(tag.Tag.Dialogs[0]);
                    return;
                }
            }
        }
        InterruptedAction = null;
        StartAction(data);
    }

    private void StartAction(RuntimeUnit data)
    {
        if (data.BuyStatus == BuyStatus.NeedsBuy) 
        {
            _model.ApplyResourceChanges(data, ResourceChangeType.BUY);
            data.BuyStatus = BuyStatus.Bought;
            data.MarkSelfDirty();
            return;
        }
        // only DONE or FRESH tasks need to pay the cost
        if (!data.IsTaskHalfWay) _model.ApplyResourceChanges(data, ResourceChangeType.COST);
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

    internal void StopAllOfType(UnitType unitType)
    {
        using var _1 = ListPool<RuntimeUnit>.Get(out var list);
        list.AddRange(RunningTasks);
        foreach (var run in list)
        {
            if (run.ConfigBasic.UnitType != unitType) continue;
            StopTask(run);
        }
    }

    public void CompleteTask(RuntimeUnit data)
    {
        data.TaskProgress = 0;
        int rangeValue = UnityEngine.Random.Range(0, 100);
        var success = data.SuccessRatePercent > rangeValue;
        //var success = data.ConfigTask.SuccessRatePercent.HasValue ? data.ConfigTask.SuccessRatePercent.Value > rangeValue : true;
        if (success)
        {
            var firstTime = data.Value == 0;
            data.ChangeValue(1);
            if (data.DotRU != null) 
            {
                ArcaniaModelDotCode.DotActionSuccess(data);
            }
            if (data.Skill != null)
            {
                data.Skill.xp = 0;
            }
            _model.ApplyResourceChanges(data, ResourceChangeType.RESULT);
            if (firstTime)
                _model.ApplyResourceChanges(data, ResourceChangeType.RESULT_ONCE);
        }
        else 
        {
            _model.ApplyResourceChanges(data, ResourceChangeType.RESULT_FAIL);
        }

        StopTask(data);
        bool isLocation = data.ConfigBasic.UnitType == UnitType.LOCATION;
        if (isLocation)
        {
            // If completes location, just tries 
            TryStartRestAction();
            return;
        }
        if (data.ConfigTask.Perpetual && this.CanStartAction(data))
        {
            StartAction(data);
        }
        else
        {
            // instant tasks don't cause swapping
            if (data.ConfigTask.Duration > 0)
            {
                TaskInterruptedTrySwap(data);
            }

        }
    }

    public void ManualUpdate(float dt)
    {
        if (_dataWaitingForDialog != null && _model.Dialog.HasResult(out int option))
        {
            if (option == 0 && CanStartAction(_dataWaitingForDialog))
            {
                this.StartAction(_dataWaitingForDialog);
            }
            _dataWaitingForDialog = null;
        }
        using var _1 = ListPool<RuntimeUnit>.Get(out var list);
        list.AddRange(RunningTasks);
        foreach (var run in list)
        {
            var taskContinue = _model.CanAfford(run.ConfigTask.Run) && !run.IsMaxed;
            if (run.ConfigBasic.UnitType == UnitType.SKILL)
            {
                // even if result and effect are redudant, skills still run to get XP, so nothing to do here
            }
            else
            {
                taskContinue = taskContinue && CheckIfActionIsMeaningful(run);
            }

            if (!taskContinue)
            {
                InterruptTask(run);
                continue;
            }

            float beforeProg = run.TaskProgress;
            float taskProgressDt = dt * run.GetSpeedMultiplier();
            run.TaskProgress += taskProgressDt;

            // reached a new second in progress
            int previousProgress = Mathf.FloorToInt(beforeProg);
            int newProgress = Mathf.FloorToInt(run.TaskProgress);
            var numberOfLoops = newProgress - previousProgress;

            if (numberOfLoops > 0)
            {
                _model.Exploration.UpdateLoopProgressedSecond();
                _model.ApplyResourceChanges(run, ResourceChangeType.RUN);
                _model.ApplyResourceChanges(run, ResourceChangeType.EFFECT);
                if (run.ConfigBasic.UnitType == UnitType.SKILL)
                {
                    run.Skill.StudySkillTick();
                    if (run.Skill.HasEnoughXPToLevelUp())
                    {
                        run.ChangeValue(1);
                        run.Skill.xp = 0;
                    }

                    run.TaskProgress = 0;
                }

                // if duration is 1, make sure to only complete the task after run / effect is applied
                if (run.ConfigTask.Duration == 1)
                {
                    if (run.IsTaskComplete())
                    {
                        CompleteTask(run);
                    }
                }
            }
            // if duration is 1, it will get completed on the code above
            if (run.ConfigTask.Duration != 1)
            {
                if (run.IsTaskComplete())
                {
                    CompleteTask(run);
                }
            }

        }
    }

    public void InterruptTask(RuntimeUnit run)
    {
        StopTask(run);
        TaskInterruptedTrySwap(run);
    }

    private void StopTask(RuntimeUnit run)
    {
        RunningTasks.Remove(run);
    }

    private void TaskInterruptedTrySwap(RuntimeUnit run)
    {
        if (run == _model.arcaniaUnits.RestActionActive && InterruptedAction != null)
        {
            if (!CanStartAction(InterruptedAction))
            {
                InterruptedAction = null;
                return;
            }

            StartAction(InterruptedAction);
            InterruptedAction = null;
        }
        else
        {
            bool restActionStarted = TryStartRestAction();
            if (!restActionStarted) return;
            InterruptedAction = run;

        }
    }

    private bool TryStartRestAction()
    {
        if (_model.arcaniaUnits.RestActionActive == null) return false;
        RuntimeUnit restAct = _model.arcaniaUnits.RestActionActive;
        if (!CanStartAction(restAct) || !restAct.Visible)
        {
            return false;
        }
        StartAction(_model.arcaniaUnits.RestActionActive);
        return true;
    }

    private bool CheckIfActionIsMeaningful(RuntimeUnit run)
    {
        if (run.DotConfig != null) return true;
        if (run.DotRU != null) return true;
        if (run.HasMax) return true;
        if (run.ConfigBasic.UnitType == UnitType.LOCATION) return true;
        if (run.Value == 0 && _model.DoChangesMakeADifference(run, ResourceChangeType.RESULT_ONCE)) return true;
        return (_model.DoChangesMakeADifference(run, ResourceChangeType.RESULT) || _model.DoChangesMakeADifference(run, ResourceChangeType.EFFECT));
    }


    public bool CanAcquireSkill(RuntimeUnit ru)
    {
        return _model.CanAfford(ru.ConfigTask.Cost) && !ru.Skill.Acquired;
    }

    public void AcquireSkill(RuntimeUnit ru)
    {
        _model.ApplyResourceChanges(ru, ResourceChangeType.COST);
        ru.Skill.Acquire();
    }
}
