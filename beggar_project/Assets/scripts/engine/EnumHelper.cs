using System;
using System.Collections.Generic;

public static class EnumHelper<TEnum> where TEnum : System.Enum
{
    private static Dictionary<string, TEnum> enumNameToValueMap;
    private static Dictionary<TEnum, string> enumValueToNameMap;
    private static List<TEnum> allEnumValues; // New cache for all enum values

    public static string GetName(TEnum enumValue)
    {
        if (enumValueToNameMap == null)
        {
            InitializeEnumNamesAndMap();
        }

        if (enumValueToNameMap.TryGetValue(enumValue, out string name))
        {
            return name;
        }

        return string.Empty;
    }

    public static bool TryGetEnumFromName(string enumName, out TEnum enumValue)
    {
        if (enumNameToValueMap == null)
        {
            InitializeEnumNamesAndMap();
        }

        if (enumNameToValueMap.TryGetValue(enumName, out enumValue))
        {
            return true;
        }

        enumValue = default(TEnum);
        return false;
    }

    public static List<TEnum> GetAllValues()
    {
        if (allEnumValues == null)
        {
            InitializeEnumNamesAndMap();
        }

        return allEnumValues;
    }

    private static void InitializeEnumNamesAndMap()
    {
        enumNameToValueMap = new Dictionary<string, TEnum>();
        enumValueToNameMap = new Dictionary<TEnum, string>();
        allEnumValues = new List<TEnum>();

        Array values = Enum.GetValues(typeof(TEnum));
        foreach (TEnum value in values)
        {
            string name = value.ToString();
            enumNameToValueMap[name] = value;
            enumValueToNameMap[value] = name;
            allEnumValues.Add(value);
        }
    }
}
