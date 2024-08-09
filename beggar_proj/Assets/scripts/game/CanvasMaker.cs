using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using HeartUnity.View;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System;

public class DynamicCanvas
{
    public List<LayoutParent> children = new List<LayoutParent>();
    public List<LayoutParent> LowerMenus = new();
    public Queue<LayoutParent> ActiveChildren = new();
    public GameObject canvasGO;

    public RectTransform RootRT { get; internal set; }



    public LayoutParent CreateLowerMenuLayout(int height)
    {
        var lc = LayoutChild.Create();
        lc.RectTransform.SetParent(canvasGO.transform);
        lc.RectTransform.SetHeight(height);
        lc.RectTransform.FillParentWidth();
        lc.RectTransform.SetBottomYToParent(0);
        LayoutParent layoutParent = CanvasMaker.CreateLayout(lc);
        LowerMenus.Add(layoutParent);
        return layoutParent;
    }

    public int CalculateNumberOfVisibleHorizontalChildren() 
    {
        return Mathf.FloorToInt(Screen.width / 320f);
    }

    public void ShowChild(int childIndex) 
    {
        ShowChild(children[childIndex]);
    }

    public void ManualUpdate()
    {
        // Show/Hide children based on Canvas width
        int maxActiveChildrenCount = CalculateNumberOfVisibleHorizontalChildren();
        while (maxActiveChildrenCount < ActiveChildren.Count) {
            ActiveChildren.Dequeue();
        }
        
        if (maxActiveChildrenCount > 0)
        {
            int activeChildrenCount = ActiveChildren.Count;
            float availableWidth = Screen.width;
            float childWidth = Mathf.Clamp(availableWidth / activeChildrenCount, 320, 640);

            // Calculate total width of active children
            float totalWidth = childWidth * activeChildrenCount;

            // Calculate centered offset
            float centeredXOffset = (availableWidth - totalWidth) / 2;

            float xOffset = centeredXOffset;
            var LowerMenuTotalHeight = 0f;
            foreach (var item in LowerMenus)
            {
                LowerMenuTotalHeight += item.SelfChild.RectTransform.GetHeight();
            }

            List<LayoutParent> layouts = children;
            foreach (var layoutP in layouts)
            {
                var child = layoutP.SelfChild.RectTransform.gameObject;
                layoutP.SelfChild.Visible = ActiveChildren.Contains(layoutP);
                if (child.activeSelf)
                {
                    RectTransform rt = child.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.sizeDelta = new Vector2(childWidth, rt.sizeDelta.y);
                        // Adjust position based on pivot
                        float pivotOffset = rt.pivot.x * childWidth;
                        rt.anchoredPosition = new Vector2(xOffset + pivotOffset, rt.anchoredPosition.y);
                        rt.SetOffsetMinByIndex(1, LowerMenuTotalHeight);
                        xOffset += childWidth;
                    }
                    layoutP.ManualUpdate();
                }
            }
            foreach (var lowerMenuP in LowerMenus)
            {
                lowerMenuP.ManualUpdate();
            }
        }
    }

    internal void ShowChild(LayoutParent layoutParent)
    {
        if (ActiveChildren.Contains(layoutParent)) return;
        ActiveChildren.Enqueue(layoutParent);
        
    }
}



public class CanvasMaker
{
    [Serializable]
    public struct CreateButtonRequest 
    {
        public ColorDefinitions MainBody;
        public ColorDefinitions Outline;
        public ColorDefinitions GaugeFill;
        public ColorDefinitions TextColor;
    }

    [Serializable]
    public struct CreateGaugeRequest
    {
        public Color MainBody;
        public Color Outline;
        public Color GaugeFill;
        public Color TextColor;
        public Vector2 InitialSize;
        public RectOffset Padding;
    }

    [Serializable]
    public struct ColorDefinitions 
    {
        public Color NormalColor;
        public Color DisabledColor;
        public Color ClickColor;
        public Color HoverColor;
    }

    [Serializable]
    public struct CreateObjectRequest {
        public Color MainColor;
        public Color SecondaryColor;
        public Sprite MainSprite;
        public Sprite secondarySprite;
        public TMPro.TMP_FontAsset font;
    }

    [Serializable]
    public struct CreateCanvasRequest
    {
        public ScrollStyle ScrollStyle;
    }

    [Serializable]
    public struct ScrollStyle {
        public Color ScrollBarBG;
        public Color ScrollHandleColor;
        public Color ScrollHandleColorFocused;
    }

    private static GameObject CreateButtonObject(Color c, Sprite sprite = null)
    {
        // Create a GameObject for the button
        GameObject buttonObject = new GameObject("Button");

        // Add RectTransform component
        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(160, 30);
        rectTransform.localPosition = new Vector2(Screen.width / 2, Screen.height / 2);

        // Add CanvasRenderer component
        buttonObject.AddComponent<CanvasRenderer>();

        // Add Button component
        var button = buttonObject.AddComponent<Button>();

        // Add Image component for button background
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = c; // Set button background color
        buttonImage.sprite = sprite;
        button.targetGraphic = buttonImage;

        return buttonObject;
    }

    public static UIUnit CreateSimpleImage(Color c) 
    {
        GameObject iconObject = new GameObject("Icon");

        // Add RectTransform component for the icon
        RectTransform iconRectTransform = iconObject.AddComponent<RectTransform>();
        iconRectTransform.sizeDelta = new Vector2(30, 30); // Adjust size as needed
        iconRectTransform.localPosition = Vector2.zero;
        iconRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        iconRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        iconRectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Add Image component for the icon
        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.color = c; // Set icon color
        iconImage.raycastTarget = false;
        return iconObject.AddComponent<UIUnit>();
    }

    public static IconButton CreateButtonWithIcon(Sprite iconSprite)
    {
        GameObject buttonObject = CreateButtonObject(new Color(0, 0, 0, 0));

        // Create a GameObject for the icon
        GameObject iconObject = new GameObject("Icon");
        iconObject.transform.SetParent(buttonObject.transform);

        // Add RectTransform component for the icon
        RectTransform iconRectTransform = iconObject.AddComponent<RectTransform>();
        iconRectTransform.sizeDelta = new Vector2(30, 30); // Adjust size as needed
        iconRectTransform.localPosition = Vector2.zero;
        iconRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        iconRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        iconRectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Add Image component for the icon
        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.color = Color.white; // Set icon color

        var uiUnit = buttonObject.AddComponent<IconButton>();
        uiUnit.icon = iconImage;
        return uiUnit;
    }



    public static ButtonWithProgressBar CreateButton(string buttonText, CreateObjectRequest request, CreateButtonRequest buttonRequest)
    {
        GameObject buttonObject = CreateButtonObject(buttonRequest.MainBody.NormalColor, request.MainSprite);
        var image = CreateSimpleImage(buttonRequest.GaugeFill.NormalColor);
        
        image.gameObject.transform.SetParent(buttonObject.transform);
        image.RectTransform.FillParent();
        // Add Text component
        Color textColor = request.SecondaryColor;
        TMP_FontAsset font = request.font;
        // Create a Text GameObject for the button label
        var textUiUnit = CreateTextUnit(textColor, font, 16);
        textUiUnit.text.text = buttonText;
        textUiUnit.gameObject.transform.SetParent(buttonObject.transform);
        {
            var textRectTransform = textUiUnit.RectTransform;
            // Set anchors to stretch the textRectTransform to cover the entire parent
            textRectTransform.anchorMin = new Vector2(0, 0);
            textRectTransform.anchorMax = new Vector2(1, 1);

            // Set the offset to zero to ensure it covers the entire area
            textRectTransform.offsetMin = Vector2.zero;
            textRectTransform.offsetMax = Vector2.zero;
        }
        var uiUnit = buttonObject.AddComponent<UIUnit>();
        uiUnit.text = textUiUnit.text;
        var bbb = new ButtonWithProgressBar()
        {
            Button = uiUnit,
            ProgressImage = image
        };
        bbb.SetProgress(0f);
        return bbb;
    }

    public static UIUnit CreateTextUnit(Color textColor, TMP_FontAsset font, int fontSize)
    {
        GameObject textObject = new GameObject("Text");
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        // Add RectTransform component for text
        RectTransform textRectTransform = textObject.GetComponent<RectTransform>();
        textRectTransform.SetWidth(40);
        
        text.alignment = TextAlignmentOptions.Center;
        text.color = textColor; // Set text color
        text.fontSize = fontSize;
        text.font = font;
        text.raycastTarget = false;
        UIUnit textUiUnit = textObject.AddComponent<UIUnit>();
        textUiUnit.text = text;
        return textUiUnit;
    }

    public static DynamicCanvas CreateCanvas(int N, CreateCanvasRequest canvasReq)
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
            dc.children.Add(CreateChild(rootGO, i, canvasReq.ScrollStyle));
        }
        dc.RootRT = rootRT;
        // Create EventSystem GameObject
        GameObject eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<InputSystemUIInputModule>();

        // Set the EventSystem as a sibling of the Canvas
        eventSystemGO.transform.SetParent(dc.canvasGO.transform, false);

        for (int i = N - 1; i >= 0; i--)
        {
            dc.ShowChild(dc.children[i]);
        }

        return dc;
    }

    static LayoutParent CreateChild(GameObject parent, int index, ScrollStyle scrollStyle)
    {
        LayoutParent lp = null;
        lp = CreateLayout();
        lp.FitSelfSizeToChildren[1] = true;
        var childRT = lp.SelfChild.RectTransform;
        var childGO = lp.SelfChild.RectTransform.gameObject;
        childRT.anchorMin = new Vector2(0, 0);
        childRT.anchorMax = new Vector2(0, 1);
        childRT.sizeDelta = new Vector2(320, 0);
        childGO.transform.SetParent(parent.transform, false);


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


        lp.ContentTransformOverridingSelfChildTransform = contentRT;

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

        // Hide Child if Canvas width is less than required
        if (Screen.width < 320 * (index + 1))
        {
            childGO.SetActive(false);
        }

        return lp;
    }

    public static LayoutParent CreateLayout(LayoutChild lc)
    {
        LayoutParent lp;
        {
            lp = new LayoutParent(lc);
        }
        return lp;
    }

    public static LayoutParent CreateLayout()
    {
        LayoutParent lp;
        {
            // Create Child GameObject
            GameObject childGO2 = new GameObject();
            RectTransform childRT2 = childGO2.AddComponent<RectTransform>();
            lp = new LayoutParent(childRT2);
        }

        return lp;
    }

    internal static TripleTextView CreateTripleTextView(CreateObjectRequest buttonObjectRequest)
    {
        var font = buttonObjectRequest.font;
        var ttv = new TripleTextView();
        GameObject parentGo = new GameObject();
        RectTransform parentRectTransform = parentGo.AddComponent<RectTransform>();
        ttv.LayoutChild = new LayoutChild()
        {
            RectTransform = parentRectTransform
        };
        
        ttv.MainText = CreateTextUnit(buttonObjectRequest.SecondaryColor, font, 16).SetTextAlignment(TextAlignmentOptions.Left).SetParent(parentRectTransform);
        ttv.SecondaryText = CreateTextUnit(buttonObjectRequest.SecondaryColor, font, 16).SetTextAlignment(TextAlignmentOptions.Left).SetParent(parentRectTransform);
        ttv.TertiaryText = CreateTextUnit(buttonObjectRequest.SecondaryColor, font, 16).SetTextAlignment(TextAlignmentOptions.Right).SetParent(parentRectTransform);
        parentRectTransform.SetHeight(ttv.MainText.RectTransform.GetHeight());
        
        return ttv;
    }


}

public static class ColorExtensions
{
    public static Color FromHex(uint hex)
    {
        // Extract RGB and alpha values from the integer
        byte r = (byte)((hex >> 24) & 0xFF); // Red
        byte g = (byte)((hex >> 16) & 0xFF); // Green
        byte b = (byte)((hex >> 8) & 0xFF);  // Blue
        byte a = (byte)(hex & 0xFF);         // Alpha

        return new Color32(r, g, b, a);
    }
}

