using arcania;
using HeartUnity;
using System.Collections.Generic;

public class ConfigTask
{

    public List<ResourceChange> Cost => GetResourceChangeList(ResourceChangeType.COST);
    public List<ResourceChange> Result => GetResourceChangeList(ResourceChangeType.RESULT);
    public List<ResourceChange> ResultOnce => GetResourceChangeList(ResourceChangeType.RESULT_ONCE);
    public List<ResourceChange> ResultFail => GetResourceChangeList(ResourceChangeType.RESULT_FAIL);
    public List<ResourceChange> Run => GetResourceChangeList(ResourceChangeType.RUN);
    public List<ResourceChange> Effect => GetResourceChangeList(ResourceChangeType.EFFECT);
    public AutoNewList<List<ResourceChange>> ResourceChangeLists = new AutoNewList<List<ResourceChange>>();

    public bool Perpetual { get; set; }
    public int? Duration { get; set; } = null;
    public string SlotKey { get; set; }
    public ConditionalExpression Need { get; set; }
    public int? SuccessRatePercent { get; set; } = null;

    public List<ResourceChange> GetResourceChangeList(int i)
    {
        return ResourceChangeLists[i];
    }

    public List<ResourceChange> GetResourceChangeList(ResourceChangeType i)
    {
        return ResourceChangeLists[(int) i];
    }

    
}
