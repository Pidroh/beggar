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
            
            var tabHolder = jCanvas.children[tabIndex];
            foreach (var sep in tab.SeparatorControls)
            {
                if (!sep.SepD.ArchiveMainUI) continue;
                //var sepContentHolder = sep.SeparatorLayout.ChildSelf;
                var layoutD = layoutMaster.LayoutDatas.GetData("content_holder_expandable");
                JLayoutRuntimeUnit heuristicsParent = JCanvasMaker.CreateLayout(layoutD, runtime);
                {
                    var lc = tabHolder.LayoutRuntimeUnit.AddLayoutAsChild(heuristicsParent);
                    lc.PositionModeOverride = new PositionMode[] { PositionMode.CENTER, PositionMode.SIBLING_DISTANCE };
                }
                
                foreach (var heur in jControlDataHolder.archiveControlData.archiveData.euristicDatas)
                {
                    
                    var heuristicLayout = JCanvasMaker.CreateLayout("expandable_resource_text", mgc.JLayoutRuntime);
                    // disable expander button by sprite so the object still exists
                    heuristicLayout.ButtonChildren[0].Item1.ImageChildren[0].UiUnit.Image.enabled = false;
                    var lc = heuristicsParent.AddLayoutAsChild(heuristicLayout);
                    lc.PositionModeOverride = new PositionMode[] { PositionMode.CENTER, PositionMode.SIBLING_DISTANCE };
                    var label = mgc.JControlData.LabelArchiveHeuristicLabel[heur.EuristicType];
                    float percent = ((int)(heur.current * 10000.0f / heur.max)) / 100f;
                    heuristicLayout.SetTextRaw(0, label);
                    heuristicLayout.SetTextRaw(1, $"{heur.current} / {heur.max} ({percent}%)");
                }
                {
                    var exitButtonLayout = JCanvasMaker.CreateLayout("exploration_simple_button", runtime);
                    var lc = tabHolder.LayoutRuntimeUnit.AddLayoutAsChild(exitButtonLayout);
                    lc.PositionModeOverride = new PositionMode[2] { PositionMode.CENTER, PositionMode.SIBLING_DISTANCE };
                    exitButtonLayout.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("Exit archive"));
                    JRTControlUnit jCU = new();
                    jCU.MainLayout = exitButtonLayout;
                    jCU.MainExecuteButton = new JButtonAccessor(exitButtonLayout, 0);
                    jControlDataHolder.archiveControlData.ExitJCU = jCU;
                }
            }
        }
    }
}

