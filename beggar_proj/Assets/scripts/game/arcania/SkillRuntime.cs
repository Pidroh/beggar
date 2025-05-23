﻿using System;
using UnityEngine;

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

    public float XPRatio => GetXpRatio();

    private float GetXpRatio()
    {
        int max = GetMaxXP();
        return Mathf.Min(xp / (float)max, 1f);
    }

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
