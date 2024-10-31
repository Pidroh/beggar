using UnityEngine;
using System.Collections.Generic;
using HeartUnity.View;

public class DialogView
{
    public UIUnit dialogText;
    public ButtonWithProgressBar buttonConfirm;
    public ButtonWithProgressBar buttonCancel;
    public UIUnit fullScreenOverlay;
    public UIUnit parentTransform;

    public bool IsVisible { get => fullScreenOverlay.Active; internal set => fullScreenOverlay.Active = value; }
}

public class DynamicCanvas
{

    public List<LayoutParent> children = new List<LayoutParent>();
    public List<LayoutParent> LowerMenus = new();
    public List<DialogView> DialogViews = new();
    public Queue<LayoutParent> ActiveChildren = new();
    public GameObject canvasGO;

    public RectTransform RootRT { get; internal set; }
    public Canvas Canvas { get; internal set; }
    public RectTransform OverlayRoot { get; internal set; }
    public LayoutParent OverlayMainLayout { get; set; }
    public bool OverlayVisible => OverlayRoot.gameObject.activeSelf;
    private const int minimumDefaultTabPixelWidth = 320;

    // Pixel size adjusted from fall back DPI to actual DPI
    public float DefaultPixelSizeToPhysicalPixelSize => RectTransformExtensions.PixelToMilimiterFallback * RectTransformExtensions.MilimeterToPixel;

    public bool IsDialogActive { get; internal set; }

    internal void AddDialog(DialogView dialogView)
    {
        dialogView.fullScreenOverlay.SetParent(RootRT);
        dialogView.fullScreenOverlay.RectTransform.FillParent();
        dialogView.parentTransform.RectTransform.FillParent();
        dialogView.parentTransform.RectTransform.SetOffsets(10);
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
        if (maxActiveChildrenCount >= children.Count && ActiveChildren.Count < children.Count)
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
            if (!item.IsVisible) continue;
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

        OverlayMainLayout.ManualUpdate();
    }

    internal void ShowDialog(string id, string title, string content)
    {
        var dv = DialogViews[0];
        dv.dialogText.SetTextRaw(content);
        dv.buttonConfirm.Button.SetTextKey(ReusableLocalizationKeys.CST_YES);
        dv.buttonCancel.Button.SetTextKey(ReusableLocalizationKeys.CST_NO);
    }

    internal void ShowOverlay() => OverlayRoot.gameObject.SetActive(true);

    internal void HideOverlay() => OverlayRoot.gameObject.SetActive(false);

    internal void ShowChild(LayoutParent layoutParent)
    {
        if (ActiveChildren.Contains(layoutParent)) return;
        ActiveChildren.Enqueue(layoutParent);

    }


}

