using System.Collections.Generic;

public class ControlExploration : ControlSubUnit
{
    public ArcaniaModelExploration modelExploration => _model.Exploration;
    public ExplorationDataHolder dataHolder = new();
    public ControlExploration(MainGameControl ctrl) : base(ctrl)
    {
    }

    public void ManualUpdate() 
    {
        dataHolder.LocationTCU.Data = modelExploration.LastActiveLocation;
        foreach (var item in dataHolder.ExplorationActiveUnits)
        {
            item.SetVisible(modelExploration.IsExplorationActive);
        }
        if (!modelExploration.IsExplorationActive) return;
        dataHolder.LocationTCU.XPGauge.SetRatio(modelExploration.ExplorationRatio);
        dataHolder.EncounterTCU.XPGauge.SetRatio(modelExploration.EncounterRatio);
        foreach (var item in dataHolder.ExplorationActiveUnits)
        {
            item.lwe?.ManualUpdate();
            item.XPGauge?.ManualUpdate();
        }
        // dataHolder.LocationTCU.ManualUpdate();
        // dataHolder.EncounterTCU.ManualUpdate();
    }
}

public class ExplorationDataHolder
{
    public RTControlUnit LocationTCU { get; internal set; }
    public RTControlUnit EncounterTCU { get; internal set; }
    public List<RTControlUnit> ExplorationActiveUnits = new();

    public void FinishSetup() 
    {
        ExplorationActiveUnits.Add(LocationTCU);
        ExplorationActiveUnits.Add(EncounterTCU);
    }
}
