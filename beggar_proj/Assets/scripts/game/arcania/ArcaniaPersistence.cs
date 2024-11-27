using HeartUnity;
using System;
using System.Collections.Generic;

public class ArcaniaPersistence
{
    public SaveDataUnit<ArcaniaPersistenceData> saveUnit;

    public ArcaniaPersistence(HeartGame hg)
    {
        saveUnit = new SaveDataUnit<ArcaniaPersistenceData>("maindata", hg);
    }

    public void Save(ArcaniaUnits units, ArcaniaModelExploration exploration)
    {
        ArcaniaPersistenceData apd = new();
        apd.Exploration.locationProgress = exploration.locationProgress;
        apd.Exploration.lastLocationID = exploration.LastActiveLocation.ConfigBasic.Id;
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
                if (unit.ConfigTask != null)
                {
                    apd.Tasks.Add(new ArcaniaTaskPersistence()
                    {
                        id = unit.ConfigBasic.Id,
                        TaskProgress = unit.TaskProgress
                    });
                }
                if (unit.Skill != null)
                {
                    apd.Skills.Add(new ArcaniaSkillPersistence()
                    {
                        id = unit.ConfigBasic.Id,
                        xp = unit.Skill.xp,
                        acquired = unit.Skill.Acquired
                    });
                }
            }
        }
        saveUnit.Save(apd);
    }

    internal void Load(ArcaniaUnits arcaniaUnits, ArcaniaModelExploration exploration)
    {
        if (!saveUnit.TryLoad(out var persistence)) return;
        exploration.locationProgress = persistence.Exploration.locationProgress;
        if (!string.IsNullOrWhiteSpace(persistence.Exploration.lastLocationID)) 
            exploration.LoadLastActiveLocation(arcaniaUnits.GetOrCreateIdPointer(persistence.Exploration.lastLocationID)?.RuntimeUnit);

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
            v.RuntimeUnit.Skill.Load(skill);
        }

    }
}

[Serializable]
public class ArcaniaPersistenceData
{
    public List<ArcaniaBasicPersistence> Basics = new();
    public List<ArcaniaTaskPersistence> Tasks = new();
    public List<ArcaniaSkillPersistence> Skills = new();
    public ArcaniaExplorationPersistence Exploration = new();
}

[Serializable]
public class ArcaniaExplorationPersistence
{
    public int locationProgress;
    public string lastLocationID;
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
    public bool acquired;
}