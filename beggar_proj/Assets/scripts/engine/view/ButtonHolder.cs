//using UnityEngine.U2D;

using Michsky.MUIP;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HeartUnity.View
{
    public class ButtonHolder : MonoBehaviour, IPointerDownHandler
    {
        public GameObject selectedIndicator;
        public ButtonManager buttonManager;
        public UnityEvent OnCursorDown = new UnityEvent();

        public void OnPointerDown(PointerEventData eventData)
        {
            OnCursorDown.Invoke();
        }
    }
}