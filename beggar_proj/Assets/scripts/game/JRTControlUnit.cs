using JLayout;
using System.Collections.Generic;

public static class JGameControlExecuter 
{
    public static void ManualUpdate(MainGameControl mgc, JGameControlDataHolder controlData, float dt) 
    {
        var arcaniaModel = mgc.arcaniaModel;
        foreach (var tabControl in controlData.TabControlUnits)
        {
            foreach (var pair in tabControl.UnitGroupControls)
            {
                foreach (var unit in pair.Value)
                {
                    switch (pair.Key)
                    {
                        case UnitType.RESOURCE:
                            break;
                        case UnitType.TASK:
                            {
                                // tcu.bwe.MainButtonEnabled = arcaniaModel.Runner.CanStartAction(data);
                                // tcu.bwe.MainButtonSelected(arcaniaModel.Runner.RunningTasks.Contains(data));
                                if (unit.TaskClicked)
                                {
                                    arcaniaModel.Runner.StartActionExternally(unit.Data);
                                }
                            }
                            break;
                        case UnitType.HOUSE:
                            break;
                        case UnitType.CLASS:
                            break;
                        case UnitType.SKILL:
                            break;
                        case UnitType.FURNITURE:
                            break;
                        case UnitType.TAB:
                            break;
                        case UnitType.DIALOG:
                            break;
                        case UnitType.LOCATION:
                            break;
                        case UnitType.ENCOUNTER:
                            break;
                        default:
                            break;
                    }

                }
            }
        }
    }
}

public class JRTControlUnit
{
    public JButtonAccessor MainExecuteButton { get; internal set; }
    public JButtonAccessor ExpandButton { get; internal set; }
    public JLayTextAccessor Description { get; internal set; }
    public bool TaskClicked => MainExecuteButton.ButtonClicked;

    public RuntimeUnit Data { get; internal set; }
}

public class JGameControlDataHolder
{
    public List<JTabControlUnit> TabControlUnits = new();
}

public class JTabControlUnit
{
    public Dictionary<UnitType, List<JRTControlUnit>> UnitGroupControls = new();
}
