using HeartUnity;
using JLayout;
using System.Collections.Generic;

public class JRTControlUnit
{
    public JButtonAccessor MainExecuteButton { get; internal set; }
    public JButtonAccessor ExpandButton { get; internal set; }
    public JLayTextAccessor Description { get; internal set; }
    public AutoList<JResourceChangeGroup> ChangeGroups = new();
    public List<JLayoutRuntimeUnit> InsideExpandable = new();
    public bool TaskClicked => MainExecuteButton.ButtonClicked;

    public RuntimeUnit Data { get; internal set; }
    public bool Expanded { get; internal set; }
    public JImageAccessor ExpandButtonImage { get; internal set; }
    public JLayoutRuntimeUnit MainLayout { get; internal set; }
    public JLayoutRuntimeUnit ExpandWhenClickingLayout { get; internal set; }
}

public class JResourceChangeGroup 
{
    public AutoList<JLayoutRuntimeUnit> tripleTextViews = new();
}

public class JGameControlDataHolder
{
    public List<JTabControlUnit> TabControlUnits = new();
}

public class JTabControlUnit
{
    //public Dictionary<UnitType, List<JRTControlUnit>> UnitGroupControls = new();
    public List<JSeparatorControl> SeparatorControls = new();
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

