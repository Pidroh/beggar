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
            var offset = 0f;
            foreach (var mainCanvasChild in data.jLayCanvas.childrenForLayouting)
            {
                JLayoutRuntimeUnit parentLayout = mainCanvasChild;
                // temporary code
                float newSize = 320 * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
                parentLayout.RectTransform.SetWidth(newSize);
                // parentLayout.ContentTransform.SetWidth(320 * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                parentLayout.RectTransform.SetLeftXToParent(offset);
                offset += newSize;
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
            float totalChildOccupiedHeight = 0f;
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

                var accountForTotalSize = true;

                var axisM = axisModes[yAxis];
                float? height = null;
                switch (axisM)
                {
                    case AxisMode.PARENT_SIZE_PERCENT:
                    case AxisMode.PARENT_SIZE_PERCENT_RAW:
                        // var sizeRatio = 1f;
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
                                childRect.SetLocalX((padding.left - padding.right) * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                            if (axis == 1)
                                childRect.SetLocalY((padding.bottom - padding.top) * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);

                            break;
                        case PositionMode.CENTER_RAW:
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
                                    childRect.SetLeftLocalX(prevRect.GetRightLocalX() + child.Commons.PositionOffsets[0]);
                                }
                                if (axis == 1)
                                {
                                    childRect.SetTopLocalY(prevRect.GetBottomLocalY() + child.Commons.PositionOffsets[1]);
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
                float height = totalChildOccupiedHeight + padding.vertical * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize;
                height = Mathf.Max(height, parentLayout.LayoutData.commons.MinSize[1] * RectTransformExtensions.DefaultPixelSizeToPhysicalPixelSize);
                parentLayout.ContentTransform.SetHeight(height);
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
                    case AxisMode.PARENT_SIZE_PERCENT_RAW:
                        child.Rect.SetWidth(parentRect.GetWidth());
                        break;
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

    public class JImageAccessor { }

    public class JLayTextAccessor
    {
        public JLayTextAccessor(JLayoutRuntimeUnit descLayout, int v)
        {
            DescLayout = descLayout;
            V = v;
        }

        public JLayoutRuntimeUnit DescLayout { get; }
        public int V { get; }
    }

    public class JButtonAccessor
    {
        public JLayoutRuntimeUnit buttonOwner;
        public int index;

        public JButtonAccessor(JLayoutRuntimeUnit buttonOwner, int index)
        {
            this.buttonOwner = buttonOwner;
            this.index = index;
        }

        public bool ButtonClicked => buttonOwner.ButtonChildren[index].Item2.UiUnit.Clicked;
    }
    public class JLayoutRuntimeData
    {
        public JLayCanvas jLayCanvas;

        public TMP_FontAsset DefaultFont { get; internal set; }

        public KeyedSprites ImageSprites;


    }

    public class JLayoutRuntimeUnit
    {
        public RectTransform RectTransform;
        //public List<JLayoutRuntimeUnit> Sublayouts = new();
        public List<JLayoutChild> Children = new();
        public List<JLayoutChild> TextChildren = new();
        public List<(JLayoutRuntimeUnit, JLayoutChild)> ButtonChildren = new();
        public List<JLayoutChild> ImageChildren = new();
        private bool _visibleSelf;
        private bool _parentShowing = true;
        private bool _visibleResult;

        private void UpdateVisibility()
        {
            var newVisibility = _parentShowing && _visibleSelf;
            if (newVisibility == _visibleResult) return;
            _visibleResult = newVisibility;
            RectTransform.gameObject.SetActive(_visibleResult);
        }

        public JLayoutRuntimeUnit(RectTransform childRT2)
        {
            RectTransform = childRT2;
            _visibleSelf = RectTransform.gameObject.activeSelf;
            UpdateVisibility();
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

        internal JLayoutChild AddLayoutAsChild(JLayoutRuntimeUnit layoutRU, ChildAddParameters? param = null)
        {
            var commons = layoutRU.LayoutData.commons;
            return AddLayoutAsChild(layoutRU, commons, param);
        }

        // Some day you might have to fuse the buttonLayout commons with childData commons
        internal JLayoutChild AddLayoutAsChild(JLayoutRuntimeUnit buttonLayout, LayoutChildData childData) => AddLayoutAsChild(buttonLayout, childData.Commons, null);

        internal void BindButton(JLayoutRuntimeUnit buttonLayout, JLayoutChild buttonChildSelf)
        {
            ButtonChildren.Add((buttonLayout, buttonChildSelf));
        }

        internal void BindImage(JLayoutChild im) => ImageChildren.Add(im);

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

        internal void SetParentShowing(bool expanded)
        {
            _parentShowing = expanded;
            UpdateVisibility();
        }

        internal void SetVisibleSelf(bool value)
        {
            _visibleSelf = value;
            UpdateVisibility();
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
        public int[] currentStep = new int[2];

        internal void SetCurrentStep(int v, int preferredIndex)
        {
            currentStep[v] = preferredIndex;
        }

        internal bool OnMaxStep(int v)
        {
            return currentStep[v] == Commons.StepSizes[v].Count - 1;
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
