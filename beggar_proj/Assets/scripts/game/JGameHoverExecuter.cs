using HeartUnity.View;
using JLayout;
using UnityEngine;


public static class JGameHoverExecuter 
{
    public static void UpdateHovered(JRTControlUnit unit, MainGameControl mgc) 
    {
        JGameHoverData hoverData = mgc.JControlData.HoverData;
        var changedHoverUnit = hoverData.PreviousHoveredUnit != unit;
        var hl = mgc.JLayoutRuntime.jLayCanvas.HoverLayout;
        hoverData.Title.SetTextRaw(0, unit?.Data?.Name ?? string.Empty);
        hl.SetVisibleSelf(unit != null);
        hoverData.controlUnitForHover.Data = unit?.Data;

        if (unit != null) 
        {

            var offsetX = 20 * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
            var hoverRect = hl.RectTransform;
            hoverRect.SetWidth(JGameControlExecuter.NormalMinTabWidth * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
            var targetRect = unit.MainLayout.RectTransform;
            var root = mgc.JLayoutRuntime.jLayCanvas.RootRT;

            // Align in the root canvas space so differing parents are fine.
            var targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(root, targetRect);
            var targetLeft = targetBounds.min.x - offsetX;
            var targetRight = targetBounds.max.x + offsetX;
            var targetTop = targetBounds.max.y;
            float hoverWidth = hoverRect.GetWidth();
            float hoverHeight = hl.ContentTransform.GetHeight();

            var rootRect = root.rect;
            var spaceRight = rootRect.xMax - targetRight;
            var spaceLeft = targetLeft - rootRect.xMin;

            if (hoverWidth > spaceRight && hoverWidth > spaceLeft) 
            {
                hl.SetVisibleSelf(false);
                MainGameJLayoutPoolExecuter.UpdateHovered(mgc, null);
                return;

            }

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

            // Y positioning with priorities:
            // 1) Top must stay within root top.
            // 2) Keep bottom within root if possible by shifting up.
            // 3) Otherwise keep aligned to target top.
            float desiredTop = targetTop;
            if (desiredTop > rootRect.yMax)
            {
                //desiredTop = rootRect.yMax;
            }

            float resultingBottom = desiredTop - hoverHeight;
            if (resultingBottom < rootRect.yMin)
            {
                
                float delta = rootRect.yMin - resultingBottom;
                desiredTop += delta;
                
                if (desiredTop > rootRect.yMax)
                {
                    desiredTop = rootRect.yMax;
                }
            }

            hoverRect.SetTopLocalY(desiredTop);
        }
        hoverData.PreviousHoveredUnit = unit;
        if (changedHoverUnit) 
        {
            MainGameJLayoutPoolExecuter.UpdateHovered(mgc, null);
            MainGameJLayoutPoolExecuter.UpdateHovered(mgc, unit);
            string humanExpressionNeed = unit?.Data?.ConfigTask?.Need.humanExpression;
            hoverData.NeedLay.SetTextRaw(0, humanExpressionNeed == null ? "" : "Needs " + humanExpressionNeed);
            hoverData.RequireLay.SetTextRaw(0, MainGameControlSetupJLayout.GetRequiredOfTarget(unit?.Data));
            hoverData.TagLay.SetTextRaw(0, MainGameControlSetupJLayout.GetTagText(unit?.Data));

        }
        #region update UI values like change list and mods
        if (unit != null)
        {
            JGameControlExecuter.UpdateExpandedUI(hoverData.controlUnitForHover);
        }
        #endregion


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
