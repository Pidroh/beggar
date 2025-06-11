using UnityEngine;
using HeartUnity.View;
using System;

namespace JLayout
{
    public class JLayoutChild
    {
        public LayoutChildData LayoutChildData;

        public JLayoutRuntimeUnit LayoutRU { get; internal set; }
        public LayoutCommons Commons { get; internal set; }
        public UIUnit UiUnit;
        public RectTransform Rect => LayoutRU?.RectTransform ?? UiUnit?.RectTransform;
        public PositionMode[] PositionModeOverride;
        public AxisMode[] AxisModeOverride;
        public ColorSetResolved OverwriteColorSet;

        public PositionMode[] PositionModes => PositionModeOverride ?? Commons.PositionModes;
        public AxisMode[] AxisModes => AxisModeOverride ?? Commons.AxisModes;

        public TextData TextData { get; internal set; }
        public JLayoutRuntimeUnit Parent { get; internal set; }

        private float _sizeRatioAsGauge = 1f;
        public float SizeRatioAsGauge => _sizeRatioAsGauge;

        public int ColorSchemeId { get; internal set; }

        public void UpdateSizeRatioAsGauge(float ratio)
        {
            if (_sizeRatioAsGauge == ratio) return;
            _sizeRatioAsGauge = ratio;
            PropagateDirtyUp();
        }

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
            Color colorV;
            if (OverwriteColorSet != null && OverwriteColorSet.ColorDatas.TryGetValue(color, out var v))
            {
                colorV = v.Colors[ColorSchemeId];
            }
            else
            {
                ColorSet colorSet = Commons.ColorSet;
                colorSet ??= TextData?.ColorSet;
                if (colorSet == null) return;
                if (!colorSet.ColorDatas.TryGetValue(color, out var cd)) return;
                if (UiUnit == null)
                {
                    return;
                    // maybe there are some acceptable situations for this...?
                    Debug.LogError("Has color but has no ui unit, what is this situation?");
                }
                colorV = cd.data.Colors[ColorSchemeId];
            }
            if (UiUnit.Image != null)
                UiUnit.Image.color = colorV;
            if (UiUnit.text != null)
                UiUnit.text.color = colorV;
        }

        internal void PropagateDirtyUp()
        {
            if (Parent == null) return;
            Parent.PropagateDirtyUp();
        }
    }
}
