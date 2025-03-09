using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using static JLayout.JLayoutRuntimeData;
using HeartUnity.View;
using TMPro;

namespace JLayout
{
    public static class JLayoutRuntimeExecuter
    {
        public static void ManualUpdate(JLayoutRuntimeData data)
        {
            foreach (var mainCanvasChild in data.jLayCanvas.ActiveChildren)
            {
                JLayoutRuntimeUnit parentLayout = mainCanvasChild;
                // temporary code
                parentLayout.RectTransform.SetWidth(320 * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                parentLayout.ContentTransform.SetWidth(320 * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                parentLayout.RectTransform.SetLeftXToParent(0);
                mainCanvasChild.RectTransform.gameObject.SetActive(mainCanvasChild.Children.Count > 0);
                if (mainCanvasChild.Children.Count == 0) continue;

                // layout code
                var contentRect = parentLayout.ContentTransform;

                #region solve layout width
                SolveLayoutWidth(parentLayout, contentRect);
                #endregion

                TemporarySolveHeightAndPosition(parentLayout, contentRect);
            }
        }

        private static void TemporarySolveHeightAndPosition(JLayoutRuntimeUnit parentLayout, RectTransform contentRect)
        {
            var defaultPositionModes = parentLayout.DefaultPositionModes;
            JLayoutChild previousChild = null;
            float totalChildHeight = 0f;
            var padding = parentLayout.LayoutData.commons.Padding;
            foreach (var child in parentLayout.Children)
            {
                if (child.LayoutRU != null)
                {
                    TemporarySolveHeightAndPosition(child.LayoutRU, child.LayoutRU.ContentTransform);
                }
                RectTransform childRect = child.Rect;
                

                #region size
                var axisModes = child.Commons.AxisModes;
                var yAxis = 1;

                var axisM = axisModes[yAxis];
                switch (axisM)
                {
                    case AxisMode.PARENT_SIZE_PERCENT:
                        // var sizeRatio = 1f;
                        if (yAxis == 0)
                        {
                        }
                        else
                        {
                            childRect.SetHeight(contentRect.GetHeight());
                        }
                        break;
                    case AxisMode.SELF_SIZE:
                        childRect.SetHeight(child.Commons.Size[yAxis] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                        break;
                    case AxisMode.CONTAIN_CHILDREN:
                        break;
                    case AxisMode.FILL_REMAINING_SIZE:
                        break;
                    case AxisMode.TEXT_PREFERRED:
                        break;
                    default:
                        break;
                }

                totalChildHeight += childRect.GetHeight();

                #endregion

                #region position
                var positionModes = child.PositionModes ?? defaultPositionModes;
                for (int axis = 0; axis < 2; axis++)
                {
                    var pm = positionModes[axis];
                    switch (pm)
                    {
                        case PositionMode.LEFT_ZERO:
                            Debug.Assert(axis == 0);
                            // If this breaks because it's messing up with the anchors, you can create a variation where you take into account parent width
                            // that should allow you to write the code without changing the anchors? Pivot? etc
                            childRect.SetLeftXToParent(padding.left * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                            break;
                        case PositionMode.RIGHT_ZERO:
                            Debug.Assert(axis == 0);
                            childRect.SetRightXToParent(padding.right * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                            break;
                        case PositionMode.CENTER:
                            //childRect.SetPivotAndAnchors(new Vector2(0.5f, 0.5f));
                            childRect.SetAnchorsByIndex(axis, 0.5f);
                            childRect.SetPivotByIndex(axis, 0.5f);
                            if (axis == 0)
                                childRect.SetLocalX(0);
                            if (axis == 1)
                                childRect.SetLocalY(0);

                            break;
                        case PositionMode.SIBLING_DISTANCE:
                            var prevRect = previousChild?.Rect;
                            if (prevRect != null)
                            {
                                if (axis == 0)
                                {
                                    childRect.SetLeftLocalX(prevRect.GetRightLocalX());
                                }
                                if (axis == 1)
                                {
                                    childRect.SetTopLocalY(prevRect.GetBottomLocalY());
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
                parentLayout.ContentTransform.SetHeight(totalChildHeight + padding.vertical * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
            }

        }

        private static void SolveLayoutWidth(JLayoutRuntimeUnit parentLayout, RectTransform parentRect)
        {
            var widthOfContentPhysical = parentRect.GetWidth() - parentLayout.LayoutData.commons.Padding.horizontal * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
            var widthOfContentForComsumptionPhysical = widthOfContentPhysical;
            using var _1 = ListPool<JLayoutChild>.Get(out var fillUpChildren);
            foreach (var child in parentLayout.Children)
            {
                switch (child.Commons.AxisModes[0])
                {
                    case AxisMode.PARENT_SIZE_PERCENT:
                        child.Rect.SetWidth(widthOfContentPhysical);
                        widthOfContentForComsumptionPhysical = 0;
                        break;
                    case AxisMode.SELF_SIZE:
                        float actualWidth = child.Commons.Size[0] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
                        child.Rect.SetWidth(actualWidth);
                        widthOfContentForComsumptionPhysical -= actualWidth;
                        break;
                    case AxisMode.CONTAIN_CHILDREN:
                        Debug.LogError("Not supported");
                        break;
                    case AxisMode.FILL_REMAINING_SIZE:
                        fillUpChildren.Add(child);
                        break;
                    case AxisMode.TEXT_PREFERRED:
                        Debug.LogError("Not supported");
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
    public class JLayoutRuntimeData
    {
        public JLayCanvas jLayCanvas;

        public TMP_FontAsset DefaultFont { get; internal set; }

        public class JLayoutRuntimeUnit
        {
            public RectTransform RectTransform;
            //public List<JLayoutRuntimeUnit> Sublayouts = new();
            public List<JLayoutChild> Children = new();
            public List<JLayoutChild> TextChildren = new();
            public List<JLayoutRuntimeUnit> ButtonChildren = new();

            public JLayoutRuntimeUnit(RectTransform childRT2)
            {
                RectTransform = childRT2;
            }

            public RectTransform ContentTransformOverride { get; internal set; }
            public LayoutData LayoutData { get; internal set; }
            public RectTransform ContentTransform => ContentTransformOverride ?? RectTransform;
            public AxisMode[] OverrideAxisMode { internal get; set; }
            public AxisMode[] AxisMode => OverrideAxisMode ?? LayoutData.commons?.AxisModes;

            public PositionMode[] DefaultPositionModes { get; internal set; }

            internal void AddChild(JLayoutChild child)
            {
                Children.Add(child);
                child.Rect.SetParent(RectTransform);
            }

            internal void AddLayoutAsChild(JLayoutRuntimeUnit layoutRU, ChildAddParameters? param = null)
            {
                var commons = layoutRU.LayoutData.commons;
                AddLayoutAsChild(layoutRU, commons, param);
            }

            // Some day you might have to fuse the buttonLayout commons with childData commons
            internal void AddLayoutAsChild(JLayoutRuntimeUnit buttonLayout, LayoutChildData childData) => AddLayoutAsChild(buttonLayout, childData.Commons, null);

            internal void BindButton(JLayoutRuntimeUnit buttonLayout)
            {
                ButtonChildren.Add(buttonLayout);
            }

            internal void BindText(JLayoutChild textChild)
            {
                TextChildren.Add(textChild);
            }

            internal void SetText(int v, string textKey)
            {
                // localize this?
                TextChildren[v].UiUnit.rawText = textKey;
            }

            private JLayoutChild AddLayoutAsChild(JLayoutRuntimeUnit layoutRU, LayoutCommons commons, ChildAddParameters? param)
            {
                bool differingCommons = layoutRU.LayoutData?.commons != commons;
                if (differingCommons && layoutRU.LayoutData?.commons?.AxisModes != null && commons.AxisModes != null)
                {
                    Debug.LogError("two axis modes!");
                }
                if (differingCommons && commons.AxisModes != null)
                {
                    layoutRU.OverrideAxisMode = commons.AxisModes;
                }
                JLayoutChild item = new JLayoutChild()
                {
                    LayoutRU = layoutRU,
                    Commons = commons
                };
                Children.Add(item);
                layoutRU.RectTransform.SetParent(ContentTransform);
                if (!param.HasValue) return item;
                item.PositionModeOverride = param.Value.PositionModeOverride;
                return item;

            }

            public struct ChildAddParameters
            {
                public PositionMode[] PositionModeOverride;
            }
        }

        public class JLayoutChild
        {
            public LayoutChildData LayoutChild;

            public JLayoutRuntimeUnit LayoutRU { get; internal set; }
            public LayoutCommons Commons { get; internal set; }
            public UIUnit UiUnit;
            public RectTransform Rect => LayoutRU?.RectTransform ?? UiUnit?.RectTransform;
            public PositionMode[] PositionModeOverride;

            public PositionMode[] PositionModes => PositionModeOverride ?? Commons.PositionModes;
        }
    }

    public class JLayCanvas
    {
        public GameObject canvasGO;
        public Canvas Canvas { get; internal set; }
        public RectTransform RootRT { get; internal set; }
        public List<JLayoutRuntimeUnit> children = new List<JLayoutRuntimeUnit>();
        public List<JLayoutRuntimeUnit> childrenForLayouting = new List<JLayoutRuntimeUnit>();
        public RectTransform OverlayRoot { get; internal set; }
        public Queue<JLayoutRuntimeUnit> ActiveChildren = new();

        internal void ShowOverlay() => OverlayRoot.gameObject.SetActive(true);

        internal void HideOverlay() => OverlayRoot.gameObject.SetActive(false);

        private void HideChild(JLayoutRuntimeUnit layoutParent)
        {
            using var _1 = ListPool<JLayoutRuntimeUnit>.Get(out var list);
            list.AddRange(ActiveChildren);
            ActiveChildren.Clear();
            foreach (var item in list)
            {
                if (item == layoutParent) continue;
                ActiveChildren.Enqueue(item);
            }
        }

        internal void ShowChild(JLayoutRuntimeUnit layoutParent)
        {
            if (ActiveChildren.Contains(layoutParent)) return;
            while (childrenForLayouting.Remove(layoutParent)) { }
            childrenForLayouting.Insert(0, layoutParent);
            ActiveChildren.Enqueue(layoutParent);
        }
    }
}
