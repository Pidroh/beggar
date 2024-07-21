//using UnityEngine.U2D;

using Michsky.MUIP;
using TMPro;
using UnityEngine;

namespace HeartUnity.View
{
    public class SliderHolder : MonoBehaviour
    {

        //public UIManagerSlider sliderManager;
        public SliderManager sliderManager;
        public UIUnit label;
        public GameObject selectedImage;

        public float value
        {
            get => sliderManager.mainSlider.value; set
            {
                if (value != sliderManager.mainSlider.value)
                {
                    sliderManager.mainSlider.value = value;
                    sliderManager.UpdateUI();
                }
                
            }
        }
    }
}