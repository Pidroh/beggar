using JLayout;
using System;
using System.Collections.Generic;

public static class JGameMovingNotificationExecuter 
{
    public static void ManualUpdate(MainGameControl mgc) 
    {
        if (mgc.arcaniaModel.notificationData.notificationUnits.Count == 0) return;
        var notification = mgc.arcaniaModel.notificationData.notificationUnits[0];
        var notificationUID = mgc.JControlData.MovingNotificationData;

        if (notificationUID.notificationUnits.Count == 0) 
        {
            notificationUID.notificationUnits.Add(new());
        }
        var uiUnit = notificationUID.notificationUnits[0];

        while (uiUnit.ttvs.Count < notification.Subunits.Count) 
        {
            var layout = JCanvasMaker.CreateLayout("in_header_triple_statistic", mgc.JControlData.LayoutRuntime);
            JLayoutRuntimeUnit ttv = layout;
            uiUnit.ttvs.Add(ttv);
            notificationUID.ExpandableLayout.AddLayoutAsChild(ttv);
        }
        for (int i = 0; i < uiUnit.ttvs.Count; i++)
        {
            JLayoutRuntimeUnit ttv = uiUnit.ttvs[i];
            if (notification.Subunits.Count <= i) 
            {
                continue;
            }
            var su = notification.Subunits[i];
            ttv.SetTextRaw(0, su.target.RuntimeUnit?.Name ?? su.target.Tag?.tagName ?? "");
            ttv.SetTextRaw(1, ""+su.value);
        }
    }

    internal static void ReportUnitWithNotification(MainGameControl mgc, JRTControlUnit item, int notificationPos)
    {
        if (notificationPos >= mgc.JControlData.MovingNotificationData.notificationUnits.Count) return;
        var not = mgc.JControlData.MovingNotificationData.notificationUnits[notificationPos];
        var notificationLayout = mgc.JControlData.MovingNotificationData.ExpandableLayout;
        var unitLayout = item.MainLayout;

    }
}

public class MovingNotificationData
{
    public List<MovingNotificationDataUnit> notificationUnits = new();
    public JLayoutRuntimeUnit ParentLayout { get; internal set; }
    public JLayoutRuntimeUnit ExpandableLayout { get; internal set; }
}

public class MovingNotificationDataUnit 
{
    public List<JLayoutRuntimeUnit> ttvs = new();
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
