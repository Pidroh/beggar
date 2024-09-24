using HeartUnity;
using System;
using System.Collections.Generic;

public class ArcaniaPersistence
{
    public SaveDataUnit<ArcaniaPersistenceData> saveUnit = new("maindata");

    public void Save(ArcaniaUnits units) 
    {
        ArcaniaPersistenceData apd = new();
        foreach (var u in units.datas)
        {
            foreach (var unit in u.Value)
            {
                apd.Basics.Add(new ArcaniaBasicPersistence()
                {
                    id = unit.ConfigBasic.Id,
                    requireMet = unit.RequireMet,
                    value = unit._value
                });
                if (unit.ConfigTask != null) {
                    apd.Tasks.Add(new ArcaniaTaskPersistence() {
                        id = unit.ConfigBasic.Id,
                        TaskProgress = unit.TaskProgress
                    });
                }
                if (unit.Skill != null) 
                {
                    apd.Skills.Add(new ArcaniaSkillPersistence() {
                        id = unit.ConfigBasic.Id,
                        xp = unit.Skill.xp
                    });
                }
            }
        }
        saveUnit.Save(apd);
    }

    internal void Load(ArcaniaUnits arcaniaUnits)
    {
        if (!saveUnit.TryLoad(out var persistence)) return;
        foreach (var basic in persistence.Basics)
        {
            if (!arcaniaUnits.IdMapper.TryGetValue(basic.id, out var v)) continue;
            v.RuntimeUnit._value = basic.value;
            v.RuntimeUnit.RequireMet = basic.requireMet;
        }
        foreach (var task in persistence.Tasks)
        {
            if (!arcaniaUnits.IdMapper.TryGetValue(task.id, out var v)) continue;
            if (v.RuntimeUnit.ConfigTask == null) continue;
            v.RuntimeUnit.TaskProgress = task.TaskProgress;
        }
        foreach (var skill in persistence.Skills)
        {
            if (!arcaniaUnits.IdMapper.TryGetValue(skill.id, out var v)) continue;
            if (v.RuntimeUnit.Skill == null) continue;
            v.RuntimeUnit.Skill.xp = skill.xp;
        }

    }
}

[Serializable]
public class ArcaniaPersistenceData 
{
    public List<ArcaniaBasicPersistence> Basics = new();
    public List<ArcaniaTaskPersistence> Tasks = new();
    public List<ArcaniaSkillPersistence> Skills = new();
}

[Serializable]
public class ArcaniaBasicPersistence
{
    public string id;
    public float value;
    public bool requireMet;
}

[Serializable]
public class ArcaniaTaskPersistence
{
    public string id;
    public float TaskProgress;
}

[Serializable]
public class ArcaniaSkillPersistence
{
    public string id;
    public int xp;
}