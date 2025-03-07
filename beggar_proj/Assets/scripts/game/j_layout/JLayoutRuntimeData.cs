using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using static JLayout.JLayoutRuntimeData;
using HeartUnity.View;

namespace JLayout
{
    public static class JLayoutRuntimeExecuter 
    {
        public static void ManualUpdate(JLayoutRuntimeData data) 
        {
            foreach (var mainCanvasChild in data.jLayCanvas.ActiveChildren)
            {
                var width = 320;
                JLayoutChild previousChild = null;
                foreach (var child in mainCanvasChild.Children)
                {
                    RectTransform childRect = child.Rect;
                    var padding = child.Commons.Padding;
                    #region position
                    
                    for (int axis = 0; axis < 2; axis++)
                    {
                        var pm = child.Commons.PositionModes[axis];
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
                                var prevRect = previousChild.Rect;
                                var prevLocal = prevRect.localPosition;

                                if (axis == 0) 
                                {
                                    childRect.SetLeftLocalX(prevRect.GetRightLocalX());
                                }
                                if (axis == 1)
                                {
                                    childRect.SetTopLocalY(prevRect.GetBottomLocalY());
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
        
    }
    public class JLayoutRuntimeData
    {
        public JLayCanvas jLayCanvas;

        public class JLayoutRuntimeUnit
        {
            public RectTransform RectTransform;
            //public List<JLayoutRuntimeUnit> Sublayouts = new();
            public List<JLayoutChild> Children = new();

            public JLayoutRuntimeUnit(RectTransform childRT2)
            {
                RectTransform = childRT2;
            }

            public RectTransform ContentTransformOverride { get; internal set; }
            public LayoutData LayoutData { get; internal set; }
            public RectTransform ContentTransform => ContentTransformOverride ?? RectTransform;

            internal void AddLayoutAsChild(JLayoutRuntimeUnit layoutRU)
            {
                var commons = layoutRU.LayoutData.commons;
                JLayoutChild item = new JLayoutChild() {
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
            public RectTransform Rect => LayoutRU.RectTransform;
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
