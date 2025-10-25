using System;
using System.Collections.Generic;
using UnityEngine;
using static JLayout.JLayoutRuntimeData;
using HeartUnity.View;
using TMPro;
using UnityEngine.UI;

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
            imageOwner.ImageChildren[index].UpdateSizeRatioAsGauge(xPRatio);
        }

        internal void OverwriteColor(ColorSetType type, ColorData color)
        {
            JLayoutChild jLayoutChild = imageOwner.ImageChildren[index];
            jLayoutChild.OverwriteColorSet ??= new();
            jLayoutChild.OverwriteColorSet.ColorDatas[type] = color;
        }

        internal void ReleaseOverwriteColor(ColorSetType color)
        {
            JLayoutChild jLayoutChild = imageOwner.ImageChildren[index];
            if (jLayoutChild.OverwriteColorSet == null) return;
            jLayoutChild.OverwriteColorSet.ColorDatas.Remove(color);
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

        internal void ConsumeClick()
        {
            buttonOwner.ButtonChildren[index].Item2.UiUnit.ConsumeClick();
        }

        internal void SetVisible(bool v)
        {
            buttonOwner.ButtonChildren[index].Item1.SetVisibleSelf(v);
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
        public int CurrentColorSchemeId { get; internal set; }
        public KeyedSprites ImageSprites;


    }

    public class JLayoutRuntimeUnit
    {
        public int ColorScheme { get; }

        public RectTransform RectTransform;
        //public List<JLayoutRuntimeUnit> Sublayouts = new();
        public List<JLayoutChild> Children = new();
        public List<JLayoutChild> TextChildren = new();
        public List<(JLayoutRuntimeUnit, JLayoutChild)> ButtonChildren = new();
        public List<JLayoutChild> ImageChildren = new();
        public List<JLayoutChild> LayoutChildren = new();
        private bool _visibleSelf;
        private bool _parentShowing = true;
        private bool _visibleResult;
        public int[] Dirty = new int[2];

        private void UpdateVisibility()
        {
            var newVisibility = _parentShowing && _visibleSelf;
            if (newVisibility == _visibleResult) return;
            _visibleResult = newVisibility;
            RectTransform.gameObject.SetActive(_visibleResult);
            if (_visibleResult)
            {
                MarkDirtyWithChildren();
            }
            // let the parent know whenever visibility changes
            if (ChildSelf != null)
            {
                ChildSelf.PropagateDirtyUp();
            }
        }

        internal float GetSize(int axis) => RectTransform.GetSize()[axis];

        public void MarkDirtyWithChildren()
        {
            if (ChildSelf != null)
            {
                ChildSelf.PropagateDirtyUp();
            }
            // update dirty of all axis, with extra dirty on the height because of the algorithm needing two steps
            for (int i = 0; i < Dirty.Length; i++)
            {
                Dirty[i]++;
            }
            Dirty[1]++;
            foreach (var child in Children)
            {
                child.LayoutRU?.MarkDirtyWithChildren();
            }
        }

        public JLayoutRuntimeUnit(RectTransform childRT2, int currentColorSchemeId)
        {
            ColorScheme = currentColorSchemeId;
            RectTransform = childRT2;
            _visibleSelf = RectTransform.gameObject.activeSelf;
            UpdateVisibility();
            for (int i = 0; i < Dirty.Length; i++)
            {
                Dirty[i] = 3;
            }
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

        public Image ScrollViewportImage { get; internal set; }

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
            child.Parent = this;
        }

        internal JLayoutChild AddLayoutAsChild(JLayoutRuntimeUnit layoutRU, ChildAddParameters? param = null)
        {
            var commons = layoutRU.LayoutData.commons;
            return AddLayoutAsChild(layoutRU, commons, param);
        }

        // Some day you might have to fuse the buttonLayout commons with childData commons
        internal JLayoutChild AddLayoutAsChild(JLayoutRuntimeUnit buttonLayout, LayoutChildData childData) => AddLayoutAsChild(buttonLayout, childData.Commons.UseLayoutCommons ? buttonLayout.LayoutData.commons : childData.Commons, null);

        internal void BindButton(JLayoutRuntimeUnit buttonLayout, JLayoutChild buttonChildSelf)
        {
            ButtonChildren.Add((buttonLayout, buttonChildSelf));
        }

        internal void BindImage(JLayoutChild im) => ImageChildren.Add(im);

        internal void BindLayout(JLayoutChild l) => LayoutChildren.Add(l);

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
                Commons = commons,
                ColorSchemeId = layoutRU.ColorScheme
            };
            if (differingCommons && item.Commons.AxisModes == null)
            {
                item.AxisModeOverride = layoutRU.LayoutData?.commons?.AxisModes;
            }
            if (differingCommons && item.Commons.PositionOffsets == null)
            {
                item.PositionModeOverride = layoutRU.LayoutData?.commons?.PositionModes;
            }
            layoutRU.ChildSelf = item;
            Children.Add(item);
            item.Parent = this;
            layoutRU.RectTransform.SetParent(ContentTransform);
            if (Dirty[0] <= 0)
            {
                MarkDirtyWithChildren();
            }
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

        internal void SetVisibleSelfGameObjectActive(bool visibleSelf) => SetVisibleSelf(visibleSelf);
        

        internal bool IsButtonClicked(int v)
        {
            return ButtonChildren[v].Item2.UiUnit.Clicked;
        }

        internal bool TryConsumeWidthDirty()
        {
            int axis = 0;
            return TryConsumeDirtyIndex(axis);
        }

        internal bool TryConsumeHeightDirty()
        {
            int axis = 1;
            return TryConsumeDirtyIndex(axis);
        }

        private bool TryConsumeDirtyIndex(int index)
        {
            if (Dirty[index] > 0)
            {
                Dirty[index]--;
                return true;
            }
            return false;
        }

        internal void PropagateDirtyUp()
        {
            for (int i = 0; i < Dirty.Length; i++)
            {
                Dirty[i]++;
            }
            if (ChildSelf == null) return;
            ChildSelf.PropagateDirtyUp();
        }
    }

    public class JLayCanvasChild
    {
        public JLayoutRuntimeUnit LayoutRuntimeUnit;
        private Vector2? _savedPivot;

        public JLayCanvasChild(JLayoutRuntimeUnit layoutRuntimeUnit)
        {
            LayoutRuntimeUnit = layoutRuntimeUnit;
        }

        public float DesiredSize { get; private set; } = 320;
        public void UpdateDesiredSize(float width)
        {
            if (DesiredSize == width) return;
            DesiredSize = width;
            LayoutRuntimeUnit.MarkDirtyWithChildren();
        }

        internal void SavePivot()
        {
            _savedPivot = LayoutRuntimeUnit.RectTransform.pivot;
        }

        public void ApplySavedPivot() 
        {
            LayoutRuntimeUnit.RectTransform.pivot = _savedPivot.Value;
        }

        public bool Mandatory { get; internal set; }
        public float PreviousWidth { get; internal set; }
        public bool ForceCenterX { get; internal set; }
    }
}
