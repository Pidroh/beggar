public static class JGameControlExecuter 
{
    public static void ManualUpdate(MainGameControl mgc, JGameControlDataHolder controlData, float dt) 
    {
        var arcaniaModel = mgc.arcaniaModel;
        foreach (var tabControl in controlData.TabControlUnits)
        {
            foreach (var sep in tabControl.SeparatorControls)
            {
                var process = false;
                if (sep.SeparatorLayout.ClickedLayout) 
                {
                    sep.Expanded = !sep.Expanded;
                    process = true;
                }
                if (sep.Expanded) process = true;
                if (!process) continue;
                foreach (var pair in sep.UnitGroupControls)
                {
                    foreach (var unit in pair.Value)
                    {
                        unit.MainLayout.SetParentShowing(sep.Expanded);
                        var visible = unit.Data?.Visible ?? false;
                        unit.MainLayout.SetVisibleSelf(visible);
                        UpdateChangeGroups(unit);
                        var layoutClicked = unit.ExpandWhenClickingLayout?.ClickedLayout ?? false;
                        bool expandClick = unit.ExpandButton?.ButtonClicked ?? false;
                        expandClick = expandClick || layoutClicked;
                        if (expandClick)
                        {
                            var ls = unit.ExpandButtonImage.Rect.localScale;
                            ls.y *= -1;
                            unit.ExpandButtonImage.Rect.localScale = ls;
                            unit.Expanded = !unit.Expanded;
                            foreach (var item in unit.InsideExpandable)
                            {
                                item.SetParentShowing(unit.Expanded);
                            }
                        }
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
            if (item == null) continue;
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
