using System;
using System.Collections.Generic;

public class ArcaniaUnits
{
    public Dictionary<UnitType, List<RuntimeUnit>> datas = new();
    public Dictionary<string, IDPointer> IdMapper = new();

    public List<ModData> Mods { get; internal set; } = new();

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
    //public List<BasicUnit> resources = new();
}
