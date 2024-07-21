//using UnityEngine.U2D;

using System;
using System.Collections.Generic;
using UnityEngine;
using static HeartUnity.View.InputManager;

namespace HeartUnity.View
{
    [CreateAssetMenu(fileName = "InputPromptVisuals", menuName = "Custom/Input Prompt Visuals", order = 1)]
    public class InputPromptVisuals : ScriptableObject
    {
        public List<InputPromptVisualGroup> inputPromptGroups;
        [Serializable]
        public class InputPromptVisualGroup 
        {
            public InputDevice inputDevice;
            public GamepadType gamepadType;
            public List<InputPromptVisualData> datas;
        }
    }


}