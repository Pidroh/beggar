using System;

public static class JGameControlExecuterExploration
{
    internal static void ManualUpdate(MainGameControl mgc, JGameControlDataHolder controlData, float dt)
    {
        bool isExplorationActive = mgc.arcaniaModel.Exploration.IsExplorationActive;
        foreach (var item in controlData.Exploration.ExplorationModeLayouts)
        {
            item.SetParentShowing(isExplorationActive);
        }
        if (isExplorationActive) 
        {
            controlData.Exploration.AreaJCU.GaugeProgressImage.SetGaugeRatio(mgc.arcaniaModel.Exploration.ExplorationRatio);
            controlData.Exploration.EncounterJCU.GaugeProgressImage.SetGaugeRatio(mgc.arcaniaModel.Exploration.EncounterRatio);
            controlData.Exploration.AreaJCU.Name.SetTextRaw(mgc.arcaniaModel.Exploration.LastActiveLocation.Name);
            controlData.Exploration.EncounterJCU.Name.SetTextRaw(mgc.arcaniaModel.Exploration.ActiveEncounter.Name);
            foreach (var item in controlData.Exploration.StressorJCUs)
            {
                item.GaugeProgressImage.SetGaugeRatio(item.Data.ValueRatio);
            }
        }
    }
}
