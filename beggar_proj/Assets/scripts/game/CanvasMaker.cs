using UnityEngine;
using UnityEngine.UI;
using HeartUnity.View;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System;
using HeartUnity;

public static class ArcaniaCommonStrings
{
    public static string AcquireLocalLocalized => Local.GetText("Acquire", "A word for learning or becoming able to do something new");
    public static string DeactivateLocalLocalized => Local.GetText("Deactivate");
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
        public Color SelectedColor;
    }

    [Serializable]
    public struct CreateObjectRequest
    {
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
    public struct ScrollStyle
    {
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
        iconRectTransform.localPosition = Vector2.zero;
        iconRectTransform.pivot = new Vector2(0.5f, 0.5f);
        iconRectTransform.FillParent();

        // Add Image component for the icon
        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.color = Color.white; // Set icon color
        iconImage.preserveAspect = true;

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
        int fontSize = 16;
        // Create a Text GameObject for the button label
        var textUiUnit = CreateTextUnit(textColor, font, fontSize);
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
        uiUnit.NormalColor = buttonRequest.MainBody.NormalColor;
        uiUnit.ClickColor = buttonRequest.MainBody.ClickColor;
        uiUnit.text = textUiUnit.text;
        var bbb = new ButtonWithProgressBar()
        {
            Button = uiUnit,
            ProgressImage = image
        };
        bbb.SetProgress(0f);
        bbb.HeightMms = 10;
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
        textUiUnit.FontSizePhysical = fontSize;
        textUiUnit.text = text;
        return textUiUnit;
    }

    public static UIUnit CreateTextUnitClickable(Color textColor, TMP_FontAsset font, int fontSize)
    {
        var textUiUnit = CreateTextUnit(textColor, font, fontSize);
        
        GameObject buttonObject = CreateButtonObject(new Color(0, 0, 0, 0));
        var uiUnit = buttonObject.AddComponent<UIUnit>();
        uiUnit.text = textUiUnit.text;
        textUiUnit.transform.SetParent(buttonObject.transform);
        textUiUnit.RectTransform.FillParent();
        return uiUnit;
    }

    public static DynamicCanvas CreateCanvas(int N, CreateCanvasRequest canvasReq, Canvas reusableCanvas)
    {
        DynamicCanvas dc = new DynamicCanvas();
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

    public static DialogView CreateDialog(CreateObjectRequest dialogBody, CreateObjectRequest buttonObjReq, CreateButtonRequest buttonReq)
    {
        var overlay = CreateSimpleImage(new Color(0, 0, 0, 0.75f));
        var bg = CreateSimpleImage(dialogBody.MainColor);
        bg.SetParent(overlay);
        var yesB = CreateButton(ReusableLocalizationKeys.CST_YES, buttonObjReq, buttonReq);
        var noB = CreateButton(ReusableLocalizationKeys.CST_NO, buttonObjReq, buttonReq);
        yesB.Button.transform.SetParent(bg.transform);
        noB.Button.transform.SetParent(bg.transform);
        var dialogText = CreateTextUnit(dialogBody.SecondaryColor, buttonObjReq.font, 18);
        dialogText.SetParent(bg);
        dialogText.rawText = "HEY TWITCH WOHOO";
        var dv = new DialogView()
        {
            dialogText = dialogText,
            buttonConfirm = yesB,
            buttonCancel = noB,
            parentTransform = bg,
            fullScreenOverlay = overlay
        };
        return dv;
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
            childGO2.name = "Layout";
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
        ttv.LayoutChild = new LayoutChild(parentRectTransform, parentGo);

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

