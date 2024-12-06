using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
#endif

[CreateAssetMenu(fileName = "ArcaniaGameConfigurationUnit", menuName = "Arcania/Arcania Game Configuration Unit", order = 1)]
public class ArcaniaGameConfigurationUnit : ScriptableObject
{
    public List<TextAsset> jsonDatas;

}

#if UNITY_EDITOR
#endif

public class DeleteMe : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
