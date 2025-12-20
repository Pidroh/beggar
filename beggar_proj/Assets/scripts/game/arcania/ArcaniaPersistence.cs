using System;
using System.Collections.Generic;

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
    public int unlockStatus;
}

[Serializable]
public class ArcaniaTaskPersistence
{
    public string id;
    public float TaskProgress;
    public bool Bought;
}

[Serializable]
public class ArcaniaSkillPersistence
{
    public string id;
    public int xp;
    public bool acquired;
}