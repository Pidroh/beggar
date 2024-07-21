//using UnityEngine.U2D;
using UnityEngine;
using UnityEngine.UI;

namespace HeartUnity.View
{

    public class BarWidget : MonoBehaviour
    {
        public Image content;
        public Image parent;
        public bool horizontal;
        public bool ratioFixType;

        public float Ratio
        {
            get
            {
                if (horizontal)
                {
                    return content.rectTransform.sizeDelta.x / parent.rectTransform.sizeDelta.x;
                }
                else {
                    return content.rectTransform.sizeDelta.y / parent.rectTransform.sizeDelta.y;
                }
            }
            set {
                var parentSize = horizontal ? parent.rectTransform.sizeDelta.x : parent.rectTransform.sizeDelta.y;
                var sd = content.rectTransform.sizeDelta;
                float ratioFix = ratioFixType ? value - 1 : value;
                if (horizontal) sd.x = parentSize * ratioFix;
                else sd.y = parentSize * ratioFix;
                content.rectTransform.sizeDelta = sd;
            }
        }


        [ContextMenu("Random test 0.7")]
        public void RandomTest07() {
            Ratio = 0.7f;
        }
    }
}