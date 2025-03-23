using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using static JLayout.JLayoutRuntimeData;
using HeartUnity.View;
using TMPro;

namespace JLayout
{

    public class JImageAccessor 
    {
        public JLayoutRuntimeUnit imageOwner;
        public int index;

        public JImageAccessor(JLayoutRuntimeUnit buttonOwner, int index)
        {
            this.imageOwner = buttonOwner;
            this.index = index;
        }

        public RectTransform Rect => imageOwner.ImageChildren[index].Rect;

    }

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
        internal bool Visible { get => _visibleResult; }
        public UIUnit SelfUIUnit { get; internal set; }
        public bool ClickedLayout => SelfUIUnit?.Clicked ?? false;

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
        internal Dictionary<Direction, List<JLayoutRuntimeUnit>> FixedMenus = new();
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
