using HeartUnity;
using HeartUnity.View;
using JLayout;
using System;
using System.Collections.Generic;

public class JRTControlUnit
{
    public JButtonAccessor MainExecuteButton { get; internal set; }
    public JButtonAccessor ExpandButton { get; internal set; }
    public JLayTextAccessor Name { get; internal set; }
    public JLayTextAccessor Description { get; internal set; }
    public AutoList<JResourceChangeGroup> ChangeGroups = new();
    public List<JLayoutRuntimeUnit> InsideExpandable = new();
    public bool TaskClicked => MainExecuteButton.ButtonClicked;

    public RuntimeUnit Data { get; internal set; }
    public bool Expanded { get; internal set; }
    public JImageAccessor ExpandButtonImage { get; internal set; }
    public JLayoutRuntimeUnit MainLayout { get; internal set; }
    public JLayoutRuntimeUnit ExpandWhenClickingLayout { get; internal set; }
    public JLayTextAccessor ValueText { get; internal set; }
    public JLayoutRuntimeUnit GaugeLayout { get; internal set; }
    public JImageAccessor GaugeProgressImage { get; internal set; }
    public JLayoutRuntimeUnit PlusMinusLayout { get; internal set; }
    public JLayoutRuntimeUnit TitleWithValue { get; internal set; }
    public JLayTextAccessor TaskQuantityText { get; internal set; }

    public JRTControlUnitMods OwnedMods = new();
    public JRTControlUnitMods IntermediaryMods = new();
    public JRTControlUnitMods TargetingThisMods = new();
}

public class JRTControlUnitMods
{
    public JLayoutRuntimeUnit Header { get; internal set; }
    public AutoList<JLayoutRuntimeUnit> tripleTextViews = new();
    public List<ModRuntime> Mods = new();
}

public class JResourceChangeGroup 
{
    public AutoList<JLayoutRuntimeUnit> tripleTextViews = new();

    public JLayoutRuntimeUnit Header { get; internal set; }
}

public class JGameControlDataHolder
{
    public List<JTabControlUnit> TabControlUnits = new();
    public Dictionary<Direction, JLayoutRuntimeUnit> tabMenu = new();
    public JGameControlDataExploration Exploration = new();

    public JLayoutRuntimeData LayoutRuntime { get; internal set; }
    public JLayoutChild DialogLayout { get; internal set; }
    public JLayoutChild EndingLayout { get; internal set; }
}

public class JGameControlDataExploration
{
    public JRTControlUnit AreaJCU { get; internal set; }
    public JRTControlUnit EncounterJCU { get; internal set; }
    public List<JLayoutRuntimeUnit> ExplorationModeLayouts = new();
    public List<JRTControlUnit> StressorJCUs = new();

    public JRTControlUnit FleeButtonJCU { get; internal set; }

}

public class JTabControlUnit
{
    //public Dictionary<UnitType, List<JRTControlUnit>> UnitGroupControls = new();
    public List<JSeparatorControl> SeparatorControls = new();

    public RuntimeUnit TabData { get; internal set; }
    public JLayoutRuntimeUnit DesktopButton { get; internal set; }
    public List<JLayoutRuntimeUnit> TabToggleButtons = new();
    public int LogAmount { get; internal set; }
    public JLayoutRuntimeUnit MobileButton { get; internal set; }
    public JLayoutRuntimeUnit SpaceShowLayout { get; internal set; }

    public class JSeparatorControl 
    {
        //public List<JRTControlUnit> ControlUnits = new();
        public Dictionary<UnitType, List<JRTControlUnit>> UnitGroupControls = new();

        public JSeparatorControl(TabRuntime.Separator sepD)
        {
            SepD = sepD;
        }

        public TabRuntime.Separator SepD { get; }
        public JLayoutRuntimeUnit SeparatorLayout { get; internal set; }
        public bool Expanded { get; internal set; } = true;
    }
}

