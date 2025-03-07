using HeartUnity.View;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Pool;
using UnityEngine.UI;
using static JLayout.JLayoutRuntimeData;

namespace JLayout
{
    public class JLayoutRuntimeData
    {
        public class JLayoutRuntimeUnit
        {
            public RectTransform RectTransform;
            //public List<JLayoutRuntimeUnit> Sublayouts = new();
            public List<JLayoutChild> Children = new();

            public JLayoutRuntimeUnit(RectTransform childRT2)
            {
                RectTransform = childRT2;
            }

            public RectTransform ContentTransformOverride { get; internal set; }
        }

        public class JLayoutChild
        {
            public LayoutChildData LayoutChild;

        }
    }

    public class JLayCanvas
    {
        public GameObject canvasGO;
        public Canvas Canvas { get; internal set; }
        public RectTransform RootRT { get; internal set; }
        public List<JLayoutRuntimeUnit> children = new List<JLayoutRuntimeUnit>();
        public List<JLayoutRuntimeUnit> childrenForLayouting = new List<JLayoutRuntimeUnit>();
        public RectTransform OverlayRoot { get; internal set; }
        public Queue<JLayoutRuntimeUnit> ActiveChildren = new();

        internal void ShowOverlay() => OverlayRoot.gameObject.SetActive(true);

        internal void HideOverlay() => OverlayRoot.gameObject.SetActive(false);

        private void HideChild(JLayoutRuntimeUnit layoutParent)
        {
            using var _1 = ListPool<JLayoutRuntimeUnit>.Get(out var list);
            list.AddRange(ActiveChildren);
            ActiveChildren.Clear();
            foreach (var item in list)
            {
                if (item == layoutParent) continue;
                ActiveChildren.Enqueue(item);
            }
        }

        internal void ShowChild(JLayoutRuntimeUnit layoutParent)
        {
            if (ActiveChildren.Contains(layoutParent)) return;
            while (childrenForLayouting.Remove(layoutParent)) { }
            childrenForLayouting.Insert(0, layoutParent);
            ActiveChildren.Enqueue(layoutParent);
        }
    }

    public class JCanvasMaker
    {
        public static JLayCanvas CreateCanvas(int N, CanvasMaker.CreateCanvasRequest canvasReq, Canvas reusableCanvas)
        {
            JLayCanvas dc = new JLayCanvas();
            // Create Canvas GameObject
            if (reusableCanvas == null)
            {
                dc.canvasGO = new GameObject("Canvas");
                // Create EventSystem GameObject
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<InputSystemUIInputModule>();
                // Set the EventSystem as a sibling of the Canvas
                eventSystemGO.transform.SetParent(dc.canvasGO.transform, false);

                Canvas canvas = dc.canvasGO.AddComponent<Canvas>();
                dc.canvasGO.AddComponent<GraphicRaycaster>();
                dc.Canvas = canvas;
                dc.canvasGO.AddComponent<CanvasScaler>();
            }
            else
            {
                var cnvs = GameObject.Instantiate(reusableCanvas);
                dc.canvasGO = cnvs.gameObject;
                dc.Canvas = cnvs;
            }

            {
                var canvas = dc.Canvas;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                var canvasGO = dc.canvasGO;
                var scaler = canvasGO.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.referenceResolution = new Vector2(1920, 1080);


                // Create Root GameObject
                GameObject rootGO = new GameObject("Root");
                rootGO.transform.SetParent(canvasGO.transform, false);

                RectTransform rootRT = rootGO.AddComponent<RectTransform>();
                rootRT.FillParent();

                // Create N children
                for (int i = 0; i < N; i++)
                {
                    dc.children.Add(CreateChild(rootGO, i, canvasReq.ScrollStyle));
                }
                dc.childrenForLayouting.AddRange(dc.children);
                dc.RootRT = rootRT;
                dc.OverlayRoot = canvasGO.GetComponent<RectTransform>().CreateFullSizeChild("overlay_root");
                {
                    var oi = dc.OverlayRoot.CreateFullSizeChild("overlay_image");
                    oi.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.99f);
                    dc.HideOverlay();
                }
                // shows in the opposite order so that the bottoms ones are shown last
                // thus, prioritized
                for (int i = N - 1; i >= 0; i--)
                //for (int i = 0; i < N; i++)
                {
                    dc.ShowChild(dc.children[i]);
                }
            }

            return dc;
        }


        public static JLayoutRuntimeUnit CreateLayout()
        {
            JLayoutRuntimeUnit lp;
            {
                // Create Child GameObject
                GameObject childGO2 = new GameObject();
                childGO2.name = "Layout";
                RectTransform childRT2 = childGO2.AddComponent<RectTransform>();
                lp = new JLayoutRuntimeUnit(childRT2);
            }

            return lp;
        }

        static JLayoutRuntimeUnit CreateChild(GameObject parent, int index, CanvasMaker.ScrollStyle scrollStyle)
        {
            JLayoutRuntimeUnit lp = null;
            lp = CreateLayout();
            var ld = new LayoutData();
            ld.commons.AxisModes[0] = AxisMode.SELF_SIZE;
            ld.commons.AxisModes[1] = AxisMode.CONTAIN_CHILDREN;

            var childRT = lp.RectTransform;
            var childGO = lp.RectTransform.gameObject;
            childRT.anchorMin = new Vector2(0, 0);
            childRT.anchorMax = new Vector2(0, 1);
            childRT.sizeDelta = new Vector2(320, 0);
            childGO.transform.SetParent(parent.transform, false);


            childRT.anchoredPosition = new Vector2(320 * index, 0);

            // Add ScrollView
            ScrollRect scrollRect = childGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
#if !UNITY_EDITOR
        scrollRect.scrollSensitivity *= 0.1f;
#endif
#if UNITY_EDITOR
            scrollRect.scrollSensitivity *= 2f;
#endif

            // Add Viewport
            GameObject viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(childGO.transform, false);

            RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;

            CanvasRenderer viewportCR = viewportGO.AddComponent<CanvasRenderer>();
            Image viewportImage = viewportGO.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0f);

            scrollRect.viewport = viewportRT;

            // Add Content
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);

            RectTransform contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = new Vector2(0, 0);
            contentRT.sizeDelta = new Vector2(0, 0);

            const int scrollBarWidth = 10;

            contentRT.SetOffsetMaxByIndex(0, -scrollBarWidth);


            lp.ContentTransformOverride = contentRT;

            scrollRect.content = contentRT;

            // Add Vertical ScrollBar
            GameObject scrollbarGO = new GameObject("ScrollbarVertical");
            scrollbarGO.transform.SetParent(childGO.transform, false);

            RectTransform scrollbarRT = scrollbarGO.AddComponent<RectTransform>();
            scrollbarRT.anchorMin = new Vector2(1, 0);
            scrollbarRT.anchorMax = new Vector2(1, 1);
            scrollbarRT.pivot = new Vector2(1, 0.5f);

            scrollbarRT.sizeDelta = new Vector2(scrollBarWidth, 0);

            Scrollbar scrollbar = scrollbarGO.AddComponent<Scrollbar>();
            Image scrollbarImage = scrollbarGO.AddComponent<Image>();
            scrollbarImage.color = scrollStyle.ScrollBarBG;

            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarSpacing = -3;

            // Add Scrollbar Handle
            GameObject handleGO = new GameObject("Handle");
            handleGO.transform.SetParent(scrollbarGO.transform, false);

            RectTransform handleRT = handleGO.AddComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(0, 0);

            Image handleImage = handleGO.AddComponent<Image>();
            handleImage.color = scrollStyle.ScrollHandleColor;
            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRT;

            return lp;
        }
    }
}
