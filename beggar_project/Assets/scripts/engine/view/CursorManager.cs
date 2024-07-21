using System;
using UnityEngine;

namespace HeartUnity.View
{
    public class CursorManager
    {
        public CursorView cursorView;
        private RectTransform rectTrans;
        internal float distance;

        public Vector3 TargetCursorLocalPosition { get; internal set; }

        public enum CursorPositionBehavior { 
            INVALID,
            ANY,
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        internal void SetCursor(CursorView cursorView)
        {
            this.cursorView = cursorView;
            rectTrans = cursorView.GetComponent<RectTransform>();
        }

        internal void Update()
        {
            if (cursorView != null) {
                var lp = cursorView.transform.localPosition;
                VectorUtil.MoveTo(cursorView.cursorSpeed * Time.deltaTime, ref lp, TargetCursorLocalPosition);
                if (float.IsNaN(lp.x)) return;
                cursorView.transform.localPosition = lp;
                var totalTime = cursorView.graphicModes.Count * cursorView.timeBetweenFrames;
                var timeProgress = Time.time % totalTime;
                var currentGM = Mathf.FloorToInt(timeProgress / cursorView.timeBetweenFrames);
                var gm = cursorView.graphicModes[currentGM];
                cursorView.mainChild.ChangeSprite(gm.spriteChange);
                cursorView.mainChild.transform.localPosition = gm.offsetLocalPos;
            }
        }

        internal Vector2 GetCursorSizeDelta()
        {
            return rectTrans.sizeDelta;
        }

        internal void SetCurrentBehavior(CursorPositionBehavior cursorBehavior)
        {
            foreach (var graphic in cursorView.cursorConfigs)
            {
                if (graphic.behavior == cursorBehavior) {
                    cursorView.transform.eulerAngles = new Vector3(0, 0, graphic.rotation);
                    cursorView.transform.localScale = graphic.scale;
                    return;
                }
            }
        }

        internal Vector3 PositionToLocalPosition(Vector3 position)
        {
            var initPos = cursorView.transform.position;
            cursorView.transform.position = position;
            var lp = cursorView.transform.localPosition;
            cursorView.transform.position = initPos;
            return lp;
        }
    }
}