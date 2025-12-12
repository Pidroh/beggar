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
    public bool ContainsLogs { get; set; }
    public bool OpenSettings { get; set; }
    public bool OpenOtherTabs { get; set; }
    public bool ExplorationActiveTab { get; set; }
    public bool NecessaryForDesktopAndThinnable { get; set; }
    public bool ArchiveOnly { get; set; }
    public bool DisableOnArchive { get; set; }

    public class Separator {
        public List<UnitType> AcceptedUnitTypes = new();
        public List<RuntimeUnit> BoundRuntimeUnits = new();
        public bool RequireMax;
        public bool RequireInstant;

        public string Name { get; set; }
        public bool ShowSpace { get; set; }
        public string Id { get; set; }
        public List<IDPointer> Tags { get; set; }
        public int Priority { get; set; }
        public bool ContainsSaveSlots { get; set; }
        public bool ArchiveMainUI { get; set; }
    }
}
