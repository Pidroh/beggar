using System.Collections.Generic;

public class TabRuntime
{

    public List<UnitType> AcceptedUnitTypes = new();
    public List<Separator> Separators = new();

    public TabRuntime(RuntimeUnit ru)
    {
        this.RuntimeUnit = ru;
        this.RuntimeUnit.Tab = this;
    }

    public RuntimeUnit RuntimeUnit { get; }
    public bool ContainsLogs { get; internal set; }
    public bool OpenSettings { get; internal set; }
    public bool OpenOtherTabs { get; internal set; }
    public bool ExplorationActiveTab { get; internal set; }
    public bool NecessaryForDesktopAndThinnable { get; internal set; }

    public class Separator {
        public List<UnitType> AcceptedUnitTypes = new();
        public List<RuntimeUnit> BoundRuntimeUnits = new();
        public bool RequireMax;
        public bool RequireInstant;

        public string Name { get; internal set; }
        public bool ShowSpace { get; internal set; }
        public string Id { get; internal set; }
        public List<IDPointer> Tags { get; internal set; }
        public int Priority { get; internal set; }
        public bool ContainsSaveSlots { get; internal set; }
    }
}
