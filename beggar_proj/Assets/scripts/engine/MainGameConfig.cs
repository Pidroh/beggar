using HeartUnity.View;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HeartUnity
{

    // Define your ScriptableObject class
    [CreateAssetMenu(fileName = "MainGameConfig", menuName = "Custom/Main Game Config", order = 1)]
    public class MainGameConfig : ScriptableObject
    {
        public int versionNumber;
        public MusicDataList musicList;
        public AudioDataList audioList;
        public AudioDataList[] voiceLists;
        public TextAsset SettingData;
        public TextAsset Credits;   
        public TextAsset localizationData;
        public List<string> blacklistedLanguages;
        public View viewConfig;
        public InputPromptVisuals inputPromptVisuals;
        public List<PersistenceUnit> PersistenceUnits;

        [Serializable]
        public class View
        {
            public CursorView cursorView;
            public MouseAsSpriteInfo mouseAsSprite;
            public PostProcessingScale bloomConfig;
        }

        [Serializable]
        public class PersistenceUnit 
        {
            public bool ForcePrefs;
            public string Key;
        }
    }
}