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

public class ConditionControlUnit
{
    public TripleTextView TTV { get; internal set; }
}

public class ModsControlUnit
{
    public List<TripleTextView> ModTTVs { get; internal set; } = new();
}

public class LogControlUnit
{
    public LogControlUnit(LayoutChild lc, UIUnit text)
    {
        Lc = lc;
        Text = text;
    }

    public LayoutChild Lc { get; }
    public UIUnit Text { get; }
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
    public ConditionControlUnit needConditionUnit = new();

    public bool IsExpanded => (bwe != null && bwe.Expanded) || (lwe != null && lwe.Expanded);

    public ExpandableManager ExpandManager => bwe?.ExpandManager == null ? lwe?.ExpandManager : bwe.ExpandManager;

    public ButtonWithProgressBar ButtonRemove { get; internal set; }
    public ButtonWithProgressBar ButtonAdd { get; internal set; }
    public TabControlUnit.SeparatorInTab ParentTabSeparator { get; internal set; }
    public SimpleChild<UIUnit> DurationText { get; internal set; }

    public void ManualUpdate(ArcaniaModel arcaniaModel)
    {
        if (ButtonAdd != null)
        {
            ButtonAdd.ManualUpdate();
            ButtonRemove.ManualUpdate();
            ButtonAdd.SetWidthMM(15);
            ButtonRemove.SetWidthMM(15);
        }

        SimpleChild<UIUnit> description = Description;
        description.Element.text.SetFontSizePhysical(15);
        string desc = Data.ConfigBasic.Desc;
        FeedDescription(description, desc);
        var duration = Data.ConfigTask != null && Data.ConfigTask.Duration.HasValue ? Data.ConfigTask.Duration.Value : -1;

        if (duration > 0) DurationText.Element.rawText = $"Duration: {duration}s";
        if (bwe != null)
        {
            bwe.ManualUpdate();
            float progRatio = Data.TaskProgressRatio;
            if (Data.Location != null) progRatio = arcaniaModel.Exploration.LastActiveLocation == Data ? arcaniaModel.Exploration.ExplorationRatio : 0f;
            bwe.ButtonProgressBar.SetProgress(progRatio);
            bwe.MainButton.LongPressMulticlickEnabled = Data.IsInstant();
        }
        if (lwe != null)
        {
            lwe.ManualUpdate();
        }
        if (DurationText != null)
            DurationText.Visible = duration > 0 && DurationText.Visible;
        if (Data.ConfigBasic.UnitType == UnitType.SKILL)
        {
            MainTitle.Element.SetTextRaw(Data.Name);
            MainTitle.LayoutChild.RectTransform.SetHeight(MainTitle.Element.text.preferredHeight + 20);
            MainTitle.ManualUpdate();
            MainTitle.Element.text.SetFontSizePhysical(16);
            XPGauge.SetRatio(Data.Skill.XPRatio);
            XPGauge.ManualUpdate();
            XPGauge.layoutChild.Visible = Data.Skill.Acquired;
            SkillLevelText.Active = Data.Skill.Acquired;
            SkillLevelText.rawText = $"Lvl: {Data.Value} / {Data.Max}";
            SkillLevelText.text.SetFontSizePhysical(16);
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

        UpdateChangeGroups();

    }

    public void UpdateChangeGroups()
    {
        if (Data == null) return;
        for (int i = 0; i < ChangeGroups.Count; i++)
        {
            ResourceChangeType resourceChangeType = (ResourceChangeType)i;
            var sep = ChangeGroupSeparators[i];

            ResourceChangeGroup item = ChangeGroups[i];
            var resourceChanges = Data.ConfigTask.GetResourceChangeList(i);
            if (item == null || (Data.Skill != null && Data.Skill.Acquired && resourceChangeType == ResourceChangeType.COST))
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
            var bySecond = i == (int)ResourceChangeType.EFFECT || i == (int)ResourceChangeType.RUN;

            for (int ttvIndex = 0; ttvIndex < item.tripleTextViews.Count; ttvIndex++)
            {
                TripleTextView ttv = item.tripleTextViews[ttvIndex];
                ttv.Visible = resourceChanges.Count > ttvIndex;
                if (!ttv.Visible) continue;
                var rc = resourceChanges[ttvIndex];

                var min = rc.valueChange.min;
                var max = rc.valueChange.max;

                string targetName;
                string tertiaryText = "";
                if (rc.IdPointer.RuntimeUnit != null)
                {
                    RuntimeUnit dataThatWillBeChanged = rc.IdPointer.RuntimeUnit;
                    min += dataThatWillBeChanged.GetModSumWithIntermediaryCheck(Data, ModType.ResourceChangeChanger, resourceChangeType);
                    max += dataThatWillBeChanged.GetModSumWithIntermediaryCheck(Data, ModType.ResourceChangeChanger, resourceChangeType);
                    targetName = dataThatWillBeChanged.Visible ? dataThatWillBeChanged.Name : "???";
                    if(dataThatWillBeChanged.HasMax)
                        tertiaryText = $"({dataThatWillBeChanged.Value} / {dataThatWillBeChanged.Max})";
                    else
                        tertiaryText = $"({dataThatWillBeChanged.Value})";
                }
                else
                {
                    targetName = rc.IdPointer.Tag.tagName;
                }

                ttv.MainText.SetTextRaw(targetName);

                //float valueChange = rc.valueChange;

                var valueText = min != max ? $"{min}~{max}" : $"{min}";
                ttv.SecondaryText.SetTextRaw(bySecond ? $"{valueText}/s" : $"{valueText}");
                ttv.TertiaryText.SetTextRaw(tertiaryText);
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

    internal void SetVisible(bool visible)
    {
        bwe?.LayoutChild.RectTransform.parent.gameObject.SetActive(visible);
        lwe?.LayoutChild.RectTransform.parent.gameObject.SetActive(visible);
    }
}

public class ResourceChangeGroup
{
    public AutoList<TripleTextView> tripleTextViews = new();
}


