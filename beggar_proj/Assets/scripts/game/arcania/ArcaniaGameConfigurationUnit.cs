using HeartUnity;
using HeartUnity.View;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArcaniaGameConfigurationUnit", menuName = "Arcania/Arcania Game Configuration Unit", order = 1)]
public class ArcaniaGameConfigurationUnit : ScriptableObject
{
    public List<TextAsset> jsonDatas;
    public List<TextAsset> jsonDatasWorld;
    public TextAsset layoutJson;
    public KeyedSprites spritesForLayout;
    public LocalizedTextAsset arcaniaTranslationFile;
    public string gameTitleText;
    public string gameSubTitleText;
}

