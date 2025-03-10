using JLayout;
using System.Collections.Generic;

public class JRTControlUnit
{
    public JButtonAccessor MainExecuteButton { get; internal set; }
    public JButtonAccessor ExpandButton { get; internal set; }
}

public class JTabControlUnit
{
    public Dictionary<UnitType, List<JRTControlUnit>> UnitGroupControls = new();
}
