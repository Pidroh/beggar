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
        dataHolder.LocationRCU.Data = modelExploration.LastActiveLocation;
        dataHolder.EncounterRCU.Data = modelExploration.ActiveEncounter;
        foreach (var item in dataHolder.ExplorationActiveUnits)
        {
            item.SetVisible(modelExploration.IsExplorationActive);
        }
        if (!modelExploration.IsExplorationActive) return;
        // dataHolder.LocationRCU.lwe.MainText.rawText = modelExploration.LastActiveLocation.ConfigBasic.name;
        // dataHolder.EncounterRCU.lwe.MainText.rawText = modelExploration.ActiveEncounter.ConfigBasic.name;
        foreach (var rcuStress in dataHolder.StressorsRCU)
        {
            rcuStress.XPGauge.SetRatio(rcuStress.Data.ValueRatio);
        }
        dataHolder.LocationRCU.XPGauge.SetRatio(modelExploration.ExplorationRatio);
        dataHolder.EncounterRCU.XPGauge.SetRatio(modelExploration.EncounterRatio);
        if (dataHolder.FleeRCU.TaskClicked) 
        {
            _model.Exploration.Flee();
        }
        foreach (var item in dataHolder.ExplorationActiveUnits)
        {
            item.lwe?.ManualUpdate();
            item.XPGauge?.ManualUpdate();
            item.bwe?.ManualUpdate();
            item.FeedDescription();
            if(item.IsExpanded) item.UpdateChangeGroups();
            if (item.Data == null) continue;
            item.lwe.MainText.rawText = item.Data.ConfigBasic.name;
        }
        // dataHolder.LocationTCU.ManualUpdate();
        // dataHolder.EncounterTCU.ManualUpdate();
    }
}

public class ExplorationDataHolder
{
    public RTControlUnit LocationRCU { get; internal set; }
    public RTControlUnit EncounterRCU { get; internal set; }
    public RTControlUnit FleeRCU { get; internal set; }
    public List<RTControlUnit> StressorsRCU { get; internal set; } = new();

    public List<RTControlUnit> ExplorationActiveUnits = new();

    public void FinishSetup() 
    {
        ExplorationActiveUnits.Add(LocationRCU);
        ExplorationActiveUnits.Add(EncounterRCU);
        ExplorationActiveUnits.AddRange(StressorsRCU);
        ExplorationActiveUnits.Add(FleeRCU);
    }
}
