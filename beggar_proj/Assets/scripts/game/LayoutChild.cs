using HeartUnity;
using HeartUnity.View;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayoutChild
{
    public RectTransform RectTransform;
    internal bool Visible { get => RectTransform.gameObject.activeSelf; set => RectTransform.gameObject.SetActive(value); }
}


public struct Vector2Null
{
    public float? X;
    public float? Y;
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
        Text.RectTransform.SetHeight(12);
        Text.text.fontSize = 12;
        Text.text.fontStyle = TMPro.FontStyles.Italic;
    }

    public void ManualUpdate() 
    {
        Image.RectTransform.SetWidthMilimeters(LayoutChild.RectTransform.GetWidthMilimeters());
        Image.RectTransform.SetHeight(1);
        LayoutChild.RectTransform.SetHeight(18);
        Image.RectTransform.SetTopYToParent(0);
        Image.RectTransform.SetLeftXToParent(0);
        Text.RectTransform.SetBottomYToParent(0);
        Text.RectTransform.SetLeftXToParent(15);
    }

}

public class ButtonWithExpandable
{
    public UIUnit MainButton;
    public IconButton ExpandButton;
    public LayoutChild LayoutChild;
    public List<GameObject> ExpandTargets = new();

    public static implicit operator LayoutChild(ButtonWithExpandable a) => a.LayoutChild;

    public ButtonWithExpandable(UIUnit button, IconButton iconButton)
    {
        ExpandButton = iconButton;
        MainButton = button;
        GameObject parentGo = new GameObject();
        RectTransform parentRectTransform = parentGo.AddComponent<RectTransform>();

        LayoutChild = new LayoutChild()
        {
            RectTransform = parentRectTransform
        };
        button.transform.SetParent(parentRectTransform);
        iconButton.transform.SetParent(parentRectTransform);
        button.transform.localPosition = Vector3.zero;
        iconButton.transform.localPosition = Vector3.zero;

    }

    public void ManualUpdate()
    {
        var heightMM = 10; // Fixed height for both buttons

        // Set height for both buttons
        MainButton.RectTransform.SetHeightMilimeters(heightMM);
        ExpandButton.RectTransform.SetHeightMilimeters(heightMM);
        ExpandButton.RectTransform.SetWidthMilimeters(heightMM);


        var rectTransformParent = LayoutChild.RectTransform;
        rectTransformParent.SetHeightMilimeters(heightMM);
        MainButton.RectTransform.SetWidthMilimeters(rectTransformParent.GetWidthMilimeters() - heightMM);

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


