using HeartUnity;
using HeartUnity.View;
using UnityEngine;

public static class JGameControlExecuter
{
    public const float NormalMinTabWidth = 320;
    public const float NormalMaxTabWidth = 640;
    public const float NormalThinWidth = 180;

    public static void ManualUpdate(MainGameControl mgc, JGameControlDataHolder controlData, float dt)
    {
        var labelDuration = controlData.LabelDuration;
        var labelEffectDuration = controlData.LabelEffectDuration;
        var labelSuccessRate = controlData.LabelSuccessRate;
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

        // initialization for mobile
        var numberOfTabsVisible = maxNumberOfOptionalContentTabsVisible;
        if (desktopMode)
        {
            numberOfTabsVisible = 0;
            for (int tabIndex = 0; tabIndex < controlData.TabControlUnits.Count; tabIndex++)
            {
                if (controlData.TabControlUnits[tabIndex].TabData.Tab.NecessaryForDesktopAndThinnable) continue;
                if (controlData.TabControlUnits[tabIndex].TabData.Tab.OpenOtherTabs) continue;
                if (controlData.TabControlUnits[tabIndex].TabData.Tab.OpenSettings) continue;
                if (!mgc.JLayoutRuntime.jLayCanvas.IsChildVisible(tabIndex)) continue;
                numberOfTabsVisible++;
            }
            numberOfTabsVisible = Mathf.Clamp(numberOfTabsVisible, 1, maxNumberOfOptionalContentTabsVisible);
        }

        var widthOfContentTab = availableActualWidthForContent / numberOfTabsVisible;
        float maxContentTabWidth = NormalMinTabWidth * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize * 2;
        widthOfContentTab = Mathf.Min(widthOfContentTab, maxContentTabWidth);
        controlData.tabMenu[Direction.WEST].SetVisibleSelf(desktopMode);
        controlData.tabMenu[Direction.SOUTH].SetVisibleSelf(!desktopMode);
        CheckIfNeedsToHideTab(mgc, maxNumberOfTabsVisible);

        #region Main loop that does tons of things (tabs, logs, each unit)
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
                    mgc.GoToSettings();
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

            if (tabControl.SpaceShowLayout != null)
            {
                tabControl.SpaceShowLayout.SetTextRaw(0, Local.GetText("Space", "In the sense of a table taking up too much space"));
                tabControl.SpaceShowLayout.SetTextRaw(1, $"{arcaniaModel.Housing.SpaceConsumed} / {arcaniaModel.Housing.TotalSpace}");
            }

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
                child.UpdateDesiredSize(NormalThinWidth * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
            }
            else
            {
                child.Mandatory = false;
                child.UpdateDesiredSize(widthOfContentTab);
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
                        if (visible && unit.Data.Location != null && mgc.arcaniaModel.Exploration.IsExplorationActive)
                        {
                            visible = false;
                        }
                        unit.MainLayout.SetVisibleSelf(visible);
                        if (!visible) continue;
                        {
                            JRTControlUnitMods modList = unit.OwnedMods;
                            for (int modIndex = 0; modIndex < modList.Mods.Count; modIndex++)
                            {
                                ModRuntime item = modList.Mods[modIndex];
                                if (item.ModType == ModType.SpaceConsumption)
                                {

                                    JLayout.JLayoutRuntimeUnit ttv = modList.tripleTextViews[modIndex];
                                    ttv.Disabled = arcaniaModel.Housing.FurnitureNotMaxedButNotEnoughSpace(unit.Data);
                                    ttv.SetTextRaw(2, $"({arcaniaModel.Housing.SpaceConsumed} / {arcaniaModel.Housing.TotalSpace})");
                                }
                            }
                        }
                        {
                            var modList = unit.IntermediaryMods;
                            FeedModToList(modList, false);
                            FeedModToList(unit.TargetingThisMods, true);
                            FeedModToList(unit.TargetingThisEffectMods, true);
                        }
                        shouldShowSep = true;
                        UpdateChangeGroups(unit);
                        UpdateExpandLogicForUnit(unit);
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
                            case UnitType.LOCATION:
                                {
                                    var progress = unit.Data.TaskProgressRatio;
                                    bool running = arcaniaModel.Runner.RunningTasks.Contains(unit.Data);
                                    //if (unit.Data.Dirty)
                                    {
                                        if (unit.Data.BuyStatus == BuyStatus.NeedsBuy)
                                        {
                                            unit.TitleText.SetTextRaw("Acquire " + unit.Data.Name);
                                        }
                                        else if (unit.Data.BuyStatus == BuyStatus.Bought)
                                        {
                                            unit.TitleText.SetTextRaw(unit.Data.Name);
                                        }
                                    }
                                    if (unit.Data.DotRU != null && unit.Data.DotRU.Dirty)
                                    {
                                        var dotActive = unit.Data.DotRU.Value > 0;
                                        if (dotActive)
                                        {
                                            unit.ButtonImageMain.OverwriteColor(JLayout.ColorSetType.NORMAL, controlData.gameViewMiscData.ButtonColorDotActive);
                                            unit.ButtonImageProgress.OverwriteColor(JLayout.ColorSetType.NORMAL, controlData.gameViewMiscData.ButtonColorDotActive_bar);
                                        }
                                        else
                                        {
                                            unit.ButtonImageMain.ReleaseOverwriteColor(JLayout.ColorSetType.NORMAL);
                                            unit.ButtonImageProgress.ReleaseOverwriteColor(JLayout.ColorSetType.NORMAL);
                                        }
                                    }
                                    if (!running && unit.Data.DotRU != null && unit.Data.DotRU.Value != 0)
                                    {
                                        progress = unit.Data.DotRU.TaskProgressRatio;
                                    }
                                    unit.MainExecuteButton.SetButtonEnabled(arcaniaModel.Runner.CanStartAction(unit.Data));
                                    unit.MainExecuteButton.MultiClickEnabled(unit.Data.IsInstant());

                                    unit.MainExecuteButton.SetActivePowered(running);
                                    if (unit.Expanded)
                                    {
                                        var data = unit.Data;
                                        unit.TaskQuantityText?.SetTextRaw(data.HasMax ? $"{data.Value} / {data.Max}" : $"{data.Value}");
                                        if (unit.Data.ConfigTask != null)
                                        {
                                            bool hasDuration = unit.Data.ConfigTask.Duration.HasValue && unit.Data.ConfigTask.Duration.Value > 1;
                                            var hasSuccessRate = unit.Data.ConfigTask.SuccessRatePercent.HasValue && unit.Data.ConfigTask.SuccessRatePercent.Value != 100;
                                            var dotDuration = unit.Data.DotRU?.DotConfig.Duration;
                                            if (hasDuration || hasSuccessRate || dotDuration.HasValue)
                                            {
                                                var leftText = "";
                                                if (hasDuration) leftText += $" {labelDuration}: {unit.Data.ConfigTask.Duration}s ";
                                                if (hasSuccessRate)
                                                {
                                                    leftText += $" {labelSuccessRate}: {unit.Data.ConfigTask.SuccessRatePercent.Value}% ";
                                                }
                                                if (dotDuration.HasValue)
                                                {
                                                    leftText += $"\n{labelEffectDuration}: {dotDuration.Value}s ";
                                                }
                                                unit.SuccessRateAndDurationText.SetTextRaw(leftText);
                                            }
                                        }

                                    }


                                    if (pair.Key == UnitType.LOCATION)
                                        progress = arcaniaModel.Exploration.LastActiveLocation == unit.Data ? arcaniaModel.Exploration.ExplorationRatio : 0f;
                                    unit.MainLayout.Children[0].LayoutRU.ButtonChildren[0].Item1.ImageChildren[1].UpdateSizeRatioAsGauge(progress);
                                    // tcu.bwe.MainButtonEnabled = arcaniaModel.Runner.CanStartAction(data);
                                    // tcu.bwe.MainButtonSelected(arcaniaModel.Runner.RunningTasks.Contains(data));
                                    if (unit.TaskClicked)
                                    {
                                        arcaniaModel.Runner.StartActionExternally(unit.Data);
                                    }
                                }
                                break;
                            case UnitType.HOUSE:
                                {
                                    //unit.MainExecuteButton.SetButtonEnabled(arcaniaModel.Housing.CanChangeHouse(unit.Data));
                                    unit.MainExecuteButton.SetActivePowered(arcaniaModel.Housing.IsLivingInHouse(unit.Data));
                                    unit.MainExecuteButton.SetButtonEnabled(arcaniaModel.Housing.CanChangeHouse(unit.Data) || arcaniaModel.Housing.IsLivingInHouse(unit.Data));
                                    if (unit.TaskClicked && !arcaniaModel.Housing.IsLivingInHouse(unit.Data))
                                    {

                                        arcaniaModel.Housing.ChangeHouse(unit.Data);
                                    }
                                }

                                break;
                            case UnitType.SKILL:
                                {
                                    var progress = unit.Data.TaskProgressRatio;
                                    unit.ButtonImageProgress.SetGaugeRatio(progress);
                                    var data = unit.Data;
                                    bool acquired = data.Skill.Acquired;
                                    unit.MainExecuteButton.SetButtonEnabled(acquired ? arcaniaModel.Runner.CanStudySkill(data) : arcaniaModel.Runner.CanAcquireSkill(data));
                                    unit.MainExecuteButton.SetButtonTextRaw(acquired ? controlData.LabelPracticeSkill : controlData.LabelAcquireSkill);
                                    unit.GaugeLayout.SetVisibleSelf(acquired);
                                    unit.GaugeProgressImage.SetGaugeRatio(data.Skill.XPRatio);
                                    unit.MainExecuteButton.buttonOwner.ButtonChildren[0].Item1.ImageChildren[1].UpdateSizeRatioAsGauge(unit.Data.TaskProgressRatio);
                                    if (acquired)
                                    {
                                        unit.TitleWithValue.SetTextRaw(1, $"{unit.Data.Value} / {unit.Data.Max}");
                                    }
                                    if (unit.TaskClicked)
                                    {
                                        if (acquired) arcaniaModel.Runner.StudySkill(data);
                                        else { 
                                            arcaniaModel.Runner.AcquireSkill(data);
                                            unit.MainLayout.MarkDirtyWithChildren();
                                        }
                                    }
                                }

                                break;
                            case UnitType.FURNITURE:
                                {
                                    unit.PlusMinusLayout.ButtonChildren[0].Item2.UiUnit.ButtonEnabled = arcaniaModel.Housing.CanAcquireFurniture(unit.Data);
                                    unit.PlusMinusLayout.ButtonChildren[1].Item2.UiUnit.ButtonEnabled = arcaniaModel.Housing.CanRemoveFurniture(unit.Data);
                                    if (unit.PlusMinusLayout.IsButtonClicked(0))
                                    {
                                        arcaniaModel.Housing.AcquireFurniture(unit.Data);
                                    }
                                    if (unit.PlusMinusLayout.IsButtonClicked(1))
                                    {
                                        arcaniaModel.Housing.RemoveFurniture(unit.Data);
                                    }
                                }
                                break;
                            case UnitType.TAB:
                                break;
                            case UnitType.DIALOG:
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
        #endregion
        // do it twice to make sure
        CheckIfNeedsToHideTab(mgc, maxNumberOfTabsVisible);

        JGameControlExecuterExploration.ManualUpdate(mgc, controlData, dt);
        JGameControlExecuterEnding.ManualUpdate(mgc, controlData, dt);

        #region dialog

        var dialog = arcaniaModel.Dialog.ActiveDialog;
        if (arcaniaModel.Dialog.ShouldShow != controlData.DialogLayout.LayoutRU.Visible)
        {
            if (arcaniaModel.Dialog.ShouldShow)
            {
                ShowDialog(dialog, controlData);
            }
            else
            {
                controlData.LayoutRuntime.jLayCanvas.HideOverlay();
                controlData.DialogLayout.LayoutRU.SetVisibleSelf(false);
            }
        }
        for (int i = 0; i < 2; i++)
        {
            if (controlData.DialogLayout.LayoutRU.LayoutChildren[0].LayoutRU.IsButtonClicked(i))
            {
                arcaniaModel.Dialog.DialogComplete(i);
            }
        }
        #endregion
    }

    private static void FeedModToList(JRTControlUnitMods modList, bool showEvenIfZero)
    {
        var hasAnyVisible = false;
        for (int modIndex = 0; modIndex < modList.Mods.Count; modIndex++)
        {
            ModRuntime item = modList.Mods[modIndex];
            var ttv = modList.tripleTextViews[modIndex];
            if (showEvenIfZero)
            {
                bool visibleSelf = item.Source.Visible || item.Source.Value != 0;
                ttv.SetVisibleSelf(visibleSelf);
                ttv.Disabled = item.Source.Value == 0;
            }
            else
            {
                ttv.SetVisibleSelf(item.Source.Value != 0);
            }
            hasAnyVisible |= ttv.Visible;
            var noShowSourceNumber = item.Source.Value == 1;
            var noShowIntermediaryNumber = (item.Intermediary?.RuntimeUnit?.Value ?? 1) == 1;
            var needsParenthesis = !noShowSourceNumber || !noShowIntermediaryNumber;
            var negativeValue = item.Value < 0;
            var sourceValueToUse = negativeValue ? item.Value * -1 : item.Value;
            var modT = $"{sourceValueToUse}";
            if (!noShowIntermediaryNumber)
            {
                modT = $"{item.Intermediary.RuntimeUnit.Value} * {modT}";
            }
            if (!noShowSourceNumber)
            {
                modT = $"{item.Source.Value} * {modT}";
            }
            if (needsParenthesis)
            {
                modT = $"{(negativeValue ? "-" : "+")}({modT})";
            }
            else
            {
                if (!negativeValue)
                {
                    modT = "+" + modT;
                }
            }
            ttv.SetTextRaw(1, modT);
            /*
            if (item.Value > 0)
            {
                ttv.SetTextRaw(1, $"+({item.Source.Value} * {item.Value})");
            }
            else 
            {
                ttv.SetTextRaw(1, $"-({item.Source.Value} * {item.Value*-1})");
            }
            */

            // ttv.SetTextRaw(1, "+" + (item.Source.Value * item.Value));
        }
        if (modList.Header == null) return;
        modList.Header.SetVisibleSelf(hasAnyVisible);
    }

    private static void ShowDialog(DialogRuntime dialog, JGameControlDataHolder controlData)
    {
        controlData.DialogLayout.LayoutRU.SetTextRaw(0, dialog.Title);
        controlData.DialogLayout.LayoutRU.SetTextRaw(1, dialog.Content);
        controlData.LayoutRuntime.jLayCanvas.ShowOverlay();
        controlData.DialogLayout.LayoutRU.SetVisibleSelf(true);
    }

    public static void UpdateExpandLogicForUnit(JRTControlUnit unit)
    {
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
            var item = ChangeGroups[i];
            if (item == null) continue;
            var sep = item.Header;
            var resourceChanges = Data.ConfigTask.GetResourceChangeList(i);

            bool skip = (Data.Skill != null && Data.Skill.Acquired && resourceChangeType == ResourceChangeType.COST);
            skip = skip || (Data.BuyStatus == BuyStatus.Bought && resourceChangeType == ResourceChangeType.BUY);
            if (item == null || skip)
            {
                if (sep != null) sep.SetVisibleSelf(false);
                for (int ttvIndex = 0; ttvIndex < item.tripleTextViews.Count; ttvIndex++)
                {
                    var ttv = item.tripleTextViews[ttvIndex];
                    ttv.SetVisibleSelf(false);
                    // ttv.LayoutChild.VisibleSelf = false;
                }
                continue;
            }


            //if (sep != null) sep.ManualUpdate();
            //sep.LayoutChild.VisibleSelf = resourceChanges.Count > 0;
            var bySecond = i == (int)ResourceChangeType.EFFECT || i == (int)ResourceChangeType.RUN;
            var canBeDisabled = i == (int)ResourceChangeType.RUN || i == (int)ResourceChangeType.COST || i == (int)ResourceChangeType.BUY;

            for (int ttvIndex = 0; ttvIndex < item.tripleTextViews.Count; ttvIndex++)
            {
                var ttv = item.tripleTextViews[ttvIndex];
                bool visibleSelf = resourceChanges.Count > ttvIndex;
                ttv.SetVisibleSelf(visibleSelf);
                if (!visibleSelf) continue;
                var rc = resourceChanges[ttvIndex];

                var min = rc.valueChange.min;
                var max = rc.valueChange.max;

                string targetName;
                string tertiaryText = "";
                if (rc.IdPointer.RuntimeUnit != null)
                {
                    RuntimeUnit dataThatWillBeChanged = rc.IdPointer.RuntimeUnit;
                    targetName = dataThatWillBeChanged.Visible ? dataThatWillBeChanged.Name : "???";
                    int value = dataThatWillBeChanged.Value;
                    if (canBeDisabled)
                    {
                        // max is negative, so the sum needs to be above 0
                        ttv.Disabled = (value + max) < 0;
                    }
                    if (dataThatWillBeChanged.HasMax)
                        tertiaryText = $"({value} / {dataThatWillBeChanged.Max})";
                    else
                        tertiaryText = $"({value})";
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
