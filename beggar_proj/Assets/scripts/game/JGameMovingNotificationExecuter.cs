using JLayout;

public static class JGameMovingNotificationExecuter 
{
    public static void ManualUpdate(MainGameControl mgc) 
    {
        if (mgc.arcaniaModel.notificationData.notificationUnits.Count == 0) return;



    }
}

public class MovingNotificationData
{
    public JLayoutRuntimeUnit ParentLayout { get; internal set; }
    public JLayoutRuntimeUnit ExpandableLayout { get; internal set; }
}

public static class MovingNotificationSetup 
{
    public static void Setup(MainGameControl mgc) 
    {
        var control = mgc;
        JLayoutRuntimeData runtime = control.JLayoutRuntime;
        var layoutMaster = runtime.LayoutMaster;
        var freeLayout = JCanvasMaker.CreateVariousFreeLayout(runtime);

        var expandableLayout = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("content_holder_expandable_hover"), runtime);
        expandableLayout.DefaultPositionModes = new PositionMode[] { PositionMode.LEFT_ZERO, PositionMode.SIBLING_DISTANCE };

        freeLayout.AddLayoutAsChild(expandableLayout);

        mgc.JControlData.MovingNotificationData.ParentLayout = freeLayout;
        mgc.JControlData.MovingNotificationData.ExpandableLayout = freeLayout;
    }
}
