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
                parentLayout.RectTransform.SetWidth(320 * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                parentLayout.ContentTransform.SetWidth(320 * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                

                // layout code
                var parentRect = parentLayout.ContentTransform;

                var defaultPositionModes = parentLayout.DefaultPositionModes;
                #region solve layout width
                SolveLayoutWidth(parentLayout, parentRect);
                #endregion

                JLayoutChild previousChild = null;
                foreach (var child in parentLayout.Children)
                {
                    RectTransform childRect = child.Rect;
                    var padding = parentLayout.LayoutData.commons.Padding;

                    #region size
                    var axisModes = child.Commons.AxisModes;
                    for (int axis = 0; axis < axisModes.Length; axis++)
                    {
                        var axisM = axisModes[axis];
                        switch (axisM)
                        {
                            case AxisMode.PARENT_SIZE_PERCENT:
                                // var sizeRatio = 1f;
                                if (axis == 0)
                                {
                                    childRect.SetWidth(parentRect.GetWidth());
                                }
                                else
                                {
                                    childRect.SetHeight(parentRect.GetHeight());
                                }
                                break;
                            case AxisMode.SELF_SIZE:
                                childRect.SetSizeMilimeters(axis, child.Commons.Size[axis]);
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
                    }
                    #endregion

                    #region position
                    var positionModes = child.Commons.PositionModes ?? defaultPositionModes;
                    for (int axis = 0; axis < 2; axis++)
                    {
                        var pm = positionModes[axis];
                        switch (pm)
                        {
                            case PositionMode.LEFT_ZERO:
                                Debug.Assert(axis == 0);
                                childRect.SetLeftLocalX(padding.left);
                                break;
                            case PositionMode.RIGHT_ZERO:
                                Debug.Assert(axis == 0);
                                childRect.SetRightLocalX(-padding.right);
                                break;
                            case PositionMode.CENTER:
                                childRect.SetPivotAndAnchors(new Vector2(0.5f, 0.5f));
                                childRect.localPosition = Vector3.zero;
                                break;
                            case PositionMode.SIBLING_DISTANCE:
                                var prevRect = previousChild?.Rect;

                                if (axis == 0)
                                {
                                    childRect.SetLeftLocalX(prevRect?.GetRightLocalX() ?? 0);
                                }
                                if (axis == 1)
                                {
                                    childRect.SetTopLocalY(prevRect?.GetBottomLocalY() ?? 0);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    #endregion
                    previousChild = child;
                }
            }
        }

        private static void SolveLayoutWidth(JLayoutRuntimeUnit parentLayout, RectTransform parentRect)
        {
            var widthOfContentMM = parentRect.GetWidthMilimeters() - parentLayout.LayoutData.commons.Padding.horizontal;
            var widthOfContentMMForComsumption = widthOfContentMM;
            using var _1 = ListPool<JLayoutChild>.Get(out var fillUpChildren);
            foreach (var child in parentLayout.Children)
            {
                switch (child.Commons.AxisModes[0])
                {
                    case AxisMode.PARENT_SIZE_PERCENT:
                        child.Rect.SetWidthMilimeters(widthOfContentMM);
                        widthOfContentMMForComsumption = 0;
                        break;
                    case AxisMode.SELF_SIZE:
                        child.Rect.SetWidthMilimeters(child.Commons.Size[0]);
                        widthOfContentMMForComsumption -= child.Commons.Size[0];
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
                fillUpChildren[0].Rect.SetWidthMilimeters(widthOfContentMMForComsumption);
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

            public JLayoutRuntimeUnit(RectTransform childRT2)
            {
                RectTransform = childRT2;
            }

            public RectTransform ContentTransformOverride { get; internal set; }
            public LayoutData LayoutData { get; internal set; }
            public RectTransform ContentTransform => ContentTransformOverride ?? RectTransform;

            public PositionMode[] DefaultPositionModes { get; internal set; }

            internal void AddChild(JLayoutChild child)
            {
                Children.Add(child);
                child.Rect.SetParent(RectTransform);
            }

            internal void AddLayoutAsChild(JLayoutRuntimeUnit layoutRU)
            {
                var commons = layoutRU.LayoutData.commons;
                AddLayoutAsChild(layoutRU, commons);
            }

            // Some day you might have to fuse the buttonLayout commons with childData commons
            internal void AddLayoutAsChild(JLayoutRuntimeUnit buttonLayout, LayoutChildData childData) => AddLayoutAsChild(buttonLayout, childData.Commons);

            internal void BindText(JLayoutChild textChild)
            {
                TextChildren.Add(textChild);
            }

            internal void SetText(int v, string textKey)
            {
                // localize this?
                TextChildren[v].UiUnit.rawText = textKey;
            }

            private void AddLayoutAsChild(JLayoutRuntimeUnit layoutRU, LayoutCommons commons)
            {
                JLayoutChild item = new JLayoutChild()
                {
                    LayoutRU = layoutRU,
                    Commons = commons
                };
                Children.Add(item);
                layoutRU.RectTransform.SetParent(ContentTransform);
            }
        }

        public class JLayoutChild
        {
            public LayoutChildData LayoutChild;

            public JLayoutRuntimeUnit LayoutRU { get; internal set; }
            public LayoutCommons Commons { get; internal set; }
            public UIUnit UiUnit;
            public RectTransform Rect => LayoutRU?.RectTransform ?? UiUnit?.RectTransform;
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
