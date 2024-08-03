using System.Collections.Generic;
using UnityEngine;
using HeartUnity.View;

public class LayoutParent
{
    public LayoutChild SelfChild;
    public List<LayoutChild> Children = new();
    public List<LayoutParent> ChildrenLayoutParents = new();
    public bool[] FitSelfSizeToChildren = new bool[] { false, false };
    public LayoutType TypeLayout = LayoutParent.LayoutType.VERTICAL;
    public RectTransform ContentTransformOverridingSelfChildTransform;
    public RectTransform TransformParentOfChildren => ContentTransformOverridingSelfChildTransform == null ? SelfChild.RectTransform : ContentTransformOverridingSelfChildTransform;

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
        RectTransform parentRectTransform = TransformParentOfChildren;
        // Initialize offset to position the children
        float offset = 0;

        // Initialize variables to keep track of total width and height needed
        float totalWidth = 0;
        float totalHeight = 0;

        // Loop through each child in the LayoutChilds list
        foreach (var child in Children)
        {
            if (!child.Visible) continue;
            // Get the RectTransform of the child
            RectTransform childRectTransform = child.RectTransform;
            // Get the pivot of the child
            Vector2 childPivot = childRectTransform.pivot;

            if (TypeLayout == LayoutType.VERTICAL)
            {

                childRectTransform.SetPivotAndAnchors(Vector3.one);
                // Set the width of the child to fit the parent
                childRectTransform.sizeDelta = new Vector2(parentRectTransform.rect.width, childRectTransform.sizeDelta.y);
                // Position the child vertically, taking the pivot into account
                childRectTransform.anchoredPosition = new Vector2(0, -offset);
                // Increment the offset by the height of the child
                offset += childRectTransform.rect.height;

                // Update the total height needed
                totalHeight += childRectTransform.rect.height;
                totalWidth = Mathf.Max(totalWidth, childRectTransform.rect.width);
            }
            else if (TypeLayout == LayoutType.HORIZONTAL)
            {
                // Set the height of the child to fit the parent
                childRectTransform.sizeDelta = new Vector2(childRectTransform.sizeDelta.x, parentRectTransform.rect.height);
                // Position the child horizontally, taking the pivot into account
                childRectTransform.anchoredPosition = new Vector2(offset - childPivot.x * childRectTransform.rect.width, 0);
                // Increment the offset by the width of the child
                offset += childRectTransform.rect.width;

                // Update the total width needed
                totalWidth += childRectTransform.rect.width;
                totalHeight = Mathf.Max(totalHeight, childRectTransform.rect.height);
            }
        }

        // Update the parent's size if FitSelfSizeToChildren is true
        if (FitSelfSizeToChildren[0]) // Horizontal fit
        {
            parentRectTransform.sizeDelta = new Vector2(totalWidth, parentRectTransform.sizeDelta.y);
        }
        if (FitSelfSizeToChildren[1]) // Vertical fit
        {
            parentRectTransform.sizeDelta = new Vector2(parentRectTransform.sizeDelta.x, totalHeight);
        }

        foreach (var lp in ChildrenLayoutParents)
        {
            lp.ManualUpdate();
        }
    }


    public LayoutParent SetFitWidth(bool b)
    {
        FitSelfSizeToChildren[0] = b;
        return this;
    }

    public LayoutParent SetFitHeight(bool b)
    {
        FitSelfSizeToChildren[1] = b;
        return this;
    }

    internal void AddLayoutAndParentIt(LayoutParent layout)
    {
        AddLayoutChildAndParentIt(layout.SelfChild);
        ChildrenLayoutParents.Add(layout);
    }

    internal void AddLayoutChildAndParentIt(LayoutChild layoutChild)
    {
        Children.Add(layoutChild);
        layoutChild.RectTransform.SetParent(this.TransformParentOfChildren);
    }

    public enum LayoutType
    {
        INVALID,
        VERTICAL,
        HORIZONTAL,
    }
}


