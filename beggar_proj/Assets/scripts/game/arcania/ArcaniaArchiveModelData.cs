using System;
using System.Collections.Generic;

// persistence data not bound to runtime units and etc, often related to a whole file
[Serializable]
public class ArcaniaMiscPersistenceData 
{
    public bool hasAccessToArchive;
}

[Serializable]
public class ArchivePersistenceData 
{
    public List<string> knownIds = new();
}

public class ArcaniaArchiveModelData 
{
    public List<string> knownIds = new();
    public bool hasAccess;
    public List<EuristicData> euristicDatas = new();

    public class EuristicData 
    {
        public readonly ArchiveEuristics EuristicType;
        public readonly int current;
        public readonly int max;

        public EuristicData(ArchiveEuristics euristicType, int current, int max)
        {
            EuristicType = euristicType;
            this.current = current;
            this.max = max;
        }
    }

    public enum ArchiveEuristics
    {
        Tasks,
        Powerups,
        Resources,
        Skills,
        Houses,
        Furnitures,
        Locations,
        Total
        // Encounters
    }
}
