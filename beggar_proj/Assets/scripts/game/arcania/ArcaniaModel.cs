using System;
using System.Collections.Generic;
using UnityEngine;

public class ConfigSkill
{
    public int LearningDifficultyLevel;
}

public class ConfigHouse 
{
    public int AvailableSpace;
}

public class SkillRuntime
{
    public int xp;
    public ConfigSkill skillData;
    public RuntimeUnit RuntimeUnit;
    private bool _acquired;

    public SkillRuntime(RuntimeUnit ru)
    {
        this.RuntimeUnit = ru;
        this.RuntimeUnit.Skill = this;
        skillData = new ConfigSkill();
    }

    public bool Acquired => _acquired;

    internal void Acquire()
    {
        _acquired = true;
    }

    internal bool HasEnoughXPToLevelUp()
    {
        return xp >= GetMaxXP();
    }

    private int GetMaxXP()
    {
        return Mathf.FloorToInt(50 * Mathf.Pow(1.35f, RuntimeUnit.Value + skillData.LearningDifficultyLevel));
    }

    internal void StudySkillTick()
    {
        xp += 1;
    }
}

public class ArcaniaModel
{
    public ArcaniaUnits arcaniaUnits = new ArcaniaUnits();
    public ArcaniaModelActionRunner Runner;
    float _oneSecondCounter;

    public ArcaniaModel()
    {
        Runner = new(this);
    }

    public bool CanAcquireSkill(RuntimeUnit ru)
    {
        return CanAfford(ru.ConfigTask.Cost) && !ru.Skill.Acquired;
    }

    public void AcquireSkill(RuntimeUnit ru) {
        ApplyResourceChanges(ru, ResourceChangeType.COST);
        ru.Skill.Acquire();
    }

    internal void ApplyResourceChanges(RuntimeUnit parent, ResourceChangeType changeType)
    {
        var changes = parent.ConfigTask.GetResourceChangeList(changeType);
        foreach (var c in changes)
        {
            c.IdPointer.RuntimeUnit.ChangeValueByResourceChange(parent, c.valueChange, changeType);
        }
    }

    private void ChangeValue(RuntimeUnit runtimeUnit, int valueChange)
    {
        runtimeUnit.ChangeValue(valueChange);
    }

    public void ManualUpdate(float dt)
    {
        Runner.ManualUpdate(dt);
        _oneSecondCounter += dt;
        while (_oneSecondCounter > 1f) 
        {
            _oneSecondCounter -= 1f;
            foreach (var pair in arcaniaUnits.datas)
            {
                foreach (var item in pair.Value)
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
            if (rc.valueChange == 0) continue;
            if (rc.IdPointer.RuntimeUnit.CanFullyAcceptChange(rc.valueChange)) continue;
            return false;
        }
        return true;
    }

    public bool DoChangesMakeADifference(List<ResourceChange> changes)
    {
        foreach (var rc in changes)
        {
            if (rc.valueChange == 0) continue;
            if (rc.valueChange > 0 && rc.IdPointer.RuntimeUnit.IsMaxed) continue;
            if (rc.valueChange < 0 && rc.IdPointer.RuntimeUnit.IsZero) continue;
            return true;
        }
        return false;

    }


}
