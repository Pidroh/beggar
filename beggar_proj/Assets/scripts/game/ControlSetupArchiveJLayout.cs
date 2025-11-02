using HeartUnity;
using JLayout;

public static class ControlSetupArchiveJLayout
{
    public static void SetupArchiveExclusiveElements(MainGameControl mgc)
    {
        var jControlDataHolder = mgc.JControlData;
        var runtime = jControlDataHolder.LayoutRuntime;
        var jCanvas = runtime.jLayCanvas;
        var layoutMaster = runtime.LayoutMaster;

        for (int tabIndex = 0; tabIndex < jControlDataHolder.TabControlUnits.Count; tabIndex++)
        {
            var tab = jControlDataHolder.TabControlUnits[tabIndex];
            // var tabHolder = jCanvas.children[tabIndex];
            foreach (var sep in tab.SeparatorControls)
            {
                if (!sep.SepD.ArchiveMainUI) continue;
                var tabHolder = sep.SeparatorLayout.ChildSelf;

                var titleTexts = JCanvasMaker.CreateLayout("title_texts", mgc.JLayoutRuntime);
            }

            for (int indexExplorationElement = 0; indexExplorationElement < 2; indexExplorationElement++)
            {

                var parent = JCanvasMaker.CreateLayout("content_holder_expandable", runtime);
                jControlDataHolder.Exploration.ExplorationModeLayouts.Add(parent);
                var layoutRU = parent;
                tabHolder.LayoutRuntimeUnit.AddLayoutAsChild(parent);
                parent.DefaultPositionModes = new PositionMode[] { PositionMode.LEFT_ZERO, PositionMode.SIBLING_DISTANCE };
                parent.ChildSelf.Rect.gameObject.name += " " + (indexExplorationElement == 0 ? "area" : "encounter");
                var expandableTextWithBar = parent.AddLayoutAsChild(JCanvasMaker.CreateLayout("exploration_progress_part_expandable", runtime));
                JLayoutRuntimeUnit layoutThatHasName = expandableTextWithBar.LayoutRU.Children[0].LayoutRU;
                layoutThatHasName.SetTextRaw(0, (indexExplorationElement == 0 ? "area" : "encounter"));
                JRTControlUnit jCU = new();
                jCU.ExpandButton = new JButtonAccessor(expandableTextWithBar.LayoutRU, 0);
                jCU.ExpandButtonImage = new JImageAccessor(expandableTextWithBar.LayoutRU.ButtonChildren[0].Item1, 0);
                jCU.GaugeProgressImage = new JImageAccessor(expandableTextWithBar.LayoutRU.Children[0].LayoutRU.Children[1].LayoutRU, 1);
                jCU.Name = new JLayTextAccessor(layoutThatHasName, 0);
                jCU.ExpandWhenClickingLayout = expandableTextWithBar.LayoutRU;
                jCU.MainLayout = parent;

                {
                    var descLayout = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("lore_text"), runtime);
                    jCU.Description = new JLayTextAccessor(descLayout, 0);
                    AddToExpand(descLayout);
                }
                void AddToExpand(JLayoutRuntimeUnit unit)
                {
                    layoutRU.AddLayoutAsChild(unit);
                    jCU.InsideExpandable.Add(unit);
                    unit.SetParentShowing(false);
                }
                if (indexExplorationElement == 0)
                    jControlDataHolder.Exploration.AreaJCU = jCU;
                if (indexExplorationElement == 1)
                    jControlDataHolder.Exploration.EncounterJCU = jCU;
            }
            var playerParent = JCanvasMaker.CreateLayout("content_holder_expandable", runtime);
            playerParent.DefaultPositionModes = new PositionMode[] {
                PositionMode.CENTER,
                PositionMode.SIBLING_DISTANCE
            };
            jControlDataHolder.Exploration.ExplorationModeLayouts.Add(playerParent);
            tabHolder.LayoutRuntimeUnit.AddLayoutAsChild(playerParent);
            var label = playerParent.AddLayoutAsChild(JCanvasMaker.CreateLayout("exploration_player_upper_label", runtime));
            label.LayoutRU.SetTextRaw(0, "Player");
            foreach (var item in mgc.arcaniaModel.Exploration.Stressors)
            {
                var labelWithBar = JCanvasMaker.CreateLayout("exploration_progress_player_stat", runtime);
                JRTControlUnit jCU = new();
                jCU.GaugeProgressImage = new JImageAccessor(labelWithBar.Children[1].LayoutRU, 1);
                labelWithBar.SetTextRaw(0, item.Name);
                //jCU.Name = new JLayTextAccessor(labelWithBar, 0);
                jCU.MainLayout = labelWithBar;
                playerParent.AddLayoutAsChild(jCU.MainLayout);
                jControlDataHolder.Exploration.StressorJCUs.Add(jCU);
                jCU.Data = item;
            }
            {
                var fleeButtonLayout = JCanvasMaker.CreateLayout("exploration_simple_button", runtime);
                var lc = playerParent.AddLayoutAsChild(fleeButtonLayout);
                fleeButtonLayout.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("Flee"));
                JRTControlUnit jCU = new();
                jCU.MainLayout = fleeButtonLayout;
                jCU.MainExecuteButton = new JButtonAccessor(fleeButtonLayout, 0);
                fleeButtonLayout.ButtonChildren[0].Item1.ImageChildren[1].UiUnit.ActiveSelf = false;
                jControlDataHolder.Exploration.FleeButtonJCU = jCU;
            }
        }

    }
}

