using UnityEngine;
using System.Collections.Generic;
using HeartUnity.View;
using UnityEngine.Pool;

public class DialogView
{
    public UIUnit dialogText;
    public ButtonWithProgressBar buttonConfirm;
    public ButtonWithProgressBar buttonCancel;
    public UIUnit fullScreenOverlay;
    public UIUnit parentTransform;

    public bool Visible { get => fullScreenOverlay.Active; internal set => fullScreenOverlay.Active = value; }
}

public class DynamicCanvas
{
    public List<LayoutParent> children = new List<LayoutParent>();
    public List<LayoutParent> childrenForLayouting = new List<LayoutParent>();
    public List<LayoutParent> LowerMenus = new();
    public List<DialogView> DialogViews = new();
    public Queue<LayoutParent> ActiveChildren = new();
    public GameObject canvasGO;

    public bool WidthChangedThisFrame { get; private set; }

    private float _previousWidth;

    public RectTransform RootRT { get; internal set; }
    public Canvas Canvas { get; internal set; }
    public RectTransform OverlayRoot { get; internal set; }
    public List<LayoutParent> OverlayLayoutsSingleActive = new();
    public bool OverlayVisible => OverlayRoot.gameObject.activeSelf;
    private const int minimumDefaultTabPixelWidth = 320;

    // Pixel size adjusted from fall back DPI to actual DPI
    public float DefaultPixelSizeToPhysicalPixelSize => RectTransformExtensions.PixelToMilimiterFallback * RectTransformExtensions.MilimeterToPixel;

    public bool IsDialogActive => IsAnyDialogVisible();

    private bool IsAnyDialogVisible()
    {
        foreach (var dv in DialogViews)
        {
            if (dv.Visible) return true;
        }
        return false;
    }

    internal void AddDialog(DialogView dialogView)
    {
        dialogView.fullScreenOverlay.SetParent(RootRT);
        dialogView.fullScreenOverlay.RectTransform.FillParent();
        dialogView.parentTransform.RectTransform.FillParent();
        dialogView.parentTransform.RectTransform.SetOffsets(10);
        dialogView.Visible = false;
        DialogViews.Add(dialogView);
    }

    public LayoutParent CreateLowerMenuLayout(int height)
    {
        var lc = LayoutChild.Create();
        lc.RectTransform.SetParent(RootRT);
        lc.RectTransform.SetHeight(height);
        lc.RectTransform.FillParentWidth();
        lc.RectTransform.SetBottomYToParent(0);
        LayoutParent layoutParent = CanvasMaker.CreateLayout(lc);
        LowerMenus.Add(layoutParent);
        return layoutParent;
    }

    public int CalculateNumberOfVisibleHorizontalChildren()
    {
        var physicalTabPixelSize = GetAdjustedMinimumTabPixelWidth();
        return Mathf.Max(Mathf.FloorToInt(Screen.width / physicalTabPixelSize), 1);
    }

    private float GetAdjustedMinimumTabPixelWidth()
    {
        return Mathf.Max(minimumDefaultTabPixelWidth * DefaultPixelSizeToPhysicalPixelSize, minimumDefaultTabPixelWidth);
    }

    public void ToggleChild(int childIndex)
    {
        ToggleChild(children[childIndex]);
    }

    public void ShowChild(int childIndex)
    {
        ShowChild(children[childIndex]);
    }

    public void ManualUpdate()
    {
        // Show/Hide children based on Canvas width
        int maxActiveChildrenCount = CalculateNumberOfVisibleHorizontalChildren();
        while (maxActiveChildrenCount < ActiveChildren.Count)
        {
            ActiveChildren.Dequeue();
        }

        // Show EVERY children IF it can show every children AND not every children is shown
        // Once the need to unlock tabs appears (like a tab that only appears in the middle game),
        // this code will likely break (since even locked tabs will show)
        if (maxActiveChildrenCount >= childrenForLayouting.Count && ActiveChildren.Count < childrenForLayouting.Count)
        {
            foreach (var item in children)
            {
                ShowChild(item);
            }
        }

        if (maxActiveChildrenCount > 0)
        {
            int activeChildrenCount = ActiveChildren.Count;
            float availableWidth = Screen.width;
            var minimumTabWidth = GetAdjustedMinimumTabPixelWidth();
            float childWidth = Mathf.Clamp(availableWidth / activeChildrenCount, minimumTabWidth, minimumTabWidth * 2);
            WidthChangedThisFrame = childWidth != _previousWidth;
            _previousWidth = childWidth;

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

            List<LayoutParent> layouts = childrenForLayouting;
            foreach (var layoutP in layouts)
            {
                layoutP.SelfChild.SetVisibleSelf(ActiveChildren.Contains(layoutP));
                if (layoutP.SelfChild.Visible)
                {
                    RectTransform rt = layoutP.SelfChild.RectTransform;
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
            var offsetYLowerMenu = 0f;
            foreach (var lowerMenuP in LowerMenus)
            {
                lowerMenuP.ManualUpdate();
                lowerMenuP.SelfChild.RectTransform.SetBottomYToParent(offsetYLowerMenu);
                offsetYLowerMenu += lowerMenuP.SelfChild.RectTransform.GetHeight();
            }
        }

        foreach (var item in DialogViews)
        {
            if (!item.Visible) continue;
            float dialogWidth = GetAdjustedMinimumTabPixelWidth();
            item.parentTransform.RectTransform.pivot = Vector2.zero;

            item.parentTransform.RectTransform.SetWidth(dialogWidth);
            item.dialogText.RectTransform.SetWidth(dialogWidth - 10 * RectTransformExtensions.MilimeterToPixel);
            item.dialogText.ChangeHeightToFitTextPreferredHeight();
            var buttonHeightMm = 10;
            // only decide height after text width is stabilized
            var dialogHeight = (buttonHeightMm + 5 * 2 + 5 * 1) * RectTransformExtensions.MilimeterToPixel + item.dialogText.text.preferredHeight;
            for (int i = 0; i < 2; i++)
            {
                var rectTransform = i == 0 ? item.buttonConfirm.Button.RectTransform : item.buttonCancel.Button.RectTransform;
                rectTransform.SetBottomLocalY(5 * RectTransformExtensions.MilimeterToPixel);
                rectTransform.SetHeightMilimeters(buttonHeightMm);
                rectTransform.SetWidth(dialogWidth * 0.5f - 10 * RectTransformExtensions.MilimeterToPixel);
                rectTransform.SetLeftLocalX(5 * RectTransformExtensions.MilimeterToPixel + i * (rectTransform.GetWidth() + 5 * RectTransformExtensions.MilimeterToPixel));
            }
            item.dialogText.RectTransform.SetBottomLocalY((5 + buttonHeightMm + 5) * RectTransformExtensions.MilimeterToPixel);
            item.parentTransform.RectTransform.SetHeight(dialogHeight);
            item.parentTransform.RectTransform.SetLeftXToParent((item.fullScreenOverlay.RectTransform.GetWidth() - dialogWidth) * 0.5f);
            item.parentTransform.RectTransform.SetBottomYToParent((item.fullScreenOverlay.RectTransform.GetHeight() - dialogHeight) * 0.5f);
        }
        foreach (var item in OverlayLayoutsSingleActive)
        {
            if (!item.SelfChild.Visible) continue;
            item.ManualUpdate();
        }
    }

    internal void EnableChild(int tabIndex, bool enabled)
    {
        // you might need to do something else when enabling a tab, but for now nothing to do
        // only thing you gotta do for now is to hide if disabled
        if (enabled) return;
        if (ActiveChildren.Contains(children[tabIndex])) 
        {
            HideChild(children[tabIndex]);
        }
    }

    internal bool IsChildVisible(int tabIndex)
    {
        return ActiveChildren.Contains(children[tabIndex]);
    }

    internal void HideAllDialogs()
    {
        foreach (var dv in DialogViews)
        {
            dv.Visible = false;
        }
    }

    internal bool CanShowOnlyOneChild()
    {
        return CalculateNumberOfVisibleHorizontalChildren() <= 1;
    }

    internal void ShowDialog(string id, string title, string content)
    {
        var dv = DialogViews[0];
        dv.Visible = true;
        dv.dialogText.SetTextRaw(content);
        dv.buttonConfirm.Button.SetTextKey(ReusableLocalizationKeys.CST_YES);
        dv.buttonCancel.Button.SetTextKey(ReusableLocalizationKeys.CST_NO);
    }

    internal void ShowOverlay() => OverlayRoot.gameObject.SetActive(true);

    internal void HideOverlay() => OverlayRoot.gameObject.SetActive(false);

    internal void ShowOverlay(LayoutParent overlayLay)
    {
        ShowOverlay();
        foreach (var item in OverlayLayoutsSingleActive)
        {
            item.SelfChild.VisibleSelf = item == overlayLay;
        }
    }

    internal void ToggleChild(LayoutParent layoutParent)
    {
        if (ActiveChildren.Contains(layoutParent))
        {
            HideChild(layoutParent);
        }
        else
        {
            ShowChild(layoutParent);
        }
    }

    private void HideChild(LayoutParent layoutParent)
    {
        using var _1 = ListPool<LayoutParent>.Get(out var list);
        list.AddRange(ActiveChildren);
        ActiveChildren.Clear();
        foreach (var item in list)
        {
            if (item == layoutParent) continue;
            ActiveChildren.Enqueue(item);
        }
    }

    internal void ShowChild(LayoutParent layoutParent)
    {
        if (ActiveChildren.Contains(layoutParent)) return;
        while (childrenForLayouting.Remove(layoutParent)) { }
        childrenForLayouting.Insert(0, layoutParent);
        ActiveChildren.Enqueue(layoutParent);
    }


}

