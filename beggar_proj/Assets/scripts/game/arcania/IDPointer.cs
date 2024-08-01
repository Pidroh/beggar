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
        Debug.Log($"ID Pointer {id} has no value!");
#endif
        return 0;
    }
}
