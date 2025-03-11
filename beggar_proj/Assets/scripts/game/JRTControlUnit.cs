using HeartUnity;
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
                    UpdateChangeGroups(unit);
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

    public static void UpdateChangeGroups(JRTControlUnit unit)
    {
        // var Dirty = unit.Dirty;
        var Dirty = 1;
        var Data = unit.Data;
        var ChangeGroups = unit.ChangeGroups;
        if (Data == null) return;
        for (int i = 0; i < ChangeGroups.Count; i++)
        {
            ResourceChangeType resourceChangeType = (ResourceChangeType)i;
            //var sep = ChangeGroupSeparators[i];

            var item = ChangeGroups[i];
            var resourceChanges = Data.ConfigTask.GetResourceChangeList(i);
            if (item == null || (Data.Skill != null && Data.Skill.Acquired && resourceChangeType == ResourceChangeType.COST))
            {
                // if (sep != null) sep.LayoutChild.VisibleSelf = false;
                for (int ttvIndex = 0; ttvIndex < item.tripleTextViews.Count; ttvIndex++)
                {
                    var ttv = item.tripleTextViews[ttvIndex];
                    
                    // ttv.LayoutChild.VisibleSelf = false;
                }
                continue;
            }


            //if (sep != null) sep.ManualUpdate();
            //sep.LayoutChild.VisibleSelf = resourceChanges.Count > 0;
            var bySecond = i == (int)ResourceChangeType.EFFECT || i == (int)ResourceChangeType.RUN;

            for (int ttvIndex = 0; ttvIndex < item.tripleTextViews.Count; ttvIndex++)
            {
                var ttv = item.tripleTextViews[ttvIndex];
                //ttv.Visible = resourceChanges.Count > ttvIndex;
                //if (!ttv.Visible) continue;
                var rc = resourceChanges[ttvIndex];

                var min = rc.valueChange.min;
                var max = rc.valueChange.max;

                string targetName;
                string tertiaryText = "";
                if (rc.IdPointer.RuntimeUnit != null)
                {
                    RuntimeUnit dataThatWillBeChanged = rc.IdPointer.RuntimeUnit;
                    targetName = dataThatWillBeChanged.Visible ? dataThatWillBeChanged.Name : "???";
                    if (dataThatWillBeChanged.HasMax)
                        tertiaryText = $"({dataThatWillBeChanged.Value} / {dataThatWillBeChanged.Max})";
                    else
                        tertiaryText = $"({dataThatWillBeChanged.Value})";
                }
                else
                {
                    targetName = rc.IdPointer.Tag.tagName;
                }

                if (Dirty > 0)
                {
                    ttv.SetText(0, targetName);
                }

                var valueText = min != max ? $"{min}~{max}" : $"{min}";
                string secondText = bySecond ? $"{valueText}/s" : $"{valueText}";
                ttv.SetText(1, secondText);
                ttv.SetText(2, tertiaryText);
                //ttv.ManualUpdate();
            }
        }
    }
}

public class JRTControlUnit
{
    public JButtonAccessor MainExecuteButton { get; internal set; }
    public JButtonAccessor ExpandButton { get; internal set; }
    public JLayTextAccessor Description { get; internal set; }
    public AutoList<JResourceChangeGroup> ChangeGroups = new();
    public List<JLayoutRuntimeUnit> InsideExpandable = new();
    public bool TaskClicked => MainExecuteButton.ButtonClicked;

    public RuntimeUnit Data { get; internal set; }
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
    public Dictionary<UnitType, List<JRTControlUnit>> UnitGroupControls = new();
}
