﻿using HeartUnity.View;
using System.Collections.Generic;
using UnityEngine;

public class LayoutParent
{
    public LayoutChild SelfChild;
    public List<LayoutChild> LayoutChilds = new();
    public bool[] FitSelfSizeToChildren = new bool[] { false, false };
    public LayoutType TypeLayout = LayoutParent.LayoutType.VERTICAL;

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
    
}

