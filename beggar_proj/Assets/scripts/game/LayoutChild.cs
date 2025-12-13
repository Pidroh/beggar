using HeartEngineCore;
using HeartUnity;
using HeartUnity.View;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CanvasMaker;

[Obsolete]
public class Gauge
{
    private readonly CreateGaugeRequest gaugeRequest;
    private readonly float heightMM;
    public LayoutChild layoutChild;
    public UIUnit GaugeBackground;
    public UIUnit GaugeFill;

    public Gauge(CreateGaugeRequest gaugeRequest, float heightMM)
    {
        GaugeBackground = CanvasMaker.CreateSimpleImage(gaugeRequest.MainBody);
        GaugeFill = CanvasMaker.CreateSimpleImage(gaugeRequest.GaugeFill);
        RectTransform bgRT = GaugeBackground.RectTransform;
        GaugeFill.SetParent(bgRT);
        GaugeFill.RectTransform.FillParent();

        layoutChild = LayoutChild.Create(bgRT);
        layoutChild.RectTransform.SetSize(gaugeRequest.InitialSize);
        this.gaugeRequest = gaugeRequest;
        this.heightMM = heightMM;
        bgRT.FillParent();
        RectTransformExtensions.SetOffsets(bgRT, gaugeRequest.Padding);
    }

    internal void SetRatio(object explorationRatio)
    {
        throw new NotImplementedException();
    }

    public void ManualUpdate()
    {
        layoutChild.RectTransform.SetHeightMilimeters(heightMM);
    }

    public void SetRatio(float ratio)
    {

        GaugeFill.RectTransform.SetAnchorMaxByIndex(0, ratio);
    }
}

[Obsolete]
public class LayoutChild
{
    public RectTransform RectTransform;
    private GameObject _gameObject;
    internal bool Visible { get => _visibleResult; }

    public float?[] PreferredSizeMM = new float?[2];
    public List<TextDrivenPreferredHeightUnit> TextDrivenHeight = new();
    private bool _parentShowing = true;
    private bool _visibleResult = true;
    private bool _visibleSelf = true;
    public bool VisibleSelf  { get => _visibleSelf; set => SetVisibleSelf(value); }
    public string ObjectName { set { _gameObject.name = value; } }

    public LayoutChild(RectTransform rectTransform, GameObject gameObject)
    {
        RectTransform = rectTransform;
        _gameObject = gameObject;
        _visibleResult = gameObject.activeSelf;
    }

    public static LayoutChild Create(Transform transform1 = null, Transform transform2 = null)
    {
        GameObject parentGo = new GameObject();
        parentGo.name = "layout-child";
        RectTransform parentRectTransform = parentGo.AddComponent<RectTransform>();

        var lc = new LayoutChild(parentRectTransform, parentGo);
        transform1?.SetParent(parentRectTransform);
        transform2?.transform.SetParent(parentRectTransform);
        return lc;
    }

    internal void SetPreferredHeightMM(int v)
    {
        PreferredSizeMM[1] = v;
    }

    public class TextDrivenPreferredHeightUnit
    {
        public UIUnit text;
        public float AdditionalHeight;
    }

    internal void AddTextDrivenHeight(UIUnit text, float v)
    {
        TextDrivenHeight.Add(new TextDrivenPreferredHeightUnit() 
        { 
            AdditionalHeight = v,
            text = text
        });
    }

    internal void SetParentShowing(bool expanded)
    {
        _parentShowing = expanded;
        UpdateVisibility();
    }

    internal void SetVisibleSelf(bool value)
    {
        _visibleSelf = value;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        var newVisibility = _parentShowing && _visibleSelf;
        if (newVisibility == _visibleResult) return;
        _visibleResult = newVisibility;
        _gameObject.SetActive(_visibleResult);
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
    public RectOffset RectOffsetRequest;
    public SimpleChild(T element, RectTransform elementRectTransform)
    {
        LayoutChild = LayoutChild.Create(element.transform);
        Element = element;
        ElementRectTransform = elementRectTransform;

        ElementRectTransform.FillParent();
    }

    public void ManualUpdate()
    {
        if (RectOffsetRequest != null)
        {
            ElementRectTransform.FillParent();
            ElementRectTransform.SetOffsets(RectOffsetRequest); 
        }
        RectOffsetRequest = null;
    }

    public T Element { get; }
    public RectTransform ElementRectTransform { get; }
    public bool Visible { get => LayoutChild.Visible; set { LayoutChild.SetVisibleSelf(value); } }
}

public class ButtonWithProgressBar
{
    public UIUnit Button;
    public UIUnit ProgressImage;

    public int HeightMms { get; internal set; }
    // do not use visible in set, instead deactivate with Layout Child
    public bool Visible { get => Button.Active;  }

    internal void SetProgress(float v)
    {
        ProgressImage.RectTransform.SetAnchorMaxByIndex(0, v);
        ProgressImage.RectTransform.SetOffsetMaxByIndex(0, 0);
    }

    internal void ManualUpdate()
    {
        Button.RectTransform.SetHeightMilimeters(HeightMms);
    }

    internal void SetWidthMM(int mmWidth)
    {
        Button.RectTransform.SetWidthMilimeters(mmWidth);
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
    public bool Visible { get => LayoutChild.Visible; internal set => LayoutChild.SetVisibleSelf(value); }

    public TTVMode Mode = TripleTextView.TTVMode.All3;

    public enum TTVMode
    {
        All3,
        PrimarySecondary,

    }

    public void ManualUpdate()
    {

        foreach (var t in Texts)
        {
            t.text.SetFontSizePhysical(15);
        }

        // Get RectTransforms
        RectTransform rtMain = MainText.RectTransform;
        RectTransform rtSecondary = SecondaryText.RectTransform;
        RectTransform rtTertiary = TertiaryText.RectTransform;

        if (Mode == TTVMode.All3)
        {
            rtMain.SetWidth(Parent.GetWidth() * 0.33f);
            rtSecondary.SetWidth(Parent.GetWidth() * 0.33f);
            rtTertiary.SetWidth(Parent.GetWidth() * 0.33f);
        }
        // LayoutChild.RectTransform.SetHeightMilimeters(12);
        var height = MainText.text.preferredHeight;
        LayoutChild.RectTransform.SetHeight(height + 3 * RectTransformExtensions.MilimeterToPixel);


        if (Mode == TTVMode.PrimarySecondary)
        {
            rtMain.SetWidth(Parent.GetWidth() * 0.5f);
            rtSecondary.SetWidth(Parent.GetWidth() * 0.5f);
            rtTertiary.SetWidth(0);
        }


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

        LayoutChild = new LayoutChild(parentRectTransform, parentGo);
        Text.transform.SetParent(parentRectTransform);
        Image.transform.SetParent(parentRectTransform);
        Text.RectTransform.SetHeight(14);
        Text.text.fontSize = 14;
        Text.text.fontStyle = TMPro.FontStyles.Italic;
        Text.text.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
        Text.text.verticalAlignment = TMPro.VerticalAlignmentOptions.Bottom;
    }

    public void ManualUpdate()
    {
        Image.RectTransform.SetWidthMilimeters(LayoutChild.RectTransform.GetWidthMilimeters());
        Image.RectTransform.SetHeight(1);
        LayoutChild.RectTransform.SetHeightMilimeters(6f);
        Image.RectTransform.SetTopYToParent(3);
        Image.RectTransform.SetLeftXToParent(0);
        Text.RectTransform.SetBottomYToParent(3);
        Text.RectTransform.SetLeftXToParent(15);
        Text.RectTransform.SetWidth(LayoutChild.RectTransform.GetWidth() - 15);
        Text.text.SetFontSizePhysical(10);
    }

}

public class LabelWithExpandable
{

    public ExpandableManager ExpandManager;
    public UIUnit MainText;
    private bool _dirty;

    public LayoutChild LayoutChild { get; }

    public IconButton ExpandButton => ExpandManager.ExpandButton;

    public bool Expanded => ExpandManager.Expanded;

    public LabelWithExpandable(IconButton expand, UIUnit mainText)
    {
        ExpandManager = new(expand);
        ExpandManager.ExtraExpandButtons.Add(mainText);
        MainText = mainText;
        this.LayoutChild = LayoutChild.Create(expand.transform, mainText.transform);
        expand.transform.localPosition = Vector3.zero;
        mainText.transform.localPosition = Vector3.zero;
        _dirty = true;
    }

    public void ManualUpdate()
    {
        ExpandManager.ManualUpdate();
        var heightMM = 10; // Fixed height for both buttons

        if (_dirty || EngineView.DpiChanged) 
        {
            // Set height for both buttons
            MainText.RectTransform.SetHeightMilimeters(heightMM);
            ExpandButton.RectTransform.SetHeightMilimeters(heightMM);
            ExpandButton.RectTransform.SetWidthMilimeters(heightMM * 1.5f);
            MainText.text.SetFontSizePhysical(16);
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

            MainText.RectTransform.SetLeftXToParent(10);
            MainText.RectTransform.SetBottomYToParent(0);
        }
        _dirty = false;

        

        

    }

    internal void MarkAsDirty()
    {
        _dirty = true;
    }
}

public class ExpandableManager
{
    public IconButton ExpandButton;
    public List<UIUnit> ExtraExpandButtons = new();
    private bool _expanded = false;
    public List<LayoutChild> ExpandTargets = new();
    public bool Dirty;

    public bool Expanded { get => _expanded && ExpandButton.Active; set => _expanded = value; }

    public ExpandableManager(IconButton expandButton)
    {
        ExpandButton = expandButton;
    }

    public void ManualUpdate()
    {
        var previous = _expanded;
        ExpandButton.Active = ExpandTargets.Count > 0;
        if (!ExpandButton.ActiveSelf) _expanded = true;
        ExpandButton.icon.transform.localEulerAngles = new Vector3(0, 0, Expanded ? 180 : -90);
        if (ExpandButton.Clicked)
        {
            _expanded = !_expanded;
        }
        else
        {
            foreach (var extraB in ExtraExpandButtons)
            {
                if (!extraB.Clicked) continue;
                _expanded = !_expanded;
                break;
            }
        }

        foreach (var item in ExpandTargets)
        {
            item.SetParentShowing(Expanded);
        }
        Dirty = _expanded != previous;
    }
}


