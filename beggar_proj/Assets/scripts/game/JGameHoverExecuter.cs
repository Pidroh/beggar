using HeartUnity.View;
using JLayout;
using UnityEngine;


public static class JGameHoverExecuter 
{
    public static void UpdateHovered(JRTControlUnit unit, MainGameControl mgc) 
    {
        var changedHoverUnit = mgc.JControlData.HoverData.PreviousHoveredUnit != unit;
        var hl = mgc.JLayoutRuntime.jLayCanvas.HoverLayout;
        mgc.JControlData.HoverData.Title.SetTextRaw(0, unit?.Data?.Name ?? string.Empty);
        hl.SetVisibleSelf(unit != null);
        mgc.JControlData.HoverData.controlUnitForHover.Data = unit?.Data;

        if (unit != null) 
        {
            
            var hoverRect = hl.RectTransform;
            hoverRect.SetWidth(JGameControlExecuter.NormalMinTabWidth * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
            var targetRect = unit.MainLayout.RectTransform;
            var root = mgc.JLayoutRuntime.jLayCanvas.RootRT;

            // Align in the root canvas space so differing parents are fine.
            var targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(root, targetRect);
            var targetLeft = targetBounds.min.x;
            var targetRight = targetBounds.max.x;
            float hoverWidth = hoverRect.GetWidth();

            var rootRect = root.rect;
            var spaceRight = rootRect.xMax - targetRight;
            var spaceLeft = targetLeft - rootRect.xMin;

            // Prefer the side that fits; otherwise choose the roomier side.
            bool placeToRight = spaceRight >= hoverWidth || spaceRight >= spaceLeft;
            if (placeToRight)
            {
                // Hover left edge touches target right edge
                hoverRect.SetLeftLocalX(targetRight);
            }
            else
            {
                // Hover right edge touches target left edge (compute via left edge to avoid pivot quirks)
                hoverRect.SetLeftLocalX(targetLeft - hoverWidth);
            }
        }
        mgc.JControlData.HoverData.PreviousHoveredUnit = unit;
        if (changedHoverUnit) 
        {
            MainGameJLayoutPoolExecuter.UpdateHovered(mgc, null);
            MainGameJLayoutPoolExecuter.UpdateHovered(mgc, unit);
        }
            
    }
}

public class JGameHoverData
{
    public JLayoutRuntimeUnit Title { get; internal set; }
    public JLayoutRuntimeUnit MainLayout { get; internal set; }
    public JLayoutRuntimeUnit NeedLay { get; internal set; }
    public JLayoutRuntimeUnit RequireLay { get; internal set; }
    public JLayoutRuntimeUnit TagLay { get; internal set; }
    public JRTControlUnit PreviousHoveredUnit { get; internal set; }

    public JRTControlUnit controlUnitForHover = new();
}

public static class JGameHoverSetup 
{
    public static void Setup(MainGameControl control) 
    {
        control.JControlData.HoverData = new JGameHoverData();
        JLayoutRuntimeData runtime = control.JLayoutRuntime;
        var layoutMaster = runtime.LayoutMaster;
        JLayoutRuntimeUnit hoverLayout = runtime.jLayCanvas.HoverLayout;

        

        var expandableLayout = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("content_holder_expandable_hover"), runtime);
        var layoutTitle = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("above_button_title_with_value"), runtime);

        expandableLayout.DefaultPositionModes = new PositionMode[] { PositionMode.LEFT_ZERO, PositionMode.SIBLING_DISTANCE };

        hoverLayout.AddLayoutAsChild(expandableLayout);
        expandableLayout.AddLayoutAsChild(layoutTitle);

        control.JControlData.HoverData.MainLayout = expandableLayout;
        control.JControlData.HoverData.Title = layoutTitle;
        control.JControlData.HoverData.controlUnitForHover.MainLayout = expandableLayout;
        {
            // need, require, tag
            for (int i = 0; i < 3; i++)
            {
                var needLay = JCanvasMaker.CreateLayout(layoutMaster.LayoutDatas.GetData("quantity_task_text"), runtime);
                expandableLayout.AddLayoutAsChild(needLay);
                if(i == 0) control.JControlData.HoverData.NeedLay = needLay;
                if (i == 1) control.JControlData.HoverData.RequireLay = needLay;
                if (i == 2) control.JControlData.HoverData.TagLay = needLay;
            }
        }
    }
}
