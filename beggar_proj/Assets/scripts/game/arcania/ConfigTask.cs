using arcania;
using HeartUnity;
using System.Collections.Generic;

public class ConfigTask
{

    public List<ResourceChange> Cost => GetResourceChangeList(ResourceChangeType.COST);
    public List<ResourceChange> Result => GetResourceChangeList(ResourceChangeType.RESULT);
    public List<ResourceChange> Run => GetResourceChangeList(ResourceChangeType.RUN);
    public List<ResourceChange> Effect => GetResourceChangeList(ResourceChangeType.EFFECT);
    public AutoNewList<List<ResourceChange>> ResourceChangeLists = new AutoNewList<List<ResourceChange>>();

    public bool Perpetual { get; internal set; }
    public int? Duration { get; internal set; } = null;
    public string SlotKey { get; internal set; }
    public ConditionalExpression Need { get; internal set; }

    internal List<ResourceChange> GetResourceChangeList(int i)
    {
        return ResourceChangeLists[i];
    }

    internal List<ResourceChange> GetResourceChangeList(ResourceChangeType i)
    {
        return ResourceChangeLists[(int) i];
    }

    
}
