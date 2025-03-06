using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArcaniaGameConfigurationUnit", menuName = "Arcania/Arcania Game Configuration Unit", order = 1)]
public class ArcaniaGameConfigurationUnit : ScriptableObject
{
    public List<TextAsset> jsonDatas;
    public TextAsset layoutJson;

}
