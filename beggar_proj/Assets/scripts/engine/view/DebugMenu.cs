//using UnityEngine.U2D;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HeartUnity.View
{
    public class DebugMenu : MonoBehaviour
    {
        public Canvas canvas;
        public TMP_InputField mainDebugField;

        public bool IsShowing => canvas.isActiveAndEnabled;
        bool inited = false;
        public string currentDebugMessage;

        internal void Show(bool v)
        {
            canvas.gameObject.SetActive(v);
            if (v) {
                mainDebugField.Select();
                mainDebugField.ActivateInputField();
            }
            if (!inited)
            {
                inited = true;
                mainDebugField.onSubmit.AddListener((text) =>
                {
                    currentDebugMessage = text;
                });
            }
        }
    }
}