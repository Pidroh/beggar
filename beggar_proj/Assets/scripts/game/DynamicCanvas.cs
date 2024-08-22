using UnityEngine;
using System.Collections.Generic;
using HeartUnity.View;

public class DynamicCanvas
{
    public List<LayoutParent> children = new List<LayoutParent>();
    public List<LayoutParent> LowerMenus = new();
    public Queue<LayoutParent> ActiveChildren = new();
    public GameObject canvasGO;

    public RectTransform RootRT { get; internal set; }
    public Canvas Canvas { get; internal set; }
    public RectTransform OverlayRoot { get; internal set; }
    public LayoutParent OverlayMainLayout { get; set; }

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

        // Show EVERY children IF it can show every children AND not every children is shown
        // Once the need to unlock tabs appears (like a tab that only appears in the middle game),
        // this code will likely break (since even locked tabs will show)
        if (maxActiveChildrenCount >= children.Count && ActiveChildren.Count < children.Count) {
            foreach (var item in children)
            {
                ShowChild(item);
            }
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

