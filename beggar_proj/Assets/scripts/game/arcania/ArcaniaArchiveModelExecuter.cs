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

    public static void AfterModelLoadingOver(ArcaniaModel arcaniaModel, ArcaniaArchiveModelData archiveData)
    {
        #region process all model data to fit in archive
        foreach (var item in arcaniaModel.arcaniaUnits.datas)
        {
            foreach (var u in item.Value)
            {
                u.ForceMeetRequire();
            }
        }
        #endregion

        #region euristic calculation
        var euristicTypes = EnumHelper<ArcaniaArchiveModelData.ArchiveEuristics>.GetAllValues();
        var taskUnits = arcaniaModel.arcaniaUnits.datas[UnitType.TASK];
        foreach (var eursType in euristicTypes)
        {
            var normalEuristic = true;
            UnitType unitType = UnitType.RESOURCE;
            ArcaniaArchiveModelData.EuristicData eurData;
            bool requireMaxNotNormal = false;
            bool noMaxNotNormal = false;
            switch (eursType)
            {
                case ArcaniaArchiveModelData.ArchiveEuristics.Tasks:
                    noMaxNotNormal = true;
                    unitType = UnitType.TASK;
                    break;
                case ArcaniaArchiveModelData.ArchiveEuristics.Powerups:
                    requireMaxNotNormal = true;
                    unitType = UnitType.TASK;
                    break;
                case ArcaniaArchiveModelData.ArchiveEuristics.Resources:
                    {
                        unitType = UnitType.RESOURCE;
                    }
                    break;
                case ArcaniaArchiveModelData.ArchiveEuristics.Skills:
                    unitType = UnitType.SKILL;
                    break;
                case ArcaniaArchiveModelData.ArchiveEuristics.Houses:
                    unitType = UnitType.HOUSE;
                    break;
                case ArcaniaArchiveModelData.ArchiveEuristics.Furnitures:
                    unitType = UnitType.FURNITURE;
                    break;
                case ArcaniaArchiveModelData.ArchiveEuristics.Locations:
                    unitType = UnitType.LOCATION;
                    break;
                case ArcaniaArchiveModelData.ArchiveEuristics.Encounters:
                    unitType = UnitType.ENCOUNTER;
                    break;
                default:
                    break;
            }
            if (normalEuristic)
            {
                eurData = CalculateEuristicUnitTypeFull(unitType, arcaniaModel, archiveData, eursType);
            }
            else 
            {
                var units = arcaniaModel.arcaniaUnits.datas[unitType];
                var total = 0;
                var seen = 0;
                foreach (var unit in units)
                {
                    if (unit.HasMax && noMaxNotNormal) continue;
                    if (!unit.HasMax && requireMaxNotNormal) continue;
                    total++;
                    if (archiveData.knownIds.Contains(unit.ConfigBasic.Id))
                    {
                        seen++;
                    }
                }
                eurData =  new ArcaniaArchiveModelData.EuristicData(eursType, seen, total);
            }
            archiveData.euristicDatas.Add(eurData);
        }
        #endregion


    }

    private static ArcaniaArchiveModelData.EuristicData CalculateEuristicUnitTypeFull(UnitType type, ArcaniaModel arcaniaModel, ArcaniaArchiveModelData archiveData, ArcaniaArchiveModelData.ArchiveEuristics eurType)
    {
        var units = arcaniaModel.arcaniaUnits.datas[type];
        var total = units.Count;
        var seen = 0;
        foreach (var unit in units)
        {
            if (archiveData.knownIds.Contains(unit.ConfigBasic.Id)) 
            {
                seen++;
            }
        }
        return new ArcaniaArchiveModelData.EuristicData(eurType, seen, total);
    }
}
