using JLayout;

public static class JGameHoverExecuter 
{
    public static void UpdateHovered(JRTControlUnit unit, MainGameControl mgc) 
    {
        mgc.JControlData.HoverData.Title.SetTextRaw(0, unit?.Data?.Name ?? string.Empty);
    }
}

public class JGameHoverData
{
    public JLayoutRuntimeUnit Title { get; internal set; }
}

public static class JGameHoverSetup 
{
    public static void Setup(MainGameControl control) 
    {
        control.JControlData.HoverData = new JGameHoverData();
        JLayoutRuntimeData runtime = control.JLayoutRuntime;
        var layoutMaster = runtime.LayoutMaster;
        var layoutTitle = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("above_button_title_with_value"), runtime);
        runtime.jLayCanvas.HoverLayout.AddLayoutAsChild(layoutTitle);
        control.JControlData.HoverData.Title = layoutTitle;
    }
}