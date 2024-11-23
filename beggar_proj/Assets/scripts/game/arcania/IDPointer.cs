using System;
using UnityEngine;

public class IDPointer
{
    public RuntimeUnit RuntimeUnit;
    public string id;

    public TagRuntime Tag { get; internal set; }

    internal float GetValue()
    {
        if (RuntimeUnit != null)
        {
            return RuntimeUnit.Value;
        }
        if (Tag != null)
        {
            // tags are either 1 (has tag) or 0 (no tag)
            foreach (var child in Tag.UnitsWithTag)
            {
                if (child.Value > 0) return 1;
            }
            return 0;
        }
#if UNITY_EDITOR
        Debug.LogError($"ID Pointer {id} has no value!");
#endif
        return 0;
    }

    internal bool IsAllMaxed()
    {
        if(RuntimeUnit != null) 
        {
            return RuntimeUnit.IsMaxed;
        }
        foreach (var item in Tag.UnitsWithTag)
        {
            if (!item.IsMaxed) return false;
        }
        return true;
    }

    internal bool IsAllZero()
    {
        if (RuntimeUnit != null)
        {
            return RuntimeUnit.Value <= 0;
        }
        foreach (var item in Tag.UnitsWithTag)
        {
            if (item.Value > 0) return false;
        }
        return true;
    }
}
