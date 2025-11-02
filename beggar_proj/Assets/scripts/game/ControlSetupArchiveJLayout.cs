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
                var sepContentHolder = sep.SeparatorLayout.ChildSelf;

                var titleTexts = JCanvasMaker.CreateLayout("title_texts", mgc.JLayoutRuntime);
                {
                    var exitButtonLayout = JCanvasMaker.CreateLayout("exploration_simple_button", runtime);
                    var lc = sepContentHolder.LayoutRU.AddLayoutAsChild(exitButtonLayout);
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

