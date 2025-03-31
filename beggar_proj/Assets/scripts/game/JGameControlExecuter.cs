using HeartUnity.View;
using UnityEngine;

public static class JGameControlExecuter 
{
    public const float NormalMinTabWidth = 320;
    public const float NormalMaxTabWidth = 640;
    public const float NormalThinWidth = 180;
    public static void ManualUpdate(MainGameControl mgc, JGameControlDataHolder controlData, float dt)
    {
        var arcaniaModel = mgc.arcaniaModel;
        var desktopMode = false;
        var availableActualWidthForContent = Screen.width;
        #region calculate if desktop or mobile, also calculate available width
        {
            var necessaryDefaultPixelWidthForDesktop = 0f;
            // calculate full size of left side menus
            var leftWidth = 0f;
            foreach (var item in mgc.JLayoutRuntime.jLayCanvas.FixedMenus[HeartUnity.View.Direction.WEST])
            {
                leftWidth += item.LayoutRuntimeUnit.LayoutData.commons.Size[0];
            }
            // add left side menu that is mandatory for desktop
            necessaryDefaultPixelWidthForDesktop += leftWidth;
            // a single tab size added as minimum width
            necessaryDefaultPixelWidthForDesktop += NormalMinTabWidth;
            // the thin tab equivalent to the log tab
            // this is hard coded but could also be calculated based on the tab data in the model
            // the "thin necessary" blabla field
            necessaryDefaultPixelWidthForDesktop += NormalThinWidth;
            desktopMode = Screen.width > necessaryDefaultPixelWidthForDesktop * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
            if (desktopMode)
            {
                availableActualWidthForContent -= Mathf.CeilToInt((leftWidth + NormalThinWidth) * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
            }
        }

        #endregion
        var maxNumberOfOptionalContentTabsVisible = Mathf.Max(Mathf.Floor(availableActualWidthForContent / (NormalMinTabWidth * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize)), 1);
        var maxNumberOfTabsVisible = maxNumberOfOptionalContentTabsVisible;

        // if on desktop mode, add log tab to the number
        if (desktopMode) maxNumberOfTabsVisible++;
        // temporary code, should calculate this based on which tabs are visible
        var numberOfTabsVisible = maxNumberOfOptionalContentTabsVisible;
        var widthOfContentTab = availableActualWidthForContent / numberOfTabsVisible;
        controlData.tabMenu[Direction.WEST].SetVisibleSelf(desktopMode);
        controlData.tabMenu[Direction.SOUTH].SetVisibleSelf(!desktopMode);
        CheckIfNeedsToHideTab(mgc, maxNumberOfTabsVisible);

        for (int tabIndex = 0; tabIndex < controlData.TabControlUnits.Count; tabIndex++)
        {
            JTabControlUnit tabControl = controlData.TabControlUnits[tabIndex];
            // open other tabs currently just invisible for now
            bool tabEnabled = tabControl.TabData.Visible && !tabControl.TabData.Tab.OpenOtherTabs;
            var tabData = tabControl.TabData.Tab;
            bool alwaysActive = desktopMode && tabData.NecessaryForDesktopAndThinnable;

            foreach (var tabB in tabControl.TabToggleButtons)
            {
                tabB.SetVisibleSelf(tabEnabled && !alwaysActive);
            }
            mgc.JLayoutRuntime.jLayCanvas.EnableChild(tabIndex, tabEnabled);
            if (!tabEnabled) continue;
            var dynamicCanvas = mgc.JLayoutRuntime.jLayCanvas;

            bool clickedTabButton = false;
            foreach (var tabB in tabControl.TabToggleButtons)
            {
                if (tabB.ClickedLayout)
                {
                    clickedTabButton = true;
                    break;
                }
            }


            if (alwaysActive && !dynamicCanvas.IsChildVisible(tabIndex))
            {
                dynamicCanvas.ShowChild(tabIndex);
            }

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

            tabActive |= alwaysActive;
            foreach (var item in tabControl.TabToggleButtons)
            {
                item.ImageChildren[0].UiUnit.ActiveSelf = tabActive;
            }

            #endregion

            // an invisible tab needs no processing
            if (!tabActive) continue;

            #region thin tab attempt
            JLayout.JLayCanvasChild child = dynamicCanvas.children[tabIndex];
            if (tabData.NecessaryForDesktopAndThinnable && desktopMode)
            {

                int indexOfThinNecessary = mgc.JLayoutRuntime.jLayCanvas.childrenForLayouting.IndexOf(child);
                var isLast = indexOfThinNecessary == mgc.JLayoutRuntime.jLayCanvas.childrenForLayouting.Count - 1;
                if (!isLast)
                {
                    if (indexOfThinNecessary < 0)
                    {
                        dynamicCanvas.ShowChild(tabIndex);
                    }
                    mgc.JLayoutRuntime.jLayCanvas.childrenForLayouting.Remove(child);
                    mgc.JLayoutRuntime.jLayCanvas.childrenForLayouting.Add(child);
                }
                child.Mandatory = true;
                child.DesiredSize = NormalThinWidth * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
            }
            else
            {
                child.Mandatory = false;
                child.DesiredSize = widthOfContentTab;
            }
            #endregion

            #region logs with loop continue (will skip code below)
            if (tabControl.TabData.Tab.ContainsLogs)
            {
                tabControl.SeparatorControls[0].SeparatorLayout.SetVisibleSelf(true);
                while (tabControl.LogAmount < arcaniaModel.LogUnits.Count)
                {
                    var lay = MainGameControlSetupJLayout.CreateLogLayout(mgc, arcaniaModel.LogUnits[tabControl.LogAmount]);
                    dynamicCanvas.children[tabIndex].LayoutRuntimeUnit.AddLayoutAsChild(lay);
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
                            case UnitType.CLASS:
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
                            case UnitType.SKILL:
                                {
                                    var data = unit.Data;
                                    unit.MainExecuteButton.SetButtonEnabled(data.Skill.Acquired ? arcaniaModel.Runner.CanStudySkill(data) : arcaniaModel.Runner.CanAcquireSkill(data));
                                    unit.MainExecuteButton.SetButtonTextRaw(data.Skill.Acquired ? "Practice skill" : "Acquire Skill");
                                    unit.XPGaugeLayout.SetVisibleSelf(data.Skill.Acquired);
                                    unit.XPGaugeProgressImage.SetGaugeRatio(data.Skill.XPRatio);
                                    unit.MainExecuteButton.buttonOwner.ButtonChildren[0].Item1.ImageChildren[1].SizeRatioAsGauge = unit.Data.TaskProgressRatio;
                                    if (unit.TaskClicked)
                                    {
                                        if (data.Skill.Acquired) arcaniaModel.Runner.StudySkill(data);
                                        else arcaniaModel.Runner.AcquireSkill(data);
                                    }
                                }
                                
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

        // do it twice to make sure
        CheckIfNeedsToHideTab(mgc, maxNumberOfTabsVisible);
    }

    private static void CheckIfNeedsToHideTab(MainGameControl mgc, float maxNumberOfTabsVisible)
    {
        while (mgc.JLayoutRuntime.jLayCanvas.ActiveChildren.Count > maxNumberOfTabsVisible)
        {
            for (int i = mgc.JLayoutRuntime.jLayCanvas.ActiveChildren.Count - 1; i >= 0; i--)
            {
                if (mgc.JLayoutRuntime.jLayCanvas.ActiveChildren[i].Mandatory) continue;
                mgc.JLayoutRuntime.jLayCanvas.ActiveChildren.RemoveAt(i);
                break;
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
