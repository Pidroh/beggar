﻿using arcania;
using HeartUnity;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeUnit
{
    public ConfigBasic ConfigBasic;
    public ConfigTask ConfigTask;

    public LocationRuntime Location { get; internal set; }

    public SkillRuntime Skill;
    public List<ModRuntime> ModsTargetingSelf = new();
    public List<ModRuntime> ModsOwned = new();
    public bool RequireMet = false;

    public string Name => ConfigBasic.name;

    public int Max => CalculateMax();

    public bool Visible => RequireMet && IsPossiblyVisibleRegardlessOfRequire();

    public bool IsPossiblyVisibleRegardlessOfRequire() 
    {
        if (this.GetModSum(ModType.Lock) > 0) return false;
        if (ConfigBasic.UnitType == UnitType.TASK && IsMaxed) return false;
        return true;
    }

    private bool IsNeedMet()
    {
        return MeetsCondition(ConfigTask.Need?.expression);
    }

    internal bool UpdateRequireStatus()
    {
        if (!IsPossiblyVisibleRegardlessOfRequire()) return false;
        var requireMetBefore = RequireMet;
        // only gets checked when require has never been met before
        RequireMet = RequireMet || MeetsCondition(ConfigBasic.Require?.expression);
        if (!RequireMet) return false;
        if (requireMetBefore == false) return true;
        return false;
    }


    public TabRuntime Tab { get; internal set; }

    private bool MeetsCondition(ConditionalExpressionData expression)
    {
        if (expression == null) return true;
        if (expression is Condition cond)
        {
            float value = cond.Pointer.GetValue();
            return ConditionalExpression.Evaluate(value, cond.Value, cond.Operator);
        }
        else if (expression is LogicalExpression log)
        {
            var left = MeetsCondition(log.Left);
            if (left && log.Operator == ComparisonOperator.Or) return true;
            if (!left && log.Operator == ComparisonOperator.And) return false;
            return MeetsCondition(log.Right);
        }
        // force error
        string s = null;
        return s.Length == 0;
    }

    private int CalculateMax()
    {
        // has no max from the get go
        if (ConfigBasic.Max < 0) return -1;
        var sum = Mathf.FloorToInt(GetModSum(modType: ModType.MaxChange));
        return Mathf.Max(ConfigBasic.Max + sum, 0);
    }

    internal void ChangeValue(float valueChange)
    {
        _value = Mathf.Clamp(_value + valueChange, 0, MaxForCeiling);
    }

    public void ModifyValue(float valueChange) => ChangeValue(valueChange);

    internal void SetValue(int v)
    {
        ChangeValue(v - _value);
    }

    internal void ChangeValueByResourceChange(RuntimeUnit parent, FloatRange valueChange, ResourceChangeType changeType)
    {
        var modV = GetModSumWithIntermediaryCheck(parent, modType: ModType.ResourceChangeChanger, changeType);
        ChangeValue(valueChange.getValue(Random.Range(0f, 1f)) + modV);
    }

    public float GetModSumWithIntermediaryCheck(RuntimeUnit intermediary, ModType modType, ResourceChangeType changeType)
    {
        var v = 0f;
        foreach (var mod in ModsTargetingSelf)
        {
            if (mod.ModType != modType) continue;
            if (mod.ResourceChangeType != changeType) continue;
            if (mod.Intermediary == null) Debug.LogError("There should never be a resource change type mod without intermediary");
            if (mod.Intermediary.RuntimeUnit != intermediary) continue;
            v += mod.Source.Value * mod.Value;
        }
        return v;
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
        if (Location != null) 
        {
            // Location completition is done through Arcania model Exploration
            return false;
        }
        if (Skill != null)
        {
            return Skill.HasEnoughXPToLevelUp();
        }
        if (ConfigTask.Duration.HasValue) return TaskProgress > ConfigTask.Duration.Value;
        return false;
    }

    internal void ForceMeetRequire()
    {
        RequireMet = true;
    }

    internal void ApplyRate()
    {
        var rate = this.GetModSum(ModType.RateChange);
        ChangeValue(rate);
    }

    internal bool IsInstant() => !ConfigTask.Duration.HasValue;

    internal bool CanFullyAcceptChange(FloatRange valueChange)
    {
        if (valueChange.SmallerThan(0f)) return valueChange.BiggerOrEqual(-Value); //return Mathf.Abs(valueChange) <= Value;
        // no max
        if (Max < 0) return true;
        if (valueChange.BiggerThan(0f))
        {
            if (Max == 0) return false;
            int ValueToReachMax = Max - Value;
            return valueChange.SmallerOrEqual(ValueToReachMax);
        }
        return true;
    }

    internal void RegisterModTargetingSelf(ModRuntime modData)
    {
        ModsTargetingSelf.Add(modData);
    }

    public int Value => Mathf.FloorToInt(_value);
    public int MaxForCeiling => Max < 0 ? int.MaxValue : Max;
    public float _value;

    public float TaskProgress { get; internal set; }
    public float TaskProgressRatio => Skill != null ? TaskProgress : (!ConfigTask.Duration.HasValue ? 0f : TaskProgress / ConfigTask.Duration.Value);
    public bool IsMaxed => Value >= MaxForCeiling;

    public bool IsZero => Value == 0;

    public ConfigHouse ConfigHouse { get; internal set; }
    public ConfigFurniture ConfigFurniture { get; internal set; }
    public bool HasMax => CalculateMax() >= 0;

    public bool IsTaskHalfWay => !IsTaskComplete() && TaskProgress != 0;

    public bool NeedMet => IsNeedMet();

    public ConfigResource ConfigResource { get; internal set; }
    public ConfigEncounter ConfigEncounter { get; internal set; }
}
