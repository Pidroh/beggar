using HeartEngineCore;
using System;
using System.Collections;
using System.Collections.Generic;

public class IDPointer : IEnumerable<RuntimeUnit>
{
    public IEnumerable<RuntimeUnit> RuntimeUnits => GetEnumerable();
    public bool noRuntimeUnit;

    private IEnumerable<RuntimeUnit> GetEnumerable()
    {
        
        if (Tag != null) return Tag.UnitsWithTag;
        _listOfRunTimeForEnumeration ??= new();
        if (RuntimeUnit == null) 
        {
            if (!noRuntimeUnit && id == "space")
            {
                noRuntimeUnit = true;
            }
            if (noRuntimeUnit)
            {
                return _listOfRunTimeForEnumeration;
            }
            Logger.LogError($"ERROR: ID Pointer {id} seems to be invalid");
        }
        if (_listOfRunTimeForEnumeration.Count == 0) 
        {
            _listOfRunTimeForEnumeration.Add(RuntimeUnit);
        }
        return _listOfRunTimeForEnumeration;
    }

    public RuntimeUnit RuntimeUnit;
    private List<RuntimeUnit> _listOfRunTimeForEnumeration;
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
        Logger.LogError($"ID Pointer {id} has no value!");
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

    public IEnumerator<RuntimeUnit> GetEnumerator()
    {
        return RuntimeUnits.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)RuntimeUnits).GetEnumerator();
    }

    public void CheckValidity()
    {
        if (Tag != null) return;
        if (RuntimeUnit != null) return;
        if (id == "space") return;
#if UNITY_EDITOR
        Logger.LogError($"ID Pointer {id} is invalid");
#endif
    }
}
