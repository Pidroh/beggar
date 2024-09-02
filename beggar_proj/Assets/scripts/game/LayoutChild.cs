using HeartUnity;
using HeartUnity.View;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CanvasMaker;

public class Gauge
{
    private readonly CreateGaugeRequest gaugeRequest;
    public LayoutChild layoutChild;
    public UIUnit GaugeBackground;
    public UIUnit GaugeFill;


    public Gauge(CreateGaugeRequest gaugeRequest)
    {
        GaugeBackground = CanvasMaker.CreateSimpleImage(gaugeRequest.MainBody);
        GaugeFill = CanvasMaker.CreateSimpleImage(gaugeRequest.GaugeFill);
        RectTransform bgRT = GaugeBackground.RectTransform;
        GaugeFill.SetParent(bgRT);
        GaugeFill.RectTransform.FillParent();

        layoutChild = LayoutChild.Create(bgRT);
        layoutChild.RectTransform.SetSize(gaugeRequest.InitialSize);
        this.gaugeRequest = gaugeRequest;

        bgRT.FillParent();
        RectTransformExtensions.SetOffsets(bgRT, gaugeRequest.Padding);
    }

    public void ManualUpdate()
    {
        //GaugeBackground.RectTransform.SetOffsets(gaugeRequest.);
    }

    public void SetRatio(float ratio)
    {

        GaugeFill.RectTransform.SetAnchorMaxByIndex(0, ratio);
    }
}

public class LayoutChild
{
    public RectTransform RectTransform;

    public GameObject GameObject => RectTransform.gameObject;
    internal bool Visible { get => RectTransform.gameObject.activeSelf; set => RectTransform.gameObject.SetActive(value); }

    public static LayoutChild Create(Transform transform1 = null, Transform transform2 = null)
    {
        GameObject parentGo = new GameObject();
        parentGo.name = "layout-child";
        RectTransform parentRectTransform = parentGo.AddComponent<RectTransform>();

        var lc = new LayoutChild()
        {
            RectTransform = parentRectTransform
        };
        transform1?.SetParent(parentRectTransform);
        transform2?.transform.SetParent(parentRectTransform);
        return lc;
    }

}


public struct Vector2Null
{
    public float? X;
    public float? Y;
}

public class SimpleChild<T> where T : MonoBehaviour
{
    public LayoutChild LayoutChild;
    public RectOffset RectOffset;
    public SimpleChild(T element, RectTransform elementRectTransform)
    {
        LayoutChild = LayoutChild.Create(element.transform);
        Element = element;
        ElementRectTransform = elementRectTransform;
    }

    public void ManualUpdate()
    {
        ElementRectTransform.FillParent();
        if (RectOffset != null) ElementRectTransform.SetOffsets(RectOffset);
    }

    public T Element { get; }
    public RectTransform ElementRectTransform { get; }
}

public class ButtonWithProgressBar
{
    public UIUnit Button;
    public UIUnit ProgressImage;

    internal void SetProgress(float v)
    {
        ProgressImage.RectTransform.SetAnchorMaxByIndex(0, v);
        ProgressImage.RectTransform.SetOffsetMaxByIndex(0, 0);
    }
}

public class TripleTextView
{
    public LayoutChild LayoutChild;
    public RectTransform Parent => LayoutChild.RectTransform;
    public AutoList<UIUnit> Texts = new AutoList<UIUnit>();
    public UIUnit MainText
    {
        get => Texts[0];
        set => Texts[0] = value;
    }
    public UIUnit SecondaryText
    {
        get => Texts[1];
        set => Texts[1] = value;
    }
    public UIUnit TertiaryText
    {
        get => Texts[2];
        set => Texts[2] = value;
    }

    public void ManualUpdate()
    {
        if (!tertiaryWidthCalculated)
        {
            var tmp = TertiaryText.text;
            if (tmp != null)
            {
                // Save the current text
                string originalText = tmp.text;

                // Set the text to the target string
                tmp.text = "(0000/0000)";

                // Force update the canvas to ensure size calculation
                Canvas.ForceUpdateCanvases();

                // Calculate the preferred width
                float preferredWidth = tmp.preferredWidth;

                // Restore the original text
                tmp.text = originalText;

                // Update the width of the TertiaryText RectTransform
                RectTransform rt = TertiaryText.GetComponent<RectTransform>();
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);

                tertiaryWidthCalculated = true;
            }
        }

        // Get RectTransforms
        RectTransform rtMain = MainText.RectTransform;
        RectTransform rtSecondary = SecondaryText.RectTransform;
        RectTransform rtTertiary = TertiaryText.RectTransform;

        rtMain.SetWidth(Parent.GetWidth());

        // Align MainText with the left side of Parent
        rtMain.anchorMin = new Vector2(0, rtMain.anchorMin.y);
        rtMain.anchorMax = new Vector2(0, rtMain.anchorMax.y);
        rtMain.pivot = new Vector2(0, rtMain.pivot.y);
        rtMain.anchoredPosition = new Vector2(rtMain.sizeDelta.x * rtMain.pivot.x, rtMain.anchoredPosition.y);

        // Align TertiaryText with the right side of Parent
        rtTertiary.anchorMin = new Vector2(1, rtTertiary.anchorMin.y);
        rtTertiary.anchorMax = new Vector2(1, rtTertiary.anchorMax.y);
        rtTertiary.pivot = new Vector2(1, rtTertiary.pivot.y);
        rtTertiary.anchoredPosition = new Vector2(0, rtTertiary.anchoredPosition.y);

        // Align SecondaryText with the left side of TertiaryText
        rtSecondary.anchorMin = new Vector2(1, rtSecondary.anchorMin.y);
        rtSecondary.anchorMax = new Vector2(1, rtSecondary.anchorMax.y);
        rtSecondary.pivot = new Vector2(1, rtSecondary.pivot.y);
        rtSecondary.anchoredPosition = new Vector2(-rtTertiary.sizeDelta.x * rtTertiary.pivot.x, rtSecondary.anchoredPosition.y);

    }

    private bool tertiaryWidthCalculated = false;
}

public class SeparatorWithLabel
{
    public UIUnit Image;
    public UIUnit Text;
    public LayoutChild LayoutChild;

    public SeparatorWithLabel(UIUnit text, UIUnit image)
    {
        Text = text;
        Image = image;

        GameObject parentGo = new GameObject();
        RectTransform parentRectTransform = parentGo.AddComponent<RectTransform>();

        LayoutChild = new LayoutChild()
        {
            RectTransform = parentRectTransform
        };
        Text.transform.SetParent(parentRectTransform);
        Image.transform.SetParent(parentRectTransform);
        Text.RectTransform.SetHeight(14);
        Text.text.fontSize = 14;
        Text.text.fontStyle = TMPro.FontStyles.Italic;
        Text.text.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
    }

    public void ManualUpdate()
    {
        Image.RectTransform.SetWidthMilimeters(LayoutChild.RectTransform.GetWidthMilimeters());
        Image.RectTransform.SetHeight(1);
        LayoutChild.RectTransform.SetHeight(22);
        Image.RectTransform.SetTopYToParent(3);
        Image.RectTransform.SetLeftXToParent(0);
        Text.RectTransform.SetBottomYToParent(3);
        Text.RectTransform.SetLeftXToParent(15);
    }

}

public class LabelWithExpandable
{
    
    public ExpandableManager ExpandManager;
    public UIUnit MainText;

    public LayoutChild LayoutChild { get; }

    public IconButton ExpandButton => ExpandManager.ExpandButton;

    public bool Expanded => ExpandManager.Expanded;

    public List<GameObject> ExpandTargets = new();

    public LabelWithExpandable(IconButton expand, UIUnit mainText)
    {
        ExpandManager = new(expand);
        MainText = mainText;
        this.LayoutChild = LayoutChild.Create(expand.transform, mainText.transform);
        expand.transform.localPosition = Vector3.zero;
        mainText.transform.localPosition = Vector3.zero;
    }

    public void ManualUpdate() {
        ExpandManager.ManualUpdate();
        var heightMM = 10; // Fixed height for both buttons

        // Set height for both buttons
        MainText.RectTransform.SetHeightMilimeters(heightMM);
        ExpandButton.RectTransform.SetHeightMilimeters(heightMM);
        ExpandButton.RectTransform.SetWidthMilimeters(heightMM * 1.5f);


        var rectTransformParent = LayoutChild.RectTransform;
        rectTransformParent.SetHeightMilimeters(heightMM);
        MainText.RectTransform.SetWidthMilimeters(rectTransformParent.GetWidthMilimeters() - ExpandButton.RectTransform.GetWidthMilimeters());

        var expandButtonWidth = ExpandButton.RectTransform.rect.width;
        var expandButtonHeight = ExpandButton.RectTransform.rect.height;

        ExpandButton.RectTransform.anchoredPosition = new Vector2(
            rectTransformParent.rect.width * 0.5f - expandButtonWidth * (1 - ExpandButton.RectTransform.pivot.x),
            expandButtonHeight * (0.5f - ExpandButton.RectTransform.pivot.y)
        );

        /**
         * **/

        // Adjust the width of MainButton to occupy remaining space

        MainText.RectTransform.SetLeftXToParent(0);
        MainText.RectTransform.SetBottomYToParent(0);

    }
    
}

public class ExpandableManager
{
    public IconButton ExpandButton;
    private bool _expanded = false;
    public List<GameObject> ExpandTargets = new();

    public bool Expanded { get => _expanded && ExpandButton.Active; set => _expanded = value; }

    public ExpandableManager(IconButton expandButton)
    {
        ExpandButton = expandButton;
    }

    public void ManualUpdate()
    {
        ExpandButton.Active = ExpandTargets.Count > 0;
        if (!ExpandButton.ActiveSelf) _expanded = true;
        ExpandButton.icon.transform.localEulerAngles = new Vector3(0, 0, Expanded ? 180 : -90);
        if (ExpandButton.Clicked)
        {
            _expanded = !_expanded;
        }
        foreach (var item in ExpandTargets)
        {
            item.SetActive(Expanded);
        }
    }
}

public class ButtonWithExpandable
{
    public UIUnit MainButton;
    public ButtonWithProgressBar ButtonProgressBar;
    private Color _originalColorProgress;
    private readonly Color _disabledColorProgress;

    public IconButton ExpandButton => ExpandManager.ExpandButton;
    public LayoutChild LayoutChild;
    public List<GameObject> ExpandTargets => ExpandManager.ExpandTargets;
    
    public ExpandableManager ExpandManager;

    public bool Expanded => ExpandManager.Expanded;

    public bool MainButtonEnabled { get => MainButton.enabled; internal set => SetMainButtonEnabled(value); }

    private void SetMainButtonEnabled(bool value)
    {
        MainButton.enabled = value;
        ButtonProgressBar.ProgressImage.Image.color = value ? _originalColorProgress : _disabledColorProgress;
    }

    public static implicit operator LayoutChild(ButtonWithExpandable a) => a.LayoutChild;

    public ButtonWithExpandable(ButtonWithProgressBar button, IconButton iconButton)
    {
        ExpandManager = new(iconButton);
        MainButton = button.Button;
        ButtonProgressBar = button;
        _originalColorProgress = ButtonProgressBar.ProgressImage.Image.color;
        _disabledColorProgress = new Color(_originalColorProgress.r * 0.7f, _originalColorProgress.g * 0.7f, _originalColorProgress.b * 0.7f, _originalColorProgress.a);

        this.LayoutChild = LayoutChild.Create(MainButton.transform, iconButton.transform);
        MainButton.transform.localPosition = Vector3.zero;
        iconButton.transform.localPosition = Vector3.zero;

    }

    public void ManualUpdate()
    {

        ExpandManager.ManualUpdate();

        var heightMM = 10; // Fixed height for both buttons

        // Set height for both buttons
        MainButton.RectTransform.SetHeightMilimeters(heightMM);
        ExpandButton.RectTransform.SetHeightMilimeters(heightMM);
        ExpandButton.RectTransform.SetWidthMilimeters(heightMM * 1.5f);


        var rectTransformParent = LayoutChild.RectTransform;
        rectTransformParent.SetHeightMilimeters(heightMM);
        MainButton.RectTransform.SetWidthMilimeters(rectTransformParent.GetWidthMilimeters() - ExpandButton.RectTransform.GetWidthMilimeters());

        // Set the ExpandButton position on the right side
        var expandButtonWidth = ExpandButton.RectTransform.rect.width;
        var expandButtonHeight = ExpandButton.RectTransform.rect.height;
        /*  ExpandButton.rectTransform.anchoredPosition = new Vector2(
              rectTransformParent.GetWidth() * 0.5f - expandButtonWidth * (0.5f - ExpandButton.rectTransform.pivot.x),
              expandButtonHeight * (0.5f - ExpandButton.rectTransform.pivot.y)
          );*/

        ExpandButton.RectTransform.anchoredPosition = new Vector2(
            rectTransformParent.rect.width * 0.5f - expandButtonWidth * (1 - ExpandButton.RectTransform.pivot.x),
            expandButtonHeight * (0.5f - ExpandButton.RectTransform.pivot.y)
        );

        /**
         * **/

        // Adjust the width of MainButton to occupy remaining space

        // Calculate the correct position for MainButton
        var mainButtonWidth = MainButton.RectTransform.rect.width;
        var mainButtonHeight = MainButton.RectTransform.rect.height;

        // Position the MainButton so its left edge aligns with the parent's left edge
        MainButton.RectTransform.anchoredPosition = new Vector2(
            -rectTransformParent.rect.width * 0.5f + mainButtonWidth * (MainButton.RectTransform.pivot.x),
            mainButtonHeight * (0.5f - MainButton.RectTransform.pivot.y)
        );
    }

    internal void SetActive(bool visible)
    {
        this.LayoutChild.Visible = visible;
    }
}


