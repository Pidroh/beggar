using System;

public static class ArcaniaArchiveModelExecuter
{

    public static void LoadUpArchive(ArcaniaArchiveModelData archiveData, ArcaniaPersistence arcaniaPersistence)
    {
        if (!arcaniaPersistence.saveUnit.TryLoad(out var rawData)) return;
        foreach (var item in rawData.Basics)
        {
            if (!item.requireMet) continue;
            if (archiveData.knownIds.Contains(item.id)) continue;
            archiveData.knownIds.Add(item.id);
        }
        
    }

    public static void AfterModelLoadingOver(ArcaniaModel arcaniaModel)
    {
        foreach (var item in arcaniaModel.arcaniaUnits.datas)
        {
            foreach (var u in item.Value)
            {
                u.ForceMeetRequire();
            }
        }
    }
}
