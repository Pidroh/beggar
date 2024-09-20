using System.Collections.Generic;
using UnityEngine;
using HeartUnity.View;

public class LayoutParent
{
    public LayoutChild SelfChild;
    public List<LayoutChild> Children = new();
    public List<LayoutParent> ChildrenLayoutParents = new();
    public bool[] FitSelfSizeToChildren = new bool[] { false, false };
    public bool[] StretchChildren = new bool[] { false, false };
    public LayoutType TypeLayout = LayoutParent.LayoutType.VERTICAL;
    public RectTransform ContentTransformOverridingSelfChildTransform;
    public RectTransform TransformParentOfChildren => ContentTransformOverridingSelfChildTransform == null ? SelfChild.RectTransform : ContentTransformOverridingSelfChildTransform;

    public LayoutChildAlignment Alignment { get; private set; } = LayoutChildAlignment.LOWER;
    public bool InvertChildrenPositionIndex = false;

    public LayoutParent(RectTransform rT)
    {

        SelfChild = new LayoutChild()
        {
            RectTransform = rT
        };
    }

    public LayoutParent(LayoutChild lC)
    {
        SelfChild = lC;
    }


    public void ManualUpdate()
    {
        // Get the RectTransform of the parent
        RectTransform parentRectTransform = TransformParentOfChildren;


        Vector2Int ForceSize = new Vector2Int(-1, -1);
        if (StretchChildren[0] || StretchChildren[1])
        {
            var totalChildren = 0;
            foreach (var child in Children)
            {
                if (!child.Visible) continue;
                totalChildren++;
            }
            for (int i = 0; i < 2; i++)
            {
                if (StretchChildren[i])
                {
                    ForceSize[i] = Mathf.FloorToInt(parentRectTransform.GetSize()[i] / totalChildren);
                }
            }
        }

        // total children size calculation
        Vector2 totalChildrenOccupiedSize = Vector2.zero;
        foreach (var child in Children)
        {
            if (!child.Visible) continue;
            // Get the RectTransform of the child
            RectTransform childRectTransform = child.RectTransform;
            // Get the pivot of the child
            Vector2 childPivot = childRectTransform.pivot;
            for (int i = 0; i < 2; i++)
            {
                if (child.PreferredSizeMM[i].HasValue) 
                {
                    child.RectTransform.SetSizeMilimeters(i, child.PreferredSizeMM[i].Value);
                }
            }

            if (TypeLayout == LayoutType.VERTICAL)
            {

                // Set the width of the child to fit the parent
                float height = ForceSize.y > 0 ? ForceSize.y : childRectTransform.sizeDelta.y;
                childRectTransform.sizeDelta = new Vector2(parentRectTransform.rect.width, height);

                // Update the total height needed
                totalChildrenOccupiedSize.y += childRectTransform.rect.height;
                totalChildrenOccupiedSize.x = Mathf.Max(totalChildrenOccupiedSize.x, childRectTransform.rect.width);
            }
            else if (TypeLayout == LayoutType.HORIZONTAL)
            {
                // Set the height of the child to fit the parent
                float width = ForceSize.x > 0 ? ForceSize.x : childRectTransform.sizeDelta.x;
                childRectTransform.sizeDelta = new Vector2(width, parentRectTransform.rect.height);

                // Update the total width needed
                totalChildrenOccupiedSize.x += childRectTransform.rect.width;
                totalChildrenOccupiedSize.y = Mathf.Max(totalChildrenOccupiedSize.y, childRectTransform.rect.height);
            }
        }
        // Initialize offset to position the children
        float offset = 0;
        int layoutDimensionIndex = this.TypeLayout == LayoutType.HORIZONTAL ? 0 : 1;
        // if (this.Alignment == LayoutChildAlignment.MIDDLE) offset += totalChildrenOccupiedSize[layoutDimensionIndex] * 0.5f;
        // --------------------------------------------------------------
        // POSITIONING LOOP
        // --------------------------------------------------------------
        // Loop through each child in the LayoutChilds list for positioning
        for (int i = 0; i < Children.Count; i++)
        {
            var index = InvertChildrenPositionIndex ? Children.Count - 1 - i : i;
            LayoutChild child = Children[index];
            if (!child.Visible) continue;
            // Get the RectTransform of the child
            RectTransform childRectTransform = child.RectTransform;
            // Get the pivot of the child
            Vector2 childPivot = childRectTransform.pivot;

            if (TypeLayout == LayoutType.VERTICAL)
            {
                var offsetY = 0f;

                if (Alignment == LayoutChildAlignment.LOWER)
                {
                    childRectTransform.SetPivotAndAnchors(Vector3.one);
                    offsetY = 0;
                }
                if (Alignment == LayoutChildAlignment.MIDDLE)
                {
                    childRectTransform.SetPivotAndAnchors(new Vector2(1, 0.5f));
                    offsetY = totalChildrenOccupiedSize.y * 0.5f;
                }
                // no support for UPPER yet

                
                // Position the child vertically, taking the pivot into account
                childRectTransform.anchoredPosition = new Vector2(0, -offset + offsetY);
                // Increment the offset by the height of the child
                offset += childRectTransform.rect.height;
            }
            else if (TypeLayout == LayoutType.HORIZONTAL)
            {
                // Position the child horizontally, taking the pivot into account
                childRectTransform.anchoredPosition = new Vector2(offset - totalChildrenOccupiedSize.x / 2 + childPivot.x * childRectTransform.GetWidth(), 0);
                // Increment the offset by the width of the child
                offset += childRectTransform.rect.width;
            }
        }

        // Update the parent's size if FitSelfSizeToChildren is true
        if (FitSelfSizeToChildren[0]) // Horizontal fit
        {
            parentRectTransform.sizeDelta = new Vector2(totalChildrenOccupiedSize.x, parentRectTransform.sizeDelta.y);
        }
        if (FitSelfSizeToChildren[1]) // Vertical fit
        {
            parentRectTransform.sizeDelta = new Vector2(parentRectTransform.sizeDelta.x, totalChildrenOccupiedSize.y);
        }

        foreach (var lp in ChildrenLayoutParents)
        {
            lp.ManualUpdate();
        }
    }

    internal LayoutParent SetLayoutType(LayoutType type)
    {
        TypeLayout = type;
        return this;
    }

    internal LayoutParent SetLayoutChildAlignment(LayoutChildAlignment alignment)
    {
        Alignment = alignment;
        return this;
    }

    internal LayoutParent SetStretchWidth(bool b)
    {
        StretchChildren[0] = b;
        return this;
    }

    internal LayoutParent SetStretchHeight(bool b)
    {
        StretchChildren[1] = b;
        return this;
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

    internal void AddLayoutChildAndParentIt(UIUnit unit)
    {
        AddLayoutChildAndParentIt(new LayoutChild()
        {
            RectTransform = unit.RectTransform
        });
    }

    public enum LayoutType
    {
        INVALID,
        VERTICAL,
        HORIZONTAL,
    }

    public enum LayoutChildAlignment
    {
        INVALID,
        LOWER,
        MIDDLE,
        UPPER
    }
}


