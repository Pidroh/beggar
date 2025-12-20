using arcania;
using HeartEngineCore;
using System.Collections.Generic;

public class RuntimeUnit
{
    public ConfigBasic ConfigBasic;
    public ConfigTask ConfigTask;

    public LocationRuntime Location { get; set; }

    public SkillRuntime Skill;
    public List<ModRuntime> ModsTargetingSelf = new();
    public List<ModRuntime> ModsSelfAsIntermediary = new();
    public List<ModRuntime> ModsOwned = new();
    public BuyStatus BuyStatus = BuyStatus.Free;
    public UnlockNotification UnlockNotification = UnlockNotification.Locked;
    public bool RequireMet = false;
    public int Value => MathfHG.FloorToInt(_value);
    public int MaxForCeiling => Max < 0 ? int.MaxValue : Max;
    public float _value;
    

    public float TaskProgress { get; set; }
    public float TaskProgressRatio => CalculateTaskProgressRatio();

    // Skills cannot drop below max
    public bool MaxCanLimitValue => Skill == null && !ConfigBasic.AboveMax;

    public string Name => ArchiveEnabled ?? true ? ConfigBasic.name : "?????";

    public int Max => CalculateMax();

    public bool Visible => ParentRU?.Visible ?? (RequireMet && IsPossiblyVisibleRegardlessOfRequire());


    public bool IsPossiblyVisibleRegardlessOfRequire()
    {
        if (ForceInvisible) return false;
        if (IsLocked()) return false;
        if (ConfigBasic.UnitType == UnitType.TASK && IsMaxed) return false;
        if (Activatable && !HasModActive(ModType.Activate)) return false;
        // if it's a hint and the hinted thing is already visible, the hint isn't visible
        var hintTargetUnit = ConfigHintData?.hintTargetPointer?.RuntimeUnit ?? null;
        if (hintTargetUnit != null && (hintTargetUnit.Visible || hintTargetUnit.IsLocked())) return false;
        return true;
    }

    private bool IsLocked()
    {
        return this.HasModActive(ModType.Lock);
    }

    private bool IsNeedMet()
    {
        return MeetsCondition(ConfigTask.Need?.expression);
    }

    public bool UpdateRequireStatus()
    {
        if (!IsPossiblyVisibleRegardlessOfRequire()) return false;
        var requireMetBefore = RequireMet;
        // only gets checked when require has never been met before
        RequireMet = RequireMet || MeetsCondition(ConfigBasic.Require?.expression);
        if (!RequireMet) return false;
        if (requireMetBefore == false) return true;
        return false;
    }


    public TabRuntime Tab { get;  set; }

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

    internal void MarkSelfDirty()
    {
        Dirty = true;
    }

    private int CalculateMax()
    {
        // has no max from the get go
        if (ConfigBasic.Max < 0) return -1;
        var sum = MathfHG.FloorToInt(GetModSum(modType: ModType.MaxChange));
        return MathfHG.Max(ConfigBasic.Max + sum, 0);
    }

    public void ChangeValue(float valueChange)
    {
        var valueWasZero = Value == 0;
        if (MaxCanLimitValue)
            _value = MathfHG.Clamp(_value + valueChange, 0, MaxForCeiling);
        else
            _value = MathfHG.Max(_value + valueChange, 0);
        if (valueWasZero && Value != 0) 
        {
            DirtyThingsWhenNotZero();
        }

    }

    private void DirtyThingsWhenNotZero()
    {
        if (Dirty) return;
        Dirty = true;
        foreach (var mod in this.ModsOwned)
        {
            if (mod.Intermediary != null)
            {
                foreach (var ru in mod.Intermediary.RuntimeUnits)
                {
                    ru.DirtyThingsWhenNotZero();
                }
            }
            // targets will be done above if there is an intermediary
            else if (mod.Target != null)
            {
                foreach (var ru in mod.Target.RuntimeUnits)
                {
                    ru.DirtyThingsWhenNotZero();
                }

            }

        }
        foreach (var mod in this.ModsSelfAsIntermediary)
        {
            if (mod.Target != null)
            {
                foreach (var ru in mod.Target.RuntimeUnits)
                {
                    ru.DirtyThingsWhenNotZero();
                }
            }
        }
    }

    public void ModifyValue(float valueChange) => ChangeValue(valueChange);

    public void SetValue(int v)
    {
        ChangeValue(v - _value);
    }

    public void ChangeValueByResourceChange(RuntimeUnit parent, FloatRangePure valueChange, ResourceChangeType changeType)
    {
        // var modV = GetModSumWithIntermediaryCheck(parent, modType: ModType.ResourceChangeChanger, changeType);
        ChangeValue(valueChange.getValue(RandomHG.Range(0f, 1f)));
    }

    public float GetModSumWithIntermediaryCheck(RuntimeUnit intermediary, ModType modType, ResourceChangeType changeType)
    {
        var v = 0f;
        foreach (var mod in ModsTargetingSelf)
        {
            if (mod.ModType != modType) continue;
            if (mod.ResourceChangeType != changeType) continue;
            if (mod.Intermediary == null) Logger.LogError("There should never be a resource change type mod without intermediary");
            if (mod.Intermediary.RuntimeUnit != intermediary) continue;
            v += mod.Source.Value * mod.Value;
        }
        return v;
    }

    public bool HasModActive(ModType modType) 
    {
        foreach (var mod in ModsTargetingSelf)
        {
            if (mod.ModType != modType) continue;
            if (mod.Source.Value > 0) return true;
        }
        return false;
    }

    private float GetModSum(ModType modType)
    {
        var v = 0f;
        foreach (var mod in ModsTargetingSelf)
        {
            if (mod.ModType != modType) continue;
            float value = mod.Source.Value * mod.Value;
            if (mod.Intermediary != null) 
            {
                value *= mod.Intermediary.GetValue();
            }
            v += value;
        }
        return v;
    }

    public bool IsTaskComplete()
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
        if (ConfigTask.Duration.HasValue) return TaskProgress >= ConfigTask.Duration.Value;
        return false;
    }

    public void ForceMeetRequire()
    {
        RequireMet = true;
    }

    public void ApplyRate()
    {
        var rate = this.GetModSum(ModType.RateChange);
        ChangeValue(rate);
    }

    public bool IsInstant() => !ConfigTask.Duration.HasValue;

    public float GetSpeedMultiplier()
    {
        var speedP = 100f;
        speedP += GetModSum(ModType.Speed);
        return speedP * 0.01f;
    }

    public bool CanFullyAcceptChange(FloatRangePure valueChange)
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

    public void RegisterModWithSelfAsIntermediary(ModRuntime mod)
    {
        ModsSelfAsIntermediary.Add(mod);
    }

    public void RegisterModTargetingSelf(ModRuntime modData)
    {
        ModsTargetingSelf.Add(modData);
    }



    private float CalculateTaskProgressRatio()
    {
        if (Skill != null) return TaskProgress;
        if (DotConfig != null) return  1f - (TaskProgress / DotConfig.Duration);
        return (!ConfigTask.Duration.HasValue ? 0f : TaskProgress / ConfigTask.Duration.Value);
    }

    public bool IsMaxed => Value >= MaxForCeiling;

    public bool IsZero => Value == 0;

    public ConfigHouse ConfigHouse { get; set; }
    public ConfigFurniture ConfigFurniture { get; set; }
    public bool HasMax => CalculateMax() >= 0;

    public bool IsTaskHalfWay => !IsTaskComplete() && TaskProgress != 0;

    public bool NeedMet => IsNeedMet();

    public ConfigResource ConfigResource { get; set; }
    public ConfigHint ConfigHintData { get; set; }
    public ConfigEncounter ConfigEncounter { get; set; }
    public float ValueRatio => HasMax ? _value / Max : 0f;

    public bool Dirty { get; private set; }
    public int SuccessRatePercent => ConfigTask?.SuccessRatePercent != null ? (int) (ConfigTask.SuccessRatePercent.Value + GetModSum(ModType.SuccessRate)) : 100;

    public RuntimeUnit DotRU { get; set; }
    public DotConfig DotConfig { get; set; }
    public RuntimeUnit ParentRU { get; set; }
    public bool Activatable { get; set; }
    public bool? ArchiveEnabled { get; set; }
    public WorldType World { get; set; }
    public bool ForceInvisible { get; set; }
}
