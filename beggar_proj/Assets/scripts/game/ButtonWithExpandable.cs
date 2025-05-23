﻿using HeartUnity.View;
using System.Collections.Generic;
using UnityEngine;

public class ButtonWithExpandable
{
    public UIUnit MainButton;
    public ButtonWithProgressBar ButtonProgressBar;
    private Color _originalColorButton;
    private Color _selectedColorButton;
    private Color _originalColorProgress;
    private readonly Color _selectedColorProgress;
    private readonly Color _disabledColorProgress;

    public IconButton ExpandButton => ExpandManager.ExpandButton;
    public LayoutChild LayoutChild;
    public List<LayoutChild> ExpandTargets => ExpandManager.ExpandTargets;

    public ExpandableManager ExpandManager;
    private bool _dirty;
    private bool _selected;

    public bool Expanded => ExpandManager.Expanded;

    public bool MainButtonEnabled { get => MainButton.ButtonEnabled; internal set => SetMainButtonEnabled(value); }


    internal void MainButtonSelected(bool selected)
    {
        _selected = selected;
        MainButton.NormalColor = selected ? _selectedColorButton : _originalColorButton;
        if (!selected) return;

    }

    private void SetMainButtonEnabled(bool value)
    {
        MainButton.ButtonEnabled = value;
        ButtonProgressBar.ProgressImage.Image.color = value ? (_selected ? _selectedColorProgress : _originalColorProgress) : _disabledColorProgress;
    }

    public static implicit operator LayoutChild(ButtonWithExpandable a) => a.LayoutChild;

    public ButtonWithExpandable(ButtonWithProgressBar button, IconButton iconButton, CanvasMaker.CreateButtonRequest buttonRequest)
    {
        ExpandManager = new(iconButton);
        MainButton = button.Button;
        ButtonProgressBar = button;
        _originalColorButton = MainButton.Image.color;
        _selectedColorButton = buttonRequest.MainBody.SelectedColor;
        _originalColorProgress = ButtonProgressBar.ProgressImage.Image.color;
        _disabledColorProgress = new Color(_originalColorProgress.r * 0.7f, _originalColorProgress.g * 0.7f, _originalColorProgress.b * 0.7f, _originalColorProgress.a);
        _selectedColorProgress = new Color(_originalColorProgress.r, Mathf.Min(_originalColorProgress.g + 0.08f, 1f), Mathf.Min(_originalColorProgress.b + 0.08f, 1f), _originalColorProgress.a);

        this.LayoutChild = LayoutChild.Create(MainButton.transform, iconButton.transform);
        MainButton.transform.localPosition = Vector3.zero;
        iconButton.transform.localPosition = Vector3.zero;
        _dirty = true;
    }

    public void ManualUpdate()
    {

        ExpandManager.ManualUpdate();
        // Layout updating
        if (!_dirty && !EngineView.DpiChanged) {
            return;
        }
        _dirty = false;
        var heightMM = 10; // Fixed height for both buttons

        // Set height for both buttons
        MainButton.RectTransform.SetHeightMilimeters(heightMM);
        ExpandButton.RectTransform.SetHeightMilimeters(heightMM);
        ExpandButton.RectTransform.SetWidthMilimeters(heightMM * 1.5f);
        MainButton.text.SetFontSizePhysical(16);


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

    internal void MarkAsDirty()
    {
        _dirty = true;
    }

    internal void SetActive(bool visible)
    {
        this.LayoutChild.SetVisibleSelf(visible);
    }

}


