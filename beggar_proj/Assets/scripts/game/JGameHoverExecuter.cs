using HeartUnity.View;
using JLayout;
using UnityEngine;

public static class JGameHoverExecuter 
{
    public static void UpdateHovered(JRTControlUnit unit, MainGameControl mgc) 
    {
        var hl = mgc.JLayoutRuntime.jLayCanvas.HoverLayout;
        mgc.JControlData.HoverData.Title.SetTextRaw(0, unit?.Data?.Name ?? string.Empty);
        hl.SetVisibleSelf(unit != null);

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
