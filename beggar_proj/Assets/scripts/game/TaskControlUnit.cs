using HeartUnity;
using HeartUnity.View;
using System;
using System.Collections.Generic;

public class TaskControlUnit
{
    public ButtonWithExpandable bwe;
    public AutoList<ResourceChangeGroup> ChangeGroups = new();
    public AutoList<SeparatorWithLabel> ChangeGroupSeparators = new();
    internal RuntimeUnit Data;
    public bool TaskClicked => bwe.MainButton.Clicked;

    public ResourceChangeGroup CostGroup { get => ChangeGroups[0]; set => ChangeGroups[0] = value; }
    public ResourceChangeGroup ResultGroup { get => ChangeGroups[1]; set => ChangeGroups[1] = value; }
    public ResourceChangeGroup RunGroup { get => ChangeGroups[2]; set => ChangeGroups[2] = value; }
    public ResourceChangeGroup EffectGroup { get => ChangeGroups[3]; set => ChangeGroups[3] = value; }
    public SimpleChild<UIUnit> Description { get; internal set; }
    public List<TripleTextView> ModTTVs { get; internal set; } = new();

    public List<SeparatorWithLabel> Separators = new();

    public class ResourceChangeGroup
    {
        public AutoList<TripleTextView> tripleTextViews = new();
    }
    public void ManualUpdate()
    {
        bwe.ManualUpdate();
        bwe.ButtonProgressBar.SetProgress(Data.TaskProgress);
        if (!bwe.Expanded) return;
        Description.LayoutChild.Visible = !string.IsNullOrWhiteSpace(Data.ConfigBasic.Desc);
        Description.LayoutChild.RectTransform.SetHeight(Description.Element.text.preferredHeight);
        Description.Element.SetTextRaw(Data.ConfigBasic.Desc);
        Description.ManualUpdate();
        foreach (var sep in Separators)
        {
            sep.ManualUpdate();
        }

        for (int i = 0; i < ChangeGroups.Count; i++)
        {
            
            var sep = ChangeGroupSeparators[i];
            
            ResourceChangeGroup item = ChangeGroups[i];
            var resourceChanges = Data.ConfigTask.GetResourceChangeList(i);
            if (item == null) 
            {
                if (sep != null) sep.LayoutChild.Visible = false;
                continue;
            }
            

            if (sep != null) sep.ManualUpdate();
            sep.LayoutChild.Visible = resourceChanges.Count > 0;

            for (int ttvIndex = 0; ttvIndex < item.tripleTextViews.Count; ttvIndex++)
            {
                TripleTextView ttv = item.tripleTextViews[ttvIndex];
                var rc = resourceChanges[ttvIndex];

                RuntimeUnit ru = rc.IdPointer.RuntimeUnit;
                ttv.MainText.SetTextRaw(ru.Name);
                ttv.SecondaryText.SetTextRaw($"{rc.valueChange}");
                ttv.TertiaryText.SetTextRaw($"({ru.Value} / {ru.Max})");
                ttv.ManualUpdate();
            }
        }
    }
}

