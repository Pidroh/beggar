using HeartUnity;
using HeartUnity.View;
using System;
using System.Collections.Generic;

public class ResourceControlUnit 
{
    public LabelWithExpandable lwe;
    public SimpleChild<UIUnit> Description { get; internal set; }
    public RuntimeUnit Data { get; internal set; }
    public UIUnit ValueText { get; internal set; }

    public List<SeparatorWithLabel> Separators = new();
    public ModsControlUnit ModsUnit = new();

    internal void ManualUpdate()
    {
        lwe.ManualUpdate();
        TaskControlUnit.FeedDescription(Description, Data.ConfigBasic.Desc);
        ValueText.rawText = Data.HasMax ? $"{Data.Value} / {Data.Max}" : $"{Data.Value}";
        if (!lwe.ExpandManager.Expanded) return;
        

        foreach (var sep in Separators)
        {
            sep.ManualUpdate();
        }
    }
}

public class ModsControlUnit {
    public List<TripleTextView> ModTTVs { get; internal set; } = new();
}

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
    
    public Gauge XPGauge { get; internal set; }
    public SimpleChild<UIUnit> MainTitle { get; internal set; }
    public UIUnit SkillLevelText { get; internal set; }

    public List<SeparatorWithLabel> Separators = new();

    public ModsControlUnit ModsUnit = new();

    public class ResourceChangeGroup
    {
        public AutoList<TripleTextView> tripleTextViews = new();
    }
    public void ManualUpdate()
    {
        bwe.ManualUpdate();
        bwe.ButtonProgressBar.SetProgress(Data.TaskProgressRatio);
        bwe.MainButton.LongPressMulticlickEnabled = Data.IsInstant();
        if (Data.ConfigBasic.UnitType == UnitType.SKILL)
        {
            MainTitle.Element.SetTextRaw(Data.Name);
            MainTitle.LayoutChild.RectTransform.SetHeight(MainTitle.Element.text.preferredHeight + 20);
            MainTitle.ManualUpdate();
            XPGauge.SetRatio(Data.Skill.XPRatio);
            XPGauge.layoutChild.Visible = Data.Skill.Acquired;
            SkillLevelText.Active = Data.Skill.Acquired;
            SkillLevelText.rawText = $"Lvl: {Data.Value} / {Data.Max}";
            bwe.ButtonProgressBar.Button.rawText = Data.Skill.Acquired ? "Practice skill" : "Acquire Skill";
        }
        if (!bwe.Expanded) return;
        SimpleChild<UIUnit> description = Description;
        string desc = Data.ConfigBasic.Desc;
        FeedDescription(description, desc);
        foreach (var sep in Separators)
        {
            sep.ManualUpdate();
        }

        for (int i = 0; i < ChangeGroups.Count; i++)
        {

            var sep = ChangeGroupSeparators[i];

            ResourceChangeGroup item = ChangeGroups[i];
            var resourceChanges = Data.ConfigTask.GetResourceChangeList(i);
            if (item == null || (Data.Skill != null && Data.Skill.Acquired && i == (int)ResourceChangeType.COST))
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

    public static void FeedDescription(SimpleChild<UIUnit> description, string desc)
    {
        description.LayoutChild.Visible = !string.IsNullOrWhiteSpace(desc);
        description.LayoutChild.RectTransform.SetHeight(description.Element.text.preferredHeight + 10);
        description.Element.SetTextRaw(desc);
        description.ManualUpdate();
    }
}

