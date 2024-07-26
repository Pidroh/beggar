using HeartUnity.View;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LayoutParent
{
    public LayoutChild SelfChild;
    public List<LayoutChild> LayoutChilds = new();
    public bool[] FitSelfSizeToChildren = new bool[] { false, false };
    public LayoutType TypeLayout = LayoutParent.LayoutType.VERTICAL;

    public LayoutParent(RectTransform rT)
    {
        SelfChild = new LayoutChild()
        {
            RectTransform = rT
        };
    }

    public void ManualUpdate()
    {
        // Get the RectTransform of the parent
        RectTransform parentRectTransform = SelfChild.RectTransform;
        // Get the size of the parent
        Vector2 parentSize = parentRectTransform.rect.size;
        // Initialize offset to position the children
        float offset = 0;

        // Loop through each child in the LayoutChilds list
        foreach (var child in LayoutChilds)
        {
            // Get the RectTransform of the child
            RectTransform childRectTransform = child.RectTransform;
            // Get the pivot of the child
            Vector2 childPivot = childRectTransform.pivot;

            if (TypeLayout == LayoutType.VERTICAL)
            {
                // Set the width of the child to fit the parent
                childRectTransform.sizeDelta = new Vector2(parentSize.x, childRectTransform.sizeDelta.y);
                // Position the child vertically, taking the pivot into account
                childRectTransform.anchoredPosition = new Vector2(0, -offset + (1 - childPivot.y) * childRectTransform.rect.height);
                // Increment the offset by the height of the child
                offset += childRectTransform.rect.height;
            }
            else if (TypeLayout == LayoutType.HORIZONTAL)
            {
                // Set the height of the child to fit the parent
                childRectTransform.sizeDelta = new Vector2(childRectTransform.sizeDelta.x, parentSize.y);
                // Position the child horizontally, taking the pivot into account
                childRectTransform.anchoredPosition = new Vector2(offset - childPivot.x * childRectTransform.rect.width, 0);
                // Increment the offset by the width of the child
                offset += childRectTransform.rect.width;
            }
        }
    }

    internal void AddLayoutChildAndParentIt(LayoutChild layoutChild)
    {
        LayoutChilds.Add(layoutChild);
        layoutChild.RectTransform.SetParent(SelfChild.RectTransform);
    }

    public enum LayoutType
    {
        INVALID,
        VERTICAL,
        HORIZONTAL,
    }
}

public class LayoutChild
{
    public RectTransform RectTransform;
}


public struct Vector2Null
{
    public float? X;
    public float? Y;
}


public class ButtonWithExpandable
{
    public UIUnit MainButton;
    public IconButton ExpandButton;
    public LayoutChild LayoutChild;
    public List<GameObject> ExpandTargets = new();

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
        MainButton.rectTransform.SetHeightMilimeters(heightMM);
        ExpandButton.rectTransform.SetHeightMilimeters(heightMM);
        ExpandButton.rectTransform.SetWidthMilimeters(heightMM);

        var rectTransformParent = LayoutChild.RectTransform;
        MainButton.rectTransform.SetWidthMilimeters(rectTransformParent.GetWidthMilimeters() - heightMM);

        // Set the ExpandButton position on the right side
        var expandButtonWidth = ExpandButton.rectTransform.rect.width;
        var expandButtonHeight = ExpandButton.rectTransform.rect.height;
        /*  ExpandButton.rectTransform.anchoredPosition = new Vector2(
              rectTransformParent.GetWidth() * 0.5f - expandButtonWidth * (0.5f - ExpandButton.rectTransform.pivot.x),
              expandButtonHeight * (0.5f - ExpandButton.rectTransform.pivot.y)
          );*/

        ExpandButton.rectTransform.anchoredPosition = new Vector2(
            rectTransformParent.rect.width * 0.5f - expandButtonWidth * (1 - ExpandButton.rectTransform.pivot.x),
            expandButtonHeight * (0.5f - ExpandButton.rectTransform.pivot.y)
        );

        /**
         * **/

        // Adjust the width of MainButton to occupy remaining space



        // Calculate the correct position for MainButton
        var mainButtonWidth = MainButton.rectTransform.rect.width;
        var mainButtonHeight = MainButton.rectTransform.rect.height;

        // Position the MainButton so its left edge aligns with the parent's left edge
        MainButton.rectTransform.anchoredPosition = new Vector2(
            -rectTransformParent.rect.width * 0.5f + mainButtonWidth * (MainButton.rectTransform.pivot.x),
            mainButtonHeight * (0.5f - MainButton.rectTransform.pivot.y)
        );
    }


}


