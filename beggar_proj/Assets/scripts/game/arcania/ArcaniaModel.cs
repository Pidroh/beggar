using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ConfigResource 
{
    public bool Stressor;
}

public class ConfigEncounter
{
    public JSONNode Length { get; internal set; }
}

public class ConfigLocation
{
    public int Length { get; set; }
}
public class ConfigSkill
{
    public int LearningDifficultyLevel;
}

public class ConfigHouse
{
    public int AvailableSpace;
}

public class ConfigFurniture
{
    public int SpaceConsumed;
}

public class ArcaniaModel
{
    public List<LogUnit> LogUnits = new();
    public ArcaniaUnits arcaniaUnits = new ArcaniaUnits();
    public ArcaniaModelExploration Exploration;
    public ArcaniaModelActionRunner Runner;
    public ArcaniaModelHousing Housing;
    float _oneSecondCounter;
    public DialogModel Dialog = new();

    public ArcaniaModel()
    {
        Runner = new(this);
        Housing = new(this);
        Exploration = new(this);
    }

    internal void ApplyResourceChanges(RuntimeUnit parent, ResourceChangeType changeType)
    {
        var changes = parent.ConfigTask.GetResourceChangeList(changeType);
        foreach (var c in changes)
        {
            foreach (var ru in c.IdPointer)
            {
                switch (c.ModificationType)
                {
                    case ResourceChange.ResourceChangeModificationType.NormalChange:
                        ru.ChangeValueByResourceChange(parent, c.valueChange, changeType);
                        break;
                    case ResourceChange.ResourceChangeModificationType.XpChange:
                        // Mods not supported for now
                        ru.Skill.xp += (int) c.valueChange.getValue(UnityEngine.Random.Range(0f, 1f));
                        break;
                    default:
                        break;
                }
                
            }
        }
    }

    public void ChangeValue(RuntimeUnit runtimeUnit, int valueChange)
    {
        runtimeUnit.ChangeValue(valueChange);
    }

    public void ManualUpdate(float dt)
    {
        Dialog.ManualUpdate();
        Runner.ManualUpdate(dt);
        Exploration.ManualUpdate();
        _oneSecondCounter += dt;
        var applyRateNumber = 0;
        while (_oneSecondCounter > 1f)
        {
            _oneSecondCounter -= 1f;
            applyRateNumber++;
        }
        foreach (var pair in arcaniaUnits.datas)
        {
            foreach (var item in pair.Value)
            {
                if (item.UpdateRequireStatus() && item.ConfigBasic.UnitType != UnitType.TAB)
                {
                    LogUnits.Add(new LogUnit()
                    {
                        logType = LogUnit.LogType.UNIT_UNLOCKED,
                        Unit = item
                    });
                }
                for (int i = 0; i < applyRateNumber; i++)
                {
                    item.ApplyRate();
                }
            }
        }
    }

    public class ArcaniaModelSubmodule
    {
        internal ArcaniaModel _model;

        public ArcaniaModelSubmodule(ArcaniaModel arcaniaModel)
        {
            _model = arcaniaModel;
        }
    }


    public bool CanAfford(List<ResourceChange> changes)
    {
        foreach (var rc in changes)
        {
            if (rc.valueChange.BothEqual(0f)) continue;
            if (rc.IdPointer.RuntimeUnit.CanFullyAcceptChange(rc.valueChange)) continue;
            return false;
        }
        return true;
    }

    public bool DoChangesMakeADifference(List<ResourceChange> changes)
    {
        foreach (var rc in changes)
        {
            if (rc.valueChange.BothEqual(0f)) continue;
            if (rc.valueChange.BiggerThan(0f) && rc.IdPointer.IsAllMaxed()) continue;
            if (rc.valueChange.SmallerThan(0f) && rc.IdPointer.IsAllZero()) continue;
            return true;
        }
        return false;

    }

    private RuntimeUnit FindRuntimeUnitInternal(UnitType type, string v)
    {
        if (!arcaniaUnits.datas.TryGetValue(type, out var units)) return null;
        foreach (var item in units)
        {
            if (item.ConfigBasic.Id == v) return item;
        }
        return null;
    }

    public RuntimeUnit FindRuntimeUnit(string id)
    {
        var types = EnumHelper<UnitType>.GetAllValues();
        foreach (var t in types)
        {
            if (!arcaniaUnits.datas.ContainsKey(t)) continue;
            var ru = FindRuntimeUnitInternal(t, id);
            if (ru == null) continue;
            return ru;
        }
        Debug.Log($"Runtime unit of ID |{id}| NOT FOUND");
        return null;

    }

    internal RuntimeUnit FindRuntimeUnit(UnitType type, string v)
    {
        var ru = FindRuntimeUnitInternal(type, v);
        if (ru != null) return ru;
        Debug.Log($"Runtime unit of type |{type}| and ID |{v}| NOT FOUND");
        return null;
    }

    internal void FinishedSettingUpUnits()
    {
        Exploration.FinishedSettingUpUnits();
    }

    public class DialogModel
    {
        public DialogRuntime ActiveDialog;
        public DialogState dialogState;
        public int? pickedOption;

        public bool ShouldShow => dialogState == DialogState.ACTIVE;

        public void ShowDialog(DialogRuntime dialogRuntime)
        {
            ActiveDialog = dialogRuntime;
            dialogState = DialogModel.DialogState.ACTIVE;
            pickedOption = null;
        }

        public void ManualUpdate()
        {
            if (pickedOption.HasValue)
            {
                switch (dialogState)
                {
                    case DialogState.INACTIVE:
                        break;
                    case DialogState.ACTIVE:
                        dialogState = DialogState.RESULT_HAPPENED_THIS_FRAME;
                        break;
                    case DialogState.RESULT_HAPPENED_THIS_FRAME:
                        pickedOption = null;
                        dialogState = DialogState.INACTIVE;
                        break;
                    default:
                        break;
                }
            }
        }

        public void DialogComplete(int pickedOption)
        {
            // only goes to result complete on next frame
            this.pickedOption = pickedOption;
        }

        internal bool HasResult(out int option)
        {
            option = pickedOption.HasValue ? pickedOption.Value : -1;
            return this.dialogState == DialogState.RESULT_HAPPENED_THIS_FRAME;
        }

        public enum DialogState
        {
            INACTIVE,
            ACTIVE,
            RESULT_HAPPENED_THIS_FRAME,
        }
    }
}
