using System;
using UnityEngine;

public class LocationRuntime
{
    public int progress;
    public ConfigLocation configLocation;
    public RuntimeUnit RuntimeUnit;

    public LocationRuntime(RuntimeUnit ru, ConfigLocation cl)
    {
        this.RuntimeUnit = ru;
        this.RuntimeUnit.Location = this;
        configLocation = cl;
    }

    public float ProgressRatio => progress / configLocation.Length;


    /*
    internal void Load(LocationProgress locationP)
    {

    }
    */
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

    public float XPRatio => xp / (float) GetMaxXP();

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
        float MaxXPForLevel = 50 * Mathf.Pow(1.35f, RuntimeUnit.Value + skillData.LearningDifficultyLevel);
        return Mathf.FloorToInt(MaxXPForLevel);
    }

    internal void StudySkillTick()
    {
        xp += 1;
    }

    internal void Load(ArcaniaSkillPersistence skill)
    {
        xp = skill.xp;
        _acquired = skill.acquired;
    }
}
