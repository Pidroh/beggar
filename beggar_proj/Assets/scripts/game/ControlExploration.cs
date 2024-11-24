public class ControlExploration : ControlSubUnit
{
    public ControlView controlView = new();
    public ControlExploration(MainGameControl ctrl) : base(ctrl)
    {
    }

    public void ManualUpdate() 
    {
        controlView.LocationTCU.ManualUpdate();
        controlView.EncounterTCU.ManualUpdate();
    }
}

public class ControlView
{
    public RTControlUnit LocationTCU { get; internal set; }
    public RTControlUnit EncounterTCU { get; internal set; }
}
