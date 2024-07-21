using Michsky.MUIP;
using TMPro;
using UnityEngine;
//using UnityEngine.U2D;

namespace HeartUnity.View
{
    public class ReusableMenuPrefabs : MonoBehaviour
    {
        public ReusableMenuInCanvas menu;
        public ButtonHolder button;
        public ToggleHolder toggle;
        public SliderHolder slider;
        public TextMeshProUGUI text;
        public UIUnit textFullScreen;
        public TextAsset defaultSettingText;
        public UIUnit textAutoFitForSettings;
    }
}