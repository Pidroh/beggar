using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JsonEntryUnit
{
    public string key;
    public List<TextAsset> jsons;
}

[Serializable]
public class JsonEntries
{
    public List<JsonEntryUnit> entries;
}

