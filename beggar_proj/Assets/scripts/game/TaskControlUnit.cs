using HeartUnity;


public class TaskControlUnit
{
    public ButtonWithExpandable bwe;
    public AutoList<ResourceChangeGroup> ChangeGroups = new();
    internal RuntimeUnit Data;
    public bool TaskClicked => bwe.MainButton.Clicked;

    public ResourceChangeGroup CostGroup { get => ChangeGroups[0]; set => ChangeGroups[0] = value; }
    public ResourceChangeGroup ResultGroup { get => ChangeGroups[1]; set => ChangeGroups[1] = value; }
    public ResourceChangeGroup RunGroup { get => ChangeGroups[2]; set => ChangeGroups[2] = value; }
    public ResourceChangeGroup EffectGroup { get => ChangeGroups[3]; set => ChangeGroups[3] = value; }

    public class ResourceChangeGroup
    {
        public AutoList<TripleTextView> tripleTextViews = new();
    }
    public void ManualUpdate()
    {
        bwe.ManualUpdate();
        for (int i = 0; i < ChangeGroups.Count; i++)
        {
            ResourceChangeGroup item = ChangeGroups[i];
            var resourceChanges = Data.ConfigTask.GetResourceChangeList(i);
            if (item == null) continue;

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

