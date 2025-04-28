using System;
using System.Collections.Generic;
using UnityEngine;
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

        internal void SetGaugeRatio(float xPRatio)
        {
            imageOwner.ImageChildren[index].SizeRatioAsGauge = xPRatio;
        }
    }

    public class JLayTextAccessor
    {
        public JLayTextAccessor(JLayoutRuntimeUnit layout, int index)
        {
            OwnerLayout = layout;
            this.index = index;
        }

        public void SetTextRaw(string text)
        {
            OwnerLayout.SetTextRaw(index, text);
        }

        public JLayoutRuntimeUnit OwnerLayout { get; }
        public int index { get; }
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

        internal void MultiClickEnabled(bool v)
        {
            buttonOwner.ButtonChildren[index].Item2.UiUnit.LongPressMulticlickEnabled = v;
        }

        internal void SetButtonEnabled(bool v)
        {
            buttonOwner.ButtonChildren[index].Item2.UiUnit.ButtonEnabled = v;
        }

        internal void SetActivePowered(bool v)
        {
            buttonOwner.ButtonChildren[index].Item1.ActivePowered = v;
        }

        internal void SetButtonTextRaw(string v)
        {
            buttonOwner.ButtonChildren[index].Item1.SetTextRaw(0, v);
        }
    }
    public class JLayoutRuntimeData
    {
        public JLayCanvas jLayCanvas;

        public TMP_FontAsset DefaultFont { get; internal set; }
        public LayoutDataMaster LayoutMaster { get; internal set; }

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
        public RectTransform ContentTransform => ContentTransformOverride == null ? RectTransform : ContentTransformOverride;
        public AxisMode[] OverrideAxisMode { internal get; set; }
        public AxisMode[] AxisMode => OverrideAxisMode ?? LayoutData.commons?.AxisModes;

        public PositionMode[] DefaultPositionModes { get; internal set; }
        internal bool Visible { get => _visibleResult; }
        public UIUnit SelfUIUnit { get; internal set; }
        public bool ClickedLayout => SelfUIUnit?.Clicked ?? false;

        public JLayoutChild ChildSelf { get; private set; }
        public bool Hovered => (ChildSelf?.UiUnit) != null && ChildSelf.UiUnit.HoveredWhileVisible;

        internal bool TryGetSelfButton(out UIUnit buttonUU)
        {
            buttonUU = null;
            if (SelfUIUnit != null && SelfUIUnit.HasButton) buttonUU = SelfUIUnit;
            if (ChildSelf?.UiUnit != null && ChildSelf.UiUnit.HasButton) buttonUU = ChildSelf.UiUnit;
            return buttonUU != null;
        }

        public bool? Disabled;

        // this is different from unity game object active in the sense that it is a graphical state
        // just because active is false does not mean it is invisible
        public bool? ActivePowered;

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

        internal void SetTextRaw(int v, string textKey)
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
            layoutRU.ChildSelf = item;
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

        internal void SetVisibleSelf(bool visibleSelf)
        {
            _visibleSelf = visibleSelf;
            UpdateVisibility();
        }

        internal bool IsButtonClicked(int v)
        {
            return ButtonChildren[v].Item2.UiUnit.Clicked;
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

        public TextData TextData { get; internal set; }

        public float SizeRatioAsGauge = 1f;

        public int[] currentStep = new int[2];

        internal void SetCurrentStep(int v, int preferredIndex)
        {
            currentStep[v] = preferredIndex;
        }

        internal bool OnMaxStep(int v)
        {
            return currentStep[v] == Commons.StepSizes[v].Count - 1;
        }

        internal void ApplyColor(ColorSetType color)
        {
            ColorSet colorSet = Commons.ColorSet;
            colorSet ??= TextData?.ColorSet;
            if (colorSet == null) return;
            if (!colorSet.ColorDatas.TryGetValue(color, out var cd)) return;
            if (UiUnit == null) 
            {
                return;
                Debug.LogError("Has color but has no ui unit, what is this situation?"); 
            }
            var colorV = cd.data.Colors[0];
            if (UiUnit.Image != null)
                UiUnit.Image.color = colorV;
            if (UiUnit.text != null)
                UiUnit.text.color = colorV;
        }
    }

    public class JLayCanvasChild
    {
        public JLayoutRuntimeUnit LayoutRuntimeUnit;

        public JLayCanvasChild(JLayoutRuntimeUnit layoutRuntimeUnit)
        {
            LayoutRuntimeUnit = layoutRuntimeUnit;
        }

        public float DesiredSize { get; internal set; } = 320;
        public bool Mandatory { get; internal set; }
    }
}
