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

        MainCommonLoop(mgc, controlData);

        JGameControlExecuterExploration.ManualUpdate(mgc, controlData, dt);
        JGameControlExecuterSaveSlot.ManualUpdate(mgc);
        JGameControlExecuterEnding.ManualUpdate(mgc, controlData, dt);

        UpdateDialog(mgc, controlData);
    }

    public static void ManualUpdateArchive(MainGameControl mgc, JGameControlDataHolder controlData, float dt)
    {
        MainCommonLoop(mgc, controlData);
        // no dialogs for now
        // UpdateDialog(mgc, controlData);
    }

    private static void UpdateDialog(MainGameControl mgc, JGameControlDataHolder controlData)
    {
        ArcaniaModel arcaniaModel = mgc.arcaniaModel;
        #region dialog

        var dialog = arcaniaModel.Dialog.ActiveDialog;
        if (arcaniaModel.Dialog.ShouldShow != controlData.DialogLayout.LayoutRU.Visible)
        {
            if (arcaniaModel.Dialog.ShouldShow)
            {
                ShowDialog(mgc, dialog, controlData);
            }
            else if (controlData.overlayType == JGameControlDataHolder.OverlayType.YesNoArcaniaDialog)
            {
                HideOverlay(mgc);
                controlData.DialogLayout.LayoutRU.SetVisibleSelf(false);
            }
        }
        for (int i = 0; i < 2; i++)
        {
            if (controlData.overlayType == JGameControlDataHolder.OverlayType.YesNoArcaniaDialog)
            {
                if (controlData.DialogLayout.LayoutRU.LayoutChildren[0].LayoutRU.IsButtonClicked(i))
                {
                    arcaniaModel.Dialog.DialogComplete(i);
                }
            }
            if (controlData.overlayType == JGameControlDataHolder.OverlayType.ConfirmDeleteSave)
            {
                if (controlData.DialogLayout.LayoutRU.LayoutChildren[0].LayoutRU.IsButtonClicked(i))
                {
                    if (i == 0)
                    {
                        JGameControlExecuterSaveSlot.ConfirmDelete(mgc);
                    }
                    JGameControlExecuter.HideOverlay(mgc);
                    controlData.DialogLayout.LayoutRU.SetVisibleSelf(false);
                }
            }
        }
        #endregion
    }

    private static void MainCommonLoop(MainGameControl mgc, JGameControlDataHolder controlData)
    {
        var labelDuration = controlData.LabelDuration;
        var labelEffectDuration = controlData.LabelEffectDuration;
        var labelSuccessRate = controlData.LabelSuccessRate;
        var arcaniaModel = mgc.arcaniaModel;
        bool desktopMode;
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

        var widthOfContentTab = Mathf.Min(availableActualWidthForContent / numberOfTabsVisible, Screen.width);

        float maxContentTabWidth = NormalMinTabWidth * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize * 2;
        widthOfContentTab = Mathf.Min(widthOfContentTab, maxContentTabWidth);
        controlData.tabMenu[Direction.WEST].SetVisibleSelf(desktopMode);
        controlData.tabMenu[Direction.SOUTH].SetVisibleSelf(!desktopMode);
        CheckIfNeedsToHideTab(mgc, maxNumberOfTabsVisible);

        #region cache colors for use below
        var loreColorCode = controlData.LayoutRuntime.LayoutMaster.ColorDatas.GetData("lore_text").CodeCache[controlData.LayoutRuntime.CurrentColorSchemeId];
        #endregion


        #region find out how many tab button visible in the menus
        var numberOfTabButtonsThatNeedButtonExcludingPlusTab = 0;
        bool allTabButtonVisible;
        int maxNumberOfTabButtonVisible;
        for (int tabIndex = 0; tabIndex < controlData.TabControlUnits.Count; tabIndex++)
        {

            JTabControlUnit jTabControlUnit = controlData.TabControlUnits[tabIndex];
            // no tab button at all in this case
            if (jTabControlUnit.TabData.Tab.NecessaryForDesktopAndThinnable && desktopMode) continue;
            if (!jTabControlUnit.TabData.Visible) continue;
            if (jTabControlUnit.TabData.Tab.OpenOtherTabs) continue;
            numberOfTabButtonsThatNeedButtonExcludingPlusTab++;
        }

        {
            var checkingDesktop = desktopMode;
            var direction = checkingDesktop ? Direction.WEST : Direction.SOUTH;
            // do nothing for now on desktop, TODO implement this for diff direction
            var layout = controlData.tabMenu[direction];
            int axis = checkingDesktop ? 1 : 0;
            float size = layout.GetSize(axis);
            // leeway so it shows one more tab button depending on width
            var leeway = 20 * RectTransformExtensions.DpiScaleFromDefault;
            maxNumberOfTabButtonVisible = Mathf.FloorToInt((size + leeway) / (55 * RectTransformExtensions.DpiScaleFromDefault));
            allTabButtonVisible = numberOfTabButtonsThatNeedButtonExcludingPlusTab <= maxNumberOfTabButtonVisible;
        }

        // if has to show plus tab, exclude one of the max
        int maxNumberOfTabButtonVisibleExcludingPlusTab = allTabButtonVisible ? maxNumberOfTabButtonVisible : maxNumberOfTabButtonVisible - 1;
        #endregion

        if (mgc.JControlData.TabOverlayCloseButtonJCU.TaskClicked)
        {
            HideOverlay(mgc);
        }

        #region Main loop that does tons of things (tabs, logs, each unit)
        int numberOfTabButtonsAlreadyActiveExcludingPlusTab = 0;
        for (int tabIndex = 0; tabIndex < controlData.TabControlUnits.Count; tabIndex++)
        {
            JTabControlUnit tabControl = controlData.TabControlUnits[tabIndex];
            // open other tabs currently just invisible for now
            bool plusTabForced = tabControl.TabData.Tab.OpenOtherTabs && !allTabButtonVisible;
            bool tabEnabled = tabControl.TabData.Visible || plusTabForced;
            bool tabButtonEnabled = (tabEnabled && numberOfTabButtonsAlreadyActiveExcludingPlusTab < maxNumberOfTabButtonVisibleExcludingPlusTab) || plusTabForced;
            if (tabControl.TabData.Tab.OpenOtherTabs && !plusTabForced)
            {
                tabButtonEnabled = false;
                tabEnabled = false;
            }

            var tabData = tabControl.TabData.Tab;
            bool alwaysActive = desktopMode && tabData.NecessaryForDesktopAndThinnable;
            bool bigTabButtonEnabled = tabEnabled && !plusTabForced && !alwaysActive && !tabButtonEnabled;


            foreach (var tabB in tabControl.TabToggleButtons)
            {
                // visibility of overlay is set below
                if (tabB == tabControl.OverlayButton) continue;
                tabB.SetVisibleSelf(tabButtonEnabled && !alwaysActive);
            }
            tabControl.OverlayButton.SetVisibleSelf(bigTabButtonEnabled);
            mgc.JLayoutRuntime.jLayCanvas.EnableChild(tabIndex, tabEnabled);
            if (!tabEnabled) continue;

            if (tabButtonEnabled && !alwaysActive && !plusTabForced)
            {
                numberOfTabButtonsAlreadyActiveExcludingPlusTab++;
            }

            var dynamicCanvas = mgc.JLayoutRuntime.jLayCanvas;

            bool clickedTabButton = false;
            if (tabControl.OverlayButton.ClickedLayout)
            {
                clickedTabButton = true;
                HideOverlay(mgc);
            }
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
                    JGameControlExecuter.ShowOverlay(mgc, JGameControlDataHolder.OverlayType.TabMenu);
                    mgc.JControlData.OverlayTabMenuLayout.LayoutRU.SetVisibleSelf(true);
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
                //move this here to setup code into mgc.jcontroldata
                tabControl.SpaceShowLayout.SetTextRaw(0, mgc.JControlData.LabelSpace);
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
                shouldShowSep |= sep.SepD.ContainsSaveSlots;
                shouldShowSep |= sep.SepD.ArchiveMainUI;
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

                        shouldShowSep = true;
                        var expandChange = UpdateExpandLogicForUnit(unit);

                        if (expandChange ?? false) 
                        {
                            foreach (var u in controlData.expandedUnits)
                            {
                                if (!u.Expanded) continue;
                                if (u == unit) continue;
                                ToggleExpansion(u);
                            }
                            controlData.expandedUnits.Clear();
                            controlData.expandedUnits.Add(unit);
                        }
                        if (expandChange.HasValue) 
                        {
                            MainGameJLayoutPoolExecuter.OnExpandChanged(mgc, unit, expandChange.Value);
                        }
                        if (unit.ValueText != null && unit.Data.ConfigHintData == null)
                        {
                            var Data = unit.Data;
                            var valueT = Data.HasMax ? $"{Data.Value} <color={loreColorCode}>/ {Data.Max}</color>" : $"{Data.Value}";
                            unit.ValueText.SetTextRaw(valueT + "");
                        }
                        if (unit.Expanded)
                        {
                            {
                                var modList = unit.IntermediaryMods;
                                FeedModToList(modList, false);
                                FeedModToList(unit.TargetingThisMods, true);
                                FeedModToList(unit.TargetingThisEffectMods, true);
                            }
                            UpdateChangeGroups(unit);
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
                                    if (unit.MainExecuteButton != null)
                                    {
                                        //if (unit.Data.Dirty)
                                        {
                                            if (unit.Data.BuyStatus == BuyStatus.NeedsBuy)
                                            {
                                                unit.TitleText.SetTextRaw($"{mgc.JControlData.LabelAcquire} ({unit.Data.Name})");
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
                                                if (unit.Data.DotRU.DotConfig.Toggle)
                                                {
                                                    unit.TitleText.SetTextRaw($"{mgc.JControlData.LabelDeactivate} ({unit.Data.Name})");
                                                }
                                            }
                                            else
                                            {
                                                unit.ButtonImageMain.ReleaseOverwriteColor(JLayout.ColorSetType.NORMAL);
                                                unit.ButtonImageProgress.ReleaseOverwriteColor(JLayout.ColorSetType.NORMAL);
                                                if (unit.Data.DotRU.DotConfig.Toggle)
                                                {
                                                    unit.TitleText.SetTextRaw(unit.Data.Name);
                                                }
                                            }
                                        }
                                        if (!running && unit.Data.DotRU != null && unit.Data.DotRU.Value != 0)
                                        {
                                            progress = unit.Data.DotRU.TaskProgressRatio;
                                        }
                                        unit.MainExecuteButton.SetButtonEnabled(arcaniaModel.Runner.CanStartAction(unit.Data));
                                        unit.MainExecuteButton.MultiClickEnabled(unit.Data.IsInstant());

                                        unit.MainExecuteButton.SetActivePowered(running);
                                    }

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
                                                unit.SuccessRateAndDurationText?.SetTextRaw(leftText);
                                            }
                                        }

                                    }
                                    // archive mode doesn't have execute button
                                    if (unit.MainExecuteButton != null)
                                    {
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


                                }
                                break;
                            case UnitType.HOUSE:
                                {
                                    //unit.MainExecuteButton.SetButtonEnabled(arcaniaModel.Housing.CanChangeHouse(unit.Data));
                                    // archive doesn't need this update logic
                                    if (unit.MainExecuteButton != null)
                                    {
                                        unit.MainExecuteButton.SetActivePowered(arcaniaModel.Housing.IsLivingInHouse(unit.Data));
                                        unit.MainExecuteButton.SetButtonEnabled(arcaniaModel.Housing.CanChangeHouse(unit.Data) || arcaniaModel.Housing.IsLivingInHouse(unit.Data));
                                        if (arcaniaModel.Housing.IsLivingInHouse(unit.Data))
                                        {
                                            unit.TitleText.SetTextRaw($"{unit.Data.Name} ({mgc.JControlData.LabelLivingHere})");
                                        }
                                        else
                                        {
                                            unit.TitleText.SetTextRaw($"{unit.Data.Name}");
                                        }
                                        if (unit.TaskClicked && !arcaniaModel.Housing.IsLivingInHouse(unit.Data))
                                        {

                                            arcaniaModel.Housing.ChangeHouse(unit.Data);
                                        }
                                    }
                                }

                                break;
                            case UnitType.SKILL:
                                {
                                    if (unit.MainExecuteButton != null)
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
                                            else
                                            {
                                                arcaniaModel.Runner.AcquireSkill(data);
                                                unit.MainLayout.MarkDirtyWithChildren();
                                            }
                                        }
                                    }
                                }

                                break;
                            case UnitType.FURNITURE:
                                {
                                    // archive mode would be null
                                    if (unit.PlusMinusLayout != null)
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
        //mgc.JLayoutRuntime.jLayCanvas.Overlays[0].LayoutRuntimeUnit.ScrollViewportImage.raycastTarget
        bool overlayClickable = mgc.JControlData.overlayType == JGameControlDataHolder.OverlayType.TabMenu;
        mgc.JLayoutRuntime.jLayCanvas.Overlays[0].LayoutRuntimeUnit.ScrollViewportImage.raycastTarget = !overlayClickable;
        if (overlayClickable)
        {
            if (mgc.JLayoutRuntime.jLayCanvas.overlayImageUU.Clicked)
            {
                HideOverlay(mgc);
            }
        }

        // after all need to hide check, clear
        mgc.JLayoutRuntime.jLayCanvas.RequestVisibleNextFrame = null;
    }

    internal static WorldType GetWorld(MainGameControl mgc)
    {
        switch (mgc.controlState)
        {
            case MainGameControl.ControlState.TITLE:
            case MainGameControl.ControlState.LOADING:
            case MainGameControl.ControlState.GAME:
            case MainGameControl.ControlState.ARCHIVE_LOADING:
            case MainGameControl.ControlState.ARCHIVE_GAME:
                return WorldType.DEFAULT_CHARACTER;
            case MainGameControl.ControlState.PRESTIGE_WORLD_LOADING:
                
            case MainGameControl.ControlState.PRESTIGE_WORLD:
                return WorldType.PRESTIGE_WORLD;
            default:
                return WorldType.DEFAULT_CHARACTER;
        }
    }

    public static void HideOverlay(MainGameControl mgc)
    {
        if (mgc.JControlData.overlayType == JGameControlDataHolder.OverlayType.TabMenu)
        {
            mgc.JControlData.OverlayTabMenuLayout.LayoutRU.SetVisibleSelf(false);
        }
        mgc.JControlData.overlayType = null;
        mgc.JLayoutRuntime.jLayCanvas.HideOverlay();
    }

    public static void ShowOverlay(MainGameControl mgc, JGameControlDataHolder.OverlayType overType)
    {
        mgc.JControlData.overlayType = overType;
        mgc.JLayoutRuntime.jLayCanvas.ShowOverlay();
    }

    internal static bool IsTabVisibleAndShowing(MainGameControl mgc, JTabControlUnit tabC)
    {
        var dynamicCanvas = mgc.JLayoutRuntime.jLayCanvas;
        return dynamicCanvas.IsChildVisible(mgc.JControlData.TabControlUnits.IndexOf(tabC)) && tabC.TabData.Visible;
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
            if (!ttv.Visible) continue;
            if (item.ModType == ModType.Activate) continue;
            var resourceChangeChanger = item.ModType == ModType.ResourceChangeChanger;
            var noShowSourceNumber = item.Source.Value == 1;
            var noShowIntermediaryNumber = (item.Intermediary?.RuntimeUnit?.Value ?? 1) == 1 || resourceChangeChanger;
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
                else
                {
                    // negative + no parenthesis needs to add a - because the value became positive forcefully
                    modT = "-" + modT;
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

    private static void ShowDialog(MainGameControl mgc, DialogRuntime dialog, JGameControlDataHolder controlData)
    {
        string title = dialog.Title;
        string content = dialog.Content;
        const JGameControlDataHolder.OverlayType overlayType = JGameControlDataHolder.OverlayType.YesNoArcaniaDialog;
        ShowDialog(mgc, title, content, overlayType);
    }

    public static void ShowDialog(MainGameControl mgc, string title, string content, JGameControlDataHolder.OverlayType overlayType)
    {
        var controlData = mgc.JControlData;
        controlData.DialogLayout.LayoutRU.SetTextRaw(0, title);
        controlData.DialogLayout.LayoutRU.SetTextRaw(1, content);
        JGameControlExecuter.ShowOverlay(mgc, overlayType);
        controlData.DialogLayout.LayoutRU.SetVisibleSelf(true);
    }

    public static bool? UpdateExpandLogicForUnit(JRTControlUnit unit)
    {
        var layoutClicked = unit.ExpandWhenClickingLayout?.ClickedLayout ?? false;
        bool expandClick = unit.ExpandButton?.ButtonClicked ?? false;
        expandClick = expandClick || layoutClicked;
        if (expandClick)
        {
            ToggleExpansion(unit);
            return unit.Expanded;
        }
        return null;
    }

    public static void ToggleExpansion(JRTControlUnit unit)
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

    private static void CheckIfNeedsToHideTab(MainGameControl mgc, float maxNumberOfTabsVisible)
    {
        while (mgc.JLayoutRuntime.jLayCanvas.ActiveChildren.Count > maxNumberOfTabsVisible)
        {
            for (int i = mgc.JLayoutRuntime.jLayCanvas.ActiveChildren.Count - 1; i >= 0; i--)
            {
                if (mgc.JLayoutRuntime.jLayCanvas.ActiveChildren[i].Mandatory) continue;
                if (mgc.JLayoutRuntime.jLayCanvas.ActiveChildren[i] == mgc.JLayoutRuntime.jLayCanvas.RequestVisibleNextFrame) continue;
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
