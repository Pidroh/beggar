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
            JGameControlDataExploration exploration = controlData.Exploration;
            exploration.AreaJCU.GaugeProgressImage.SetGaugeRatio(mgc.arcaniaModel.Exploration.ExplorationRatio);
            exploration.EncounterJCU.GaugeProgressImage.SetGaugeRatio(mgc.arcaniaModel.Exploration.EncounterRatio);
            for (int i = 0; i < 2; i++)
            {
                var data = i == 0 ? mgc.arcaniaModel.Exploration.LastActiveLocation : mgc.arcaniaModel.Exploration.ActiveEncounter;
                var jCU = i == 0 ? exploration.AreaJCU : exploration.EncounterJCU;

                jCU.Name.SetTextRaw(data.Name);
                MainGameControlSetupJLayout.EnsureChangeListViewsAreCreated(controlData.LayoutRuntime, data, jCU, jCU.MainLayout);
                jCU.Data = data;
                JGameControlExecuter.UpdateChangeGroups(jCU);
                JGameControlExecuter.UpdateExpandLogicForUnit(jCU);
            }
            
            // exploration.FleeButtonJCU.GaugeProgressImage.SetGaugeRatio(0);
            foreach (var item in exploration.StressorJCUs)
            {
                item.GaugeProgressImage.SetGaugeRatio(item.Data.ValueRatio);
            }
            if (exploration.FleeButtonJCU.TaskClicked) 
            {
                mgc.arcaniaModel.Exploration.Flee();
            }
        }
    }
}
