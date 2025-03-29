public static class JGameControlExecuter 
{
    public static void ManualUpdate(MainGameControl mgc, JGameControlDataHolder controlData, float dt) 
    {
        var arcaniaModel = mgc.arcaniaModel;
        for (int tabIndex = 0; tabIndex < controlData.TabControlUnits.Count; tabIndex++)
        {
            JTabControlUnit tabControl = controlData.TabControlUnits[tabIndex];
            // open other tabs currently just invisible for now
            bool tabEnabled = tabControl.TabData.Visible && !tabControl.TabData.Tab.OpenOtherTabs;
            
            tabControl.DesktopButton.SetVisibleSelf(tabEnabled);
            mgc.JLayoutRuntime.jLayCanvas.EnableChild(tabIndex, tabEnabled);
            if (!tabEnabled) continue;
            var dynamicCanvas = mgc.JLayoutRuntime.jLayCanvas;
            
            bool clickedTabButton = tabControl.DesktopButton.ClickedLayout;
            
            #region tab clicking
            if (clickedTabButton)
            {
                if (tabControl.TabData.Tab.OpenSettings)
                {
                    // GoToSettings();
                }
                else if (tabControl.TabData.Tab.OpenOtherTabs)
                {
                    // dynamicCanvas.ShowOverlay(this.TabButtonOverlayLayout);

                }
                else if (dynamicCanvas.CanShowOnlyOneChild())
                {
                    dynamicCanvas.ShowChild(tabIndex);
                }
                else
                {
                    dynamicCanvas.ToggleChild(tabIndex);
                }
                /*if (tabControl.SelectionButtonLarge.Button.Clicked)
                {
                    dynamicCanvas.HideOverlay();
                }
                */
            }
            #endregion

            #region calculate if tab is active
            bool tabActive = dynamicCanvas.IsChildVisible(tabIndex) && !tabControl.TabData.Tab.OpenSettings && !tabControl.TabData.Tab.OpenOtherTabs;
            tabControl.DesktopButton.ImageChildren[0].UiUnit.ActiveSelf = tabActive;
            #endregion

            // an invisible tab needs no processing
            if (!tabActive) continue;

            #region logs with loop continue (will skip code below)
            if (tabControl.TabData.Tab.ContainsLogs)
            {
                tabControl.SeparatorControls[0].SeparatorLayout.SetVisibleSelf(true);
                while (tabControl.LogAmount < arcaniaModel.LogUnits.Count)
                {
                    var lay = MainGameControlSetupJLayout.CreateLogLayout(mgc, arcaniaModel.LogUnits[tabControl.LogAmount]);
                    dynamicCanvas.children[tabIndex].AddLayoutAsChild(lay);
                    tabControl.LogAmount++;
                }
                continue;
            }
            #endregion
            #region main unit loop by separator
            foreach (var sep in tabControl.SeparatorControls)
            {
                var process = false;
                var shouldShowSep = false;
                if (sep.SeparatorLayout.ClickedLayout) 
                {
                    sep.Expanded = !sep.Expanded;
                    var sepImage = sep.SeparatorLayout.ImageChildren[0];
                    var ls = sepImage.Rect.localScale;
                    ls.y *= -1;
                    sepImage.Rect.localScale = ls;
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
                        if (!visible) continue;
                        shouldShowSep = true;
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
                        if (unit.ValueText != null) 
                        {
                            var Data = unit.Data;
                            var valueT = Data.HasMax ? $"{Data.Value} / {Data.Max}" : $"{Data.Value}";
                            unit.ValueText.SetTextRaw(valueT + "");
                        }
                        switch (pair.Key)
                        {
                            case UnitType.RESOURCE:
                                {
                                    
                                }
                                break;
                            case UnitType.TASK:
                                {
                                    unit.MainExecuteButton.SetButtonEnabled(arcaniaModel.Runner.CanStartAction(unit.Data));
                                    unit.MainExecuteButton.MultiClickEnabled(unit.Data.IsInstant());
                                    unit.MainExecuteButton.SetActive(arcaniaModel.Runner.RunningTasks.Contains(unit.Data));
                                    unit.MainLayout.Children[0].LayoutRU.ButtonChildren[0].Item1.ImageChildren[1].SizeRatioAsGauge = unit.Data.TaskProgressRatio;
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
                sep.SeparatorLayout.SetVisibleSelf(shouldShowSep);
            }
            #endregion
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
                    ttv.SetTextRaw(0, targetName);
                }

                var valueText = min != max ? $"{min}~{max}" : $"{min}";
                string secondText = bySecond ? $"{valueText}/s" : $"{valueText}";
                ttv.SetTextRaw(1, secondText);
                ttv.SetTextRaw(2, tertiaryText);
                //ttv.ManualUpdate();
            }
        }
    }
}
