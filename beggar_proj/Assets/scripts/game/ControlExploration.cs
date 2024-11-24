using System.Collections.Generic;

public class ControlExploration : ControlSubUnit
{
    public ArcaniaModelExploration expo => _model.Exploration;
    public ExplorationDataHolder dataHolder = new();
    public ControlExploration(MainGameControl ctrl) : base(ctrl)
    {
    }

    public void ManualUpdate() 
    {
        dataHolder.LocationTCU.Data = expo.LastActiveLocation;
        dataHolder.LocationTCU.ManualUpdate();
        dataHolder.EncounterTCU.ManualUpdate();
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
