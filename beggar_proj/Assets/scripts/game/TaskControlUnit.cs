using HeartUnity;
using HeartUnity.View;
using System;
using System.Collections.Generic;

public class TabControlUnit
{
    public LayoutChild SelectionButtonLayoutChild { get; internal set; }
    public RuntimeUnit TabData { get; internal set; }

    public Dictionary<UnitType, List<RTControlUnit>> UnitGroupControls = new();
    internal ButtonWithProgressBar SelectionButton;
    public List<SeparatorInTab> Separators = new();
    public List<LogControlUnit> LogControlUnits = new();

    public class SeparatorInTab
    {
        public List<RTControlUnit> RelatedUnits = new();

        public SeparatorInTab(TabRuntime.Separator sepD)
        {
            Data = sepD;
        }

        public TabRuntime.Separator Data { get; }
        public LayoutChild SeparatorLC { get; internal set; }
        public bool Visible { get => SeparatorLC != null ? SeparatorLC.Visible : false; set { if (SeparatorLC == null) return; SeparatorLC.Visible = value; } }

        public UIUnit SpaceAmountText { get; internal set; }
        public UIUnit Text { get; internal set; }
    }
}


public class ModsControlUnit
{
    public List<TripleTextView> ModTTVs { get; internal set; } = new();
}

public class LogControlUnit 
{ 

}

public class RTControlUnit
{
    public ButtonWithExpandable bwe;
    public LabelWithExpandable lwe;
    public AutoList<ResourceChangeGroup> ChangeGroups = new();
    public AutoList<SeparatorWithLabel> ChangeGroupSeparators = new();
    internal RuntimeUnit Data;
    public bool TaskClicked => bwe.MainButton.Clicked;
    public UIUnit ValueText { get; internal set; }

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

    public bool IsExpanded => (bwe != null && bwe.Expanded) || (lwe != null && lwe.Expanded);

    public ExpandableManager ExpandManager => bwe?.ExpandManager == null ? lwe?.ExpandManager : bwe.ExpandManager;

    public ButtonWithProgressBar ButtonRemove { get; internal set; }
    public ButtonWithProgressBar ButtonAdd { get; internal set; }
    public TabControlUnit.SeparatorInTab ParentTabSeparator { get; internal set; }

    public void ManualUpdate()
    {
        SimpleChild<UIUnit> description = Description;
        description.Element.text.SetFontSizePhysical(15);
        string desc = Data.ConfigBasic.Desc;
        FeedDescription(description, desc);
        if (bwe != null)
        {
            bwe.ManualUpdate();
            bwe.ButtonProgressBar.SetProgress(Data.TaskProgressRatio);
            bwe.MainButton.LongPressMulticlickEnabled = Data.IsInstant();
        }
        if (lwe != null)
        {
            lwe.ManualUpdate();
        }

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
        if (ValueText != null)
        {
            ValueText.rawText = Data.HasMax ? $"{Data.Value} / {Data.Max}" : $"{Data.Value}";
            ValueText.text.SetFontSizePhysical(16);
        }
       
        if (!IsExpanded) return;
        
        
        
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
                for (int ttvIndex = 0; ttvIndex < item.tripleTextViews.Count; ttvIndex++)
                {
                    TripleTextView ttv = item.tripleTextViews[ttvIndex];
                    ttv.LayoutChild.Visible = false;
                }
                continue;
            }


            if (sep != null) sep.ManualUpdate();
            sep.LayoutChild.Visible = resourceChanges.Count > 0;

            for (int ttvIndex = 0; ttvIndex < item.tripleTextViews.Count; ttvIndex++)
            {
                TripleTextView ttv = item.tripleTextViews[ttvIndex];
                var rc = resourceChanges[ttvIndex];

                RuntimeUnit ru = rc.IdPointer.RuntimeUnit;
                ttv.MainText.SetTextRaw(ru.Visible ? ru.Name : "???");
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

public class ResourceChangeGroup
{
    public AutoList<TripleTextView> tripleTextViews = new();
}


