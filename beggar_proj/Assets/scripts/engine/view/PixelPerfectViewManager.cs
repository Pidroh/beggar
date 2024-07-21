using UnityEngine;
//using UnityEngine.U2D;

namespace HeartUnity.View
{
    public class PixelPerfectViewManager {
        private Vector2Int baseScreenSize;

        public float Width => baseScreenSize.x;
        public float Height => baseScreenSize.y;

        public void Init(int baseWidth, int baseHeight) {
            baseScreenSize = new Vector2Int(baseWidth, baseHeight);
        }

        // requires parent to be the same size as screen
        public void AttachToLeftSide(GameObject go, float y)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.localPosition = new Vector2(-baseScreenSize.x / 2 + rt.sizeDelta.x / 2, y);

        }
    }
}