//using UnityEngine.U2D;

using Michsky.MUIP;
using TMPro;
using UnityEngine;

namespace HeartUnity.View
{
    public class ToggleHolder : MonoBehaviour
    {

        public SwitchManager switchManager;
        public UIUnit label;
        public GameObject selectedIndicator;

        public bool IsOn
        {
            get => switchManager.isOn; set
            {
                if (value != switchManager.isOn)
                {
                    if (value)
                    {
                        switchManager.SetOn();
                    }
                    else
                    {
                        switchManager.SetOff();
                    }
                    switchManager.UpdateUI();
                }

            }
        }
    }
}