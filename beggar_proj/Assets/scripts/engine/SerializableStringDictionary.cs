//using UnityEngine.U2D;

using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class SerializableStringDictionary
{
    [SerializeField]
    private List<string> keys = new List<string>();

    [SerializeField]
    private List<string> values = new List<string>();

    public void Add(string key, string value)
    {
        keys.Add(key);
        values.Add(value);
    }

    public string Get(string key)
    {
        int index = keys.IndexOf(key);
        if (index != -1)
        {
            return values[index];
        }
        else
        {
            Debug.LogError($"Key not found: {key}");
            return default(string);
        }
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        for (int i = 0; i < keys.Count; i++)
        {
            yield return new KeyValuePair<string, string>(keys[i], values[i]);
        }
    }

    public bool Contains(string value)
    {
        return keys.Contains(value);
    }
}
