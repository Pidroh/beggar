using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
//using UnityEngine.U2D;

namespace HeartUnity.View
{
    public class PostProcessingSettingIntegration 
    {
        public static void Enforce(PostProcessingWrapper wrapper, SettingModel settingModel, MainGameConfig.View viewConfig) 
        {
            foreach (var uc in settingModel.unitControls)
            {
                switch (uc.settingData.standardSettingType)
                {

                    case SettingModel.SettingUnitData.StandardSettingType.PP_COLOR_CORRECTION:
                        wrapper.ColorCorrectionActive = uc.rtBool;
                        break;
                    case SettingModel.SettingUnitData.StandardSettingType.PP_BLOOM:
                        wrapper.bloom.active = uc.rtFloat > 0;
                        wrapper.bloom.intensity.value = viewConfig.bloomConfig.ScaleValue(uc.rtFloat);
                        break;
                    case SettingModel.SettingUnitData.StandardSettingType.PP_TONE:
                        wrapper.shadowMidtones.active = uc.rtBool;
                        break;
                    case SettingModel.SettingUnitData.StandardSettingType.PP_SCANLINE:
                        wrapper.tvEffect.active = uc.rtBool;
                        break;
                    case SettingModel.SettingUnitData.StandardSettingType.PP_VIGNETTE:
                        wrapper.vignette.active = uc.rtBool;
                        break;
                    case SettingModel.SettingUnitData.StandardSettingType.FULLSCREEN:
                    case SettingModel.SettingUnitData.StandardSettingType.EXIT_GAME:
                    case SettingModel.SettingUnitData.StandardSettingType.EXIT_MENU:
                    case SettingModel.SettingUnitData.StandardSettingType.MASTER_VOLUME:
                    case SettingModel.SettingUnitData.StandardSettingType.MUSIC_VOLUME:
                    case SettingModel.SettingUnitData.StandardSettingType.SFX_VOLUME:
                    case SettingModel.SettingUnitData.StandardSettingType.VOICE_VOLUME:
                    case SettingModel.SettingUnitData.StandardSettingType.LANGUAGE_SELECTION:
                    case SettingModel.SettingUnitData.StandardSettingType.DELETE_DATA:
                    default:
                        break;
                }
            }
        }
    }

    [Serializable]
    public struct PostProcessingScale 
    {
        public float minValue;
        public float halfValue;
        public float maxValue;

        public PostProcessingScale(float minValue, float halfValue, float maxValue)
        {
            this.minValue = minValue;
            this.halfValue = halfValue;
            this.maxValue = maxValue;
        }

        internal float ScaleValue(float rtFloat)
        {
            if (rtFloat < 0.5f)
            {
                return UnityEngine.Mathf.Lerp(minValue, halfValue, rtFloat * 2);
            }
            else {
                return UnityEngine.Mathf.Lerp(halfValue, maxValue, (rtFloat - 0.5f) * 2);
            }
        }
    }

    public class PostProcessingWrapper 
    {
        public Volume volume;
        public Bloom bloom;
        public Vignette vignette;
        public ColorAdjustments colorAdjustments;
        public ShadowsMidtonesHighlights shadowMidtones;
        public TVEffect tvEffect;

        public PostProcessingWrapper(Volume volume)
        {
            this.volume = volume;
            if (volume.profile.TryGet<Bloom>(out var bloom)) {
                this.bloom = bloom;
            }
            if (volume.profile.TryGet<Vignette>(out var vignette))
            {
                this.vignette = vignette;
            }
            if (volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
            {
                this.colorAdjustments = colorAdjustments;
            }
            if (volume.profile.TryGet<ShadowsMidtonesHighlights>(out var shadowsMidtones))
            {
                this.shadowMidtones = shadowsMidtones;
            }
            if (volume.profile.TryGet<TVEffect>(out var tvEffect))
            {
                this.tvEffect = tvEffect;
            }
        }

        public bool ColorCorrectionActive { get => colorAdjustments.active; set => colorAdjustments.active = value; }
    }
}