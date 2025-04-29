using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using HeartUnity.View;
using UnityEngine.UI;

namespace JLayout
{
    public class JLayCanvas
    {
        public GameObject canvasGO;
        public Canvas Canvas { get; internal set; }
        public RectTransform RootRT { get; internal set; }
        public List<JLayCanvasChild> children = new List<JLayCanvasChild>();
        public List<JLayCanvasChild> childrenForLayouting = new List<JLayCanvasChild>();
        internal Dictionary<Direction, List<JLayCanvasChild>> FixedMenus = new();
        public RectTransform OverlayRoot { get; internal set; }
        public List<JLayCanvasChild> ActiveChildren = new();
        public List<JLayCanvasChild> Overlays = new();

        private const int minimumDefaultTabPixelWidth = 320;

        // Pixel size adjusted from fall back DPI to actual DPI
        public float DefaultPixelSizeToPhysicalPixelSize => RectTransformExtensions.PixelToMilimiterFallback * RectTransformExtensions.MilimeterToPixel;

        public Image overlayImage { get; internal set; }

        internal void ShowOverlay() => OverlayRoot.gameObject.SetActive(true);

        internal void HideOverlay() => OverlayRoot.gameObject.SetActive(false);

        private void HideChild(JLayCanvasChild layoutParent)
        {
            using var _1 = ListPool<JLayCanvasChild>.Get(out var list);
            list.AddRange(ActiveChildren);
            ActiveChildren.Clear();
            foreach (var item in list)
            {
                if (item == layoutParent) continue;
                ActiveChildren.Insert(0, item);
            }
        }

        public void ToggleChild(int childIndex)
        {
            ToggleChild(children[childIndex]);
        }

        internal void ToggleChild(JLayCanvasChild layoutParent)
        {
            if (ActiveChildren.Contains(layoutParent))
            {
                HideChild(layoutParent);
            }
            else
            {
                ShowChild(layoutParent);
            }
        }

        internal void ShowChild(JLayCanvasChild layoutParent)
        {
            if (ActiveChildren.Contains(layoutParent)) return;
            while (childrenForLayouting.Remove(layoutParent)) { }
            childrenForLayouting.Insert(0, layoutParent);
            ActiveChildren.Insert(0, layoutParent);
        }

        public void ShowChild(int childIndex)
        {
            ShowChild(children[childIndex]);
        }

        internal void EnableChild(int tabIndex, bool enabled)
        {
            // you might need to do something else when enabling a tab, but for now nothing to do
            // only thing you gotta do for now is to hide if disabled
            if (enabled) return;
            if (ActiveChildren.Contains(children[tabIndex]))
            {
                HideChild(children[tabIndex]);
            }
        }

        internal bool CanShowOnlyOneChild()
        {
            return CalculateNumberOfVisibleHorizontalChildren() <= 1;
        }

        public int CalculateNumberOfVisibleHorizontalChildren()
        {
            var physicalTabPixelSize = GetAdjustedMinimumTabPixelWidth();
            return Mathf.Max(Mathf.FloorToInt(Screen.width / physicalTabPixelSize), 1);
        }

        private float GetAdjustedMinimumTabPixelWidth()
        {
            return Mathf.Max(minimumDefaultTabPixelWidth * DefaultPixelSizeToPhysicalPixelSize, minimumDefaultTabPixelWidth);
        }

        internal bool IsChildVisible(int tabIndex)
        {
            return ActiveChildren.Contains(children[tabIndex]);
        }

        internal void SetChildSize(int tabIndex, float size)
        {
            children[tabIndex].DesiredSize = size;
        }
    }
}
