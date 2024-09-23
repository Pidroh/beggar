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