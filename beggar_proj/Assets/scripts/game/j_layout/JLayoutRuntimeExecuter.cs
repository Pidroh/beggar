using UnityEngine;
using UnityEngine.Pool;
using HeartUnity.View;
using TMPro;

namespace JLayout
{
    public static class JLayoutRuntimeExecuter
    {
        public static void ManualUpdate(JLayoutRuntimeData data)
        {
            var offsetLeftX = 0f;
            var offsetBottomY = 0f;
            using var _1 = DictionaryPool<Direction, float>.Get(out var offsetDirections);

            // multi direction loop eventually
            // foreach (var d in EnumHelper<Direction>.GetAllValues())
            {
                Direction d = Direction.WEST;
                var menus = data.jLayCanvas.FixedMenus[d];
                foreach (var c in menus)
                {
                    if (!c.LayoutRuntimeUnit.Visible) continue;
                    var item = c.LayoutRuntimeUnit;
                    item.RectTransform.FillParentHeight();
                    float axisSize = item.LayoutData.commons.Size[0] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
                    item.RectTransform.SetWidth(axisSize);
                    item.RectTransform.SetLeftXToParent(offsetLeftX);
                    offsetLeftX += axisSize;
                    ProcessChildren(item);
                }
            }
            {
                Direction d = Direction.SOUTH;
                var menus = data.jLayCanvas.FixedMenus[d];
                foreach (var c in menus)
                {
                    if (!c.LayoutRuntimeUnit.Visible) continue;
                    var item = c.LayoutRuntimeUnit;
                    var nonFixedAxis = 0;
                    var fixedAxis = 1;
                    item.RectTransform.FillParentByAxisIndex(nonFixedAxis);
                    float axisSize = item.LayoutData.commons.Size[fixedAxis] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
                    item.RectTransform.SetHeight(axisSize);
                    item.RectTransform.SetBottomYToParent(offsetBottomY);
                    offsetBottomY += axisSize;
                    // offset += axisSize;
                    ProcessChildren(item);
                }
            }

            foreach (var mainCanvasChild in data.jLayCanvas.childrenForLayouting)
            {
                JLayoutRuntimeUnit parentLayout = mainCanvasChild.LayoutRuntimeUnit;
                parentLayout.SetVisibleSelf(data.jLayCanvas.ActiveChildren.Contains(mainCanvasChild) && parentLayout.Children.Count > 0);
                if (!parentLayout.Visible)
                {
                    continue;
                }

                // temporary code
                float newSize = mainCanvasChild.DesiredSize;
                parentLayout.RectTransform.SetWidth(newSize);
                // parentLayout.ContentTransform.SetWidth(320 * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                parentLayout.RectTransform.SetLeftXToParent(offsetLeftX);
                parentLayout.RectTransform.SetOffsetMinByIndex(1, offsetBottomY);
                offsetLeftX += newSize;
                ProcessChildren(parentLayout);
            }

            foreach (var overlay in data.jLayCanvas.Overlays)
            {
                overlay.LayoutRuntimeUnit.RectTransform.FillParent();
                ProcessChildren(overlay.LayoutRuntimeUnit);
            }
        }

        private static void ProcessChildren(JLayoutRuntimeUnit parentLayout)
        {
            // layout code
            var contentRect = parentLayout.ContentTransform;
            // width is solved first so you can know how wide text is
            #region solve layout width
            SolveLayoutWidth(parentLayout, contentRect);
            #endregion

            TemporarySolveHeightAndPosition(parentLayout, contentRect);

            #region click color
            ProcessColor(parentLayout, null);
            void ProcessColor(JLayoutRuntimeUnit lay, bool? active)
            {
                if (lay.ActivePowered.HasValue)
                {
                    active = lay.ActivePowered;
                }
                foreach (var item in lay.Children)
                {
                    if (item.LayoutRU == null) continue;
                    ProcessColor(item.LayoutRU, active);
                }
                var hasButton = lay.TryGetSelfButton(out UIUnit buttonUU);

                var thisActive = active;
                ColorSetType color = ColorSetType.NORMAL;
                if (lay.Disabled is true || (hasButton && !buttonUU.ButtonEnabled))
                {
                    color = ColorSetType.DISABLED;
                } else if (hasButton && buttonUU.Clicked)
                {
                    color = ColorSetType.CLICKED;
                } if (hasButton && buttonUU.MouseDown)
                {
                    color = ColorSetType.PRESSED;
                }
                else if (lay.Hovered)
                {
                    color = ColorSetType.HOVERED;
                }
                else if (thisActive is true) 
                {
                    color = ColorSetType.ACTIVE;
                }
                foreach (var c in lay.Children)
                {
                    if (c.LayoutRU != null && c.LayoutRU.Disabled is true)
                    {
                        c.ApplyColor(ColorSetType.DISABLED);
                        continue;
                    }
                    c.ApplyColor(color);
                }

                /*
                foreach (var item in lay.ButtonChildren)
                {
                    var thisActive = active;
                    if (item.Item1.Active.HasValue)
                    {
                        thisActive = item.Item1.Active;
                    }
                    ColorSetType color = ColorSetType.NORMAL;
                    if (!item.Item2.UiUnit.ButtonEnabled || lay.Disabled is true)
                    {
                        color = ColorSetType.DISABLED;
                    }
                    else if (item.Item2.UiUnit.Clicked)
                    {
                        color = ColorSetType.CLICKED;
                    }
                    else if (item.Item2.UiUnit.MouseDown)
                    {
                        color = ColorSetType.PRESSED;
                    }
                    else if (item.Item2.UiUnit.HoveredWhileVisible)
                    {
                        color = ColorSetType.HOVERED;
                    }
                    else if (thisActive.HasValue && thisActive.Value)
                    {
                        color = ColorSetType.ACTIVE;
                    }
                    foreach (var c in item.Item1.Children)
                    {
                        if (c.LayoutRU != null && c.LayoutRU.Disabled is true)
                        {
                            c.ApplyColor(ColorSetType.DISABLED);
                            continue;
                        }
                        c.ApplyColor(color);
                    }
                }
                */
            }
            #endregion
        }

        private static void TemporarySolveHeightAndPosition(JLayoutRuntimeUnit parentLayout, RectTransform contentRect)
        {
            var defaultPositionModes = parentLayout.DefaultPositionModes;
            JLayoutChild previousChild = null;
            float totalChildOccupiedHeight = 0f;
            var padding = parentLayout.LayoutData.commons.Padding;

            foreach (var child in parentLayout.Children)
            {
                if (!child.LayoutRU?.Visible ?? false) continue;
                if (child.LayoutRU != null)
                {
                    TemporarySolveHeightAndPosition(child.LayoutRU, child.LayoutRU.ContentTransform);
                }
                RectTransform childRect = child.Rect;


                #region size
                var axisModes = child.AxisModes;
                var yAxis = 1;

                var accountForTotalSize = true;

                var axisM = axisModes[yAxis];
                float? height = null;
                switch (axisM)
                {
                    case AxisMode.PARENT_SIZE_PERCENT:
                        height = contentRect.GetHeight() - padding.vertical * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
                        accountForTotalSize = false;
                        break;
                    case AxisMode.PARENT_SIZE_PERCENT_RAW:
                        // var sizeRatio = 1f;
                        // doing this in the code above, width is solved first (because of text reasons)
                        if (yAxis == 0)
                        {
                        }
                        else
                        {
                            height = contentRect.GetHeight();
                        }
                        accountForTotalSize = false;
                        break;
                    case AxisMode.SELF_SIZE:
                        height = child.Commons.Size[yAxis] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
                        break;
                    case AxisMode.CONTAIN_CHILDREN:
                        break;
                    case AxisMode.FILL_REMAINING_SIZE:
                        Debug.LogError("Not supported yet (will complicate the implementation)");
                        break;
                    case AxisMode.TEXT_PREFERRED:
                        height = child.UiUnit.text.preferredHeight;
                        break;
                    default:
                        break;
                }
                if (height.HasValue)
                {
                    height = Mathf.Max(height.Value, child.Commons.MinSize[1] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                    child.Rect.SetHeight(height.Value);
                }
                else
                {
                    height = child.Rect.GetHeight();
                }

                #endregion

                var positionModes = child.PositionModes ?? defaultPositionModes;

                #region total height calculation
                if (accountForTotalSize)
                {
                    if (positionModes[1] == PositionMode.SIBLING_DISTANCE)
                    {
                        totalChildOccupiedHeight += height.Value;
                    }
                    else
                    {
                        totalChildOccupiedHeight = Mathf.Max(height.Value, totalChildOccupiedHeight);
                    }
                }

                #endregion

                #region position calculation

                for (int axis = 0; axis < 2; axis++)
                {
                    var pm = positionModes[axis];
                    var pos = child.Commons.PositionOffsets;
                    switch (pm)
                    {
                        case PositionMode.LEFT_ZERO:
                            Debug.Assert(axis == 0);
                            // If this breaks because it's messing up with the anchors, you can create a variation where you take into account parent width
                            // that should allow you to write the code without changing the anchors? Pivot? etc
                            childRect.SetLeftXToParent((padding.left + pos.x) * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                            break;
                        case PositionMode.RIGHT_ZERO:
                            Debug.Assert(axis == 0);
                            childRect.SetRightXToParent((padding.right + pos.x) * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                            break;
                        case PositionMode.TOP_ZERO:
                            {
                                Debug.Assert(axis == 1);
                                childRect.SetTopYToParent((padding.top + pos.y) * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                            }
                            break;
                        case PositionMode.BOTTOM_ZERO:
                            {
                                Debug.Assert(axis == 1);
                                childRect.SetBottomYToParent((padding.bottom + pos.y) * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                            }
                            break;
                        case PositionMode.CENTER:
                            //childRect.SetPivotAndAnchors(new Vector2(0.5f, 0.5f));
                            childRect.SetAnchorsByIndex(axis, 0.5f);
                            childRect.SetPivotByIndex(axis, 0.5f);
                            if (axis == 0)
                                childRect.SetLocalX((padding.left - padding.right + pos.x) * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                            if (axis == 1)
                                childRect.SetLocalY((padding.bottom - padding.top + pos.y) * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);

                            break;
                        case PositionMode.CENTER_RAW:
                            childRect.SetAnchorsByIndex(axis, 0.5f);
                            childRect.SetPivotByIndex(axis, 0.5f);
                            if (axis == 0)
                                childRect.SetLocalX(0);
                            if (axis == 1)
                                childRect.SetLocalY(0);
                            break;
                        case PositionMode.RAW_FOR_GAUGE:
                            if (axis == 0)
                                childRect.SetLeftXToParent(pos.x * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                            else
                                Debug.LogError("Not supported yet");
                            break;
                        case PositionMode.FOR_GAUGE:
                            if (axis == 0)
                                childRect.SetLeftXToParent(pos.x * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize + padding.left * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                            else
                                Debug.LogError("Not supported yet");
                            break;
                        case PositionMode.SIBLING_DISTANCE_REVERSE:
                            {
                                var prevRect = previousChild?.Rect;
                                if (prevRect != null)
                                {
                                    if (axis == 0)
                                    {
                                        childRect.SetRightLocalX(prevRect.GetLeftLocalX() - child.Commons.PositionOffsets[0] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                                    }
                                    if (axis == 1)
                                    {
                                        childRect.SetBottomLocalY(prevRect.GetTopLocalY() + child.Commons.PositionOffsets[1] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                                    }
                                }
                                else
                                {
                                    if (axis == 0)
                                    {
                                        childRect.SetRightXToParent(padding.left * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                                    }
                                    if (axis == 1)
                                    {
                                        childRect.SetBottomYToParent(padding.top * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                                    }
                                }
                            }
                            break;
                        case PositionMode.SIBLING_DISTANCE:
                            {
                                var prevRect = previousChild?.Rect;
                                if (prevRect != null)
                                {
                                    if (axis == 0)
                                    {
                                        childRect.SetLeftLocalX(prevRect.GetRightLocalX() + child.Commons.PositionOffsets[0] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                                    }
                                    if (axis == 1)
                                    {
                                        totalChildOccupiedHeight += child.Commons.PositionOffsets[1];
                                        childRect.SetTopLocalY(prevRect.GetBottomLocalY() - child.Commons.PositionOffsets[1] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                                    }
                                }
                                else
                                {
                                    if (axis == 0)
                                    {
                                        childRect.SetLeftXToParent(padding.left * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                                    }
                                    if (axis == 1)
                                    {
                                        childRect.SetTopYToParent(padding.top * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                #endregion
                previousChild = child;
            }

            AxisMode? heightAxis = parentLayout.AxisMode?[1];
            if (heightAxis == null)
            {
                Debug.LogError("!!");
            }
            Debug.Assert(heightAxis != null);
            if (heightAxis == AxisMode.CONTAIN_CHILDREN)
            {
                float height = totalChildOccupiedHeight + padding.vertical * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
                height = Mathf.Max(height, parentLayout.LayoutData.commons.MinSize[1] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                parentLayout.ContentTransform.SetHeight(height);
            }

        }

        private static void SolveLayoutWidth(JLayoutRuntimeUnit parentLayout, RectTransform parentRect)
        {
            var widthOfContentPhysical = parentRect.GetWidth() - (parentLayout.LayoutData.commons.Padding.left + parentLayout.LayoutData.commons.Padding.right) * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
            var widthOfContentForComsumptionPhysical = widthOfContentPhysical;
            using var _1 = ListPool<JLayoutChild>.Get(out var fillUpChildren);
            foreach (var child in parentLayout.Children)
            {
                switch (child.AxisModes[0])
                {
                    case AxisMode.PARENT_SIZE_PERCENT_RAW:
                        child.Rect.SetWidth(parentRect.GetWidth() * child.SizeRatioAsGauge);
                        break;
                    case AxisMode.PARENT_SIZE_PERCENT:
                        child.Rect.SetWidth(widthOfContentPhysical * child.SizeRatioAsGauge);
                        widthOfContentForComsumptionPhysical = 0;
                        break;
                    case AxisMode.SELF_SIZE:
                        float actualWidth = child.Commons.Size[0] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
                        child.Rect.SetWidth(actualWidth);
                        widthOfContentForComsumptionPhysical -= actualWidth;
                        break;
                    case AxisMode.CONTAIN_CHILDREN:
                        Debug.LogError("Not supported for width. Will complicate the implementation.");
                        break;
                    case AxisMode.FILL_REMAINING_SIZE:
                        fillUpChildren.Add(child);
                        break;
                    case AxisMode.TEXT_PREFERRED:
                        Debug.LogError("Not supported");
                        break;
                    case AxisMode.STEP_SIZE_TEXT:
                        if (!child.OnMaxStep(0))
                        {
                            var preferredWidth = child.UiUnit.text.preferredWidth;
                            var stepSizes = child.Commons.StepSizes;
                            var preferredSize = stepSizes[0][0];
                            int preferredIndex = 0;
                            for (int i = 0; i < stepSizes[0].Count; i++)
                            {
                                if (preferredSize < (stepSizes[0][i] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize - 10))
                                {
                                    preferredSize = (int)(stepSizes[0][i] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                                    preferredIndex = i;
                                    break;
                                }
                            }
                            child.UiUnit.RectTransform.SetWidth(preferredSize);
                            child.SetCurrentStep(0, preferredIndex);

                            child.UiUnit.text.textWrappingMode = child.OnMaxStep(0) ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
                        }
                        break;
                    default:
                        break;
                }
            }
            Debug.Assert(fillUpChildren.Count <= 1);

            if (fillUpChildren.Count == 1)
            {
                fillUpChildren[0].Rect.SetWidth(widthOfContentForComsumptionPhysical);
            }

            foreach (var child in parentLayout.Children)
            {
                if (child.LayoutRU != null)
                {
                    SolveLayoutWidth(child.LayoutRU, child.LayoutRU.ContentTransform);
                }
            }
        }
    }
}
