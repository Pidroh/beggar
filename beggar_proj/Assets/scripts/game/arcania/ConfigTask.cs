using HeartUnity;
using System.Collections.Generic;

public class ConfigTask
{
    public const int RESOURCE_CHANGE_LIST_COST = 0;
    public const int RESOURCE_CHANGE_LIST_RESULT = 1;
    public const int RESOURCE_CHANGE_LIST_RUN = 2;
    public const int RESOURCE_CHANGE_LIST_EFFECT = 3;

    public List<ResourceChange> Cost => ResourceChangeLists[RESOURCE_CHANGE_LIST_COST];
    public List<ResourceChange> Result => ResourceChangeLists[RESOURCE_CHANGE_LIST_RESULT];
    public List<ResourceChange> Run => ResourceChangeLists[RESOURCE_CHANGE_LIST_RUN];
    public List<ResourceChange> Effect => ResourceChangeLists[RESOURCE_CHANGE_LIST_EFFECT];
    public AutoNewList<List<ResourceChange>> ResourceChangeLists = new AutoNewList<List<ResourceChange>>();

    public bool Perpetual { get; internal set; }
    public int? Duration { get; internal set; } = null;

    internal List<ResourceChange> GetResourceChangeList(int i)
    {
        return ResourceChangeLists[i];
    }
}
