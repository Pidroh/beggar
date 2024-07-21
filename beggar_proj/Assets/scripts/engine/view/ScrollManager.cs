using UnityEngine;
using UnityEngine.UI;

namespace HeartUnity.View
{
    public class ScrollManager : UIUnit {
        public ScrollRect scrollView;
        public void Add(MonoBehaviour child) {
            child.transform.SetParent(scrollView.content);
        }
        

        public void SnapTo(RectTransform target)
        {
            Canvas.ForceUpdateCanvases();
            var contentPanel = scrollView.content;

            contentPanel.anchoredPosition =
                    (Vector2)scrollView.transform.InverseTransformPoint(contentPanel.position)
                    - (Vector2)scrollView.transform.InverseTransformPoint(target.position);
        }

        public void SnapToX(RectTransform target, float clampDistance)
        {
            Canvas.ForceUpdateCanvases();
            var contentPanel = scrollView.content;
            var sd = contentPanel.sizeDelta;

            Vector2 snappedPos = (Vector2)scrollView.transform.InverseTransformPoint(contentPanel.position)
                                - (Vector2)scrollView.transform.InverseTransformPoint(target.position);
            var deltaX = snappedPos.x - contentPanel.anchoredPosition.x;
            deltaX = Mathf.Clamp(deltaX, -clampDistance, clampDistance);
            contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x + deltaX, contentPanel.anchoredPosition.y);
        }

        public void SnapToY(RectTransform target, float clampDistance)
        {
            Canvas.ForceUpdateCanvases();
            var contentPanel = scrollView.content;
            var sd = contentPanel.sizeDelta;

            Vector2 snappedPos = (Vector2)scrollView.transform.InverseTransformPoint(contentPanel.position)
                                - (Vector2)scrollView.transform.InverseTransformPoint(target.position);
            var deltaY = snappedPos.y - contentPanel.anchoredPosition.y;
            deltaY = Mathf.Clamp(deltaY, -clampDistance, clampDistance);
            contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, contentPanel.anchoredPosition.y + deltaY);
        }
    }
}