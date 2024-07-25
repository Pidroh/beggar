using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DynamicCanvas
{
    public List<GameObject> children = new List<GameObject>();
    public GameObject canvasGO;

    public void ManualUpdate()
    {
        // Show/Hide children based on Canvas width
        int activeChildrenCount = 0;
        foreach (var child in children)
        {
            if (Screen.width >= 320 * (activeChildrenCount + 1))
            {
                child.SetActive(true);
                activeChildrenCount++;
            }
            else
            {
                child.SetActive(false);
            }
        }

        if (activeChildrenCount > 0)
        {
            float availableWidth = Screen.width;
            float childWidth = Mathf.Clamp(availableWidth / activeChildrenCount, 320, 640);

            float xOffset = 0;
            foreach (var child in children)
            {
                if (child.activeSelf)
                {
                    RectTransform rt = child.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.sizeDelta = new Vector2(childWidth, rt.sizeDelta.y);
                        rt.anchoredPosition = new Vector2(xOffset, rt.anchoredPosition.y);
                        xOffset += childWidth;
                    }
                }
            }
        }
    }
}


public class CanvasMaker {
    

    public static DynamicCanvas CreateCanvas(int N)
    {
        DynamicCanvas dc = new DynamicCanvas();
        // Create Canvas GameObject
        dc.canvasGO = new GameObject("Canvas");
        var canvasGO = dc.canvasGO;
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Root GameObject
        GameObject rootGO = new GameObject("Root");
        rootGO.transform.SetParent(canvasGO.transform, false);

        RectTransform rootRT = rootGO.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;

        // Create N children
        for (int i = 0; i < N; i++)
        {
            dc.children.Add(CreateChild(rootGO, i));
        }

        return dc;
    }

    static GameObject CreateChild(GameObject parent, int index)
    {
        // Create Child GameObject
        GameObject childGO = new GameObject("Child" + index);
        childGO.transform.SetParent(parent.transform, false);

        RectTransform childRT = childGO.AddComponent<RectTransform>();
        childRT.anchorMin = new Vector2(0, 0);
        childRT.anchorMax = new Vector2(0, 1);
        childRT.sizeDelta = new Vector2(320, 0);
        childRT.anchoredPosition = new Vector2(320 * index, 0);

        // Add ScrollView
        ScrollRect scrollRect = childGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

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
        viewportImage.color = new Color(1, 1, 1, 0.2f);

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

        scrollRect.content = contentRT;

        // Optional: Add Content Placeholder
        Text contentText = contentGO.AddComponent<Text>();
        contentText.text = "Content " + index;
        contentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        contentText.alignment = TextAnchor.UpperLeft;
        contentText.color = Color.black;

        // Add Vertical ScrollBar
        GameObject scrollbarGO = new GameObject("ScrollbarVertical");
        scrollbarGO.transform.SetParent(childGO.transform, false);

        RectTransform scrollbarRT = scrollbarGO.AddComponent<RectTransform>();
        scrollbarRT.anchorMin = new Vector2(1, 0);
        scrollbarRT.anchorMax = new Vector2(1, 1);
        scrollbarRT.pivot = new Vector2(1, 0.5f);
        scrollbarRT.sizeDelta = new Vector2(20, 0);

        Scrollbar scrollbar = scrollbarGO.AddComponent<Scrollbar>();
        Image scrollbarImage = scrollbarGO.AddComponent<Image>();
        scrollbarImage.color = new Color(0, 0, 0, 0.5f);

        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.verticalScrollbarSpacing = -3;

        // Add Scrollbar Handle
        GameObject handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(scrollbarGO.transform, false);

        RectTransform handleRT = handleGO.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20, 0);

        Image handleImage = handleGO.AddComponent<Image>();
        handleImage.color = new Color(1, 1, 1, 0.7f);
        scrollbar.targetGraphic = handleImage;
        scrollbar.handleRect = handleRT;

        // Hide Child if Canvas width is less than required
        if (Screen.width < 320 * (index + 1))
        {
            childGO.SetActive(false);
        }

        return childGO;
    }
}