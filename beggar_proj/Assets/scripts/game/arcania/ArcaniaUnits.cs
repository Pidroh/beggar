using System;
using System.Collections.Generic;

public class ArcaniaUnits
{
    public Dictionary<UnitType, List<RuntimeUnit>> datas = new();
    public Dictionary<string, IDPointer> IdMapper = new();

    public List<ModRuntime> Mods { get; internal set; } = new();
    public List<ModRuntime> SpaceMods { get; internal set; } = new();
    public RuntimeUnit RestActionActive { get; internal set; }

    internal IDPointer GetOrCreateIdPointer(string key)
    {
        if (!IdMapper.TryGetValue(key, out var value))
        {
            value = new IDPointer()
            {
                id = key
            };
            IdMapper[key] = value;
        }
        return value;
    }

    internal IDPointer GetOrCreateIdPointerWithTag(string id)
    {
        var pointer = GetOrCreateIdPointer(id);
        if (pointer.Tag == null)
        {
            pointer.Tag = new TagRuntime(id);
        }
        return pointer;
    }

    //public List<BasicUnit> resources = new();
}
