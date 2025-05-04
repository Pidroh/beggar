using UnityEngine;
using HeartUnity.View;

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
        public ColorSet OverwriteColorSet;

        public PositionMode[] PositionModes => PositionModeOverride ?? Commons.PositionModes;
        public AxisMode[] AxisModes => AxisModeOverride ?? Commons.AxisModes;

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
            Color colorV;
            if (OverwriteColorSet != null && OverwriteColorSet.ColorDatas.TryGetValue(color, out var v)) 
            {
                colorV = v.data.Colors[0];
            } else 
            {
                ColorSet colorSet = Commons.ColorSet;
                colorSet ??= TextData?.ColorSet;
                if (colorSet == null) return;
                if (!colorSet.ColorDatas.TryGetValue(color, out var cd)) return;
                if (UiUnit == null)
                {
                    Debug.LogError("Has color but has no ui unit, what is this situation?");
                    return;
                }
                colorV = cd.data.Colors[0];
            }
            if (UiUnit.Image != null)
                UiUnit.Image.color = colorV;
            if (UiUnit.text != null)
                UiUnit.text.color = colorV;
        }
    }
}
