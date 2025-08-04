//using UnityEngine.U2D;

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeartUnity
{
    public class SettingModel
    {
        public SettingPersistence persistence;
        public List<SettingUnitRealTime> unitControls = new List<SettingUnitRealTime>();
        public TextAsset defaultData;
        public float fullScreenChangeHot = 0f;

        public class SettingUnitRealTime
        {
            public SettingUnitData settingData;
            public bool rtBool;
            public int rtInt;
            public float rtFloat;
            public string rtString;

            internal void ReadDefaultData()
            {
                rtString = settingData.defaultValueString;
                rtBool = settingData.defaultValueBool.HasValue ? settingData.defaultValueBool.Value : false;
                rtInt = settingData.defaultValueInt.HasValue ? settingData.defaultValueInt.Value : 0;
                rtFloat = settingData.defaultValueFloat.HasValue ? settingData.defaultValueFloat.Value : 0;
            }
        }


        public void SaveData()
        {
            persistence.SaveMethod(unitControls);
        }

        public void Enforce(SettingUnitRealTime uc)
        {
            switch (uc.settingData.standardSettingType)
            {
                case SettingUnitData.StandardSettingType.FULLSCREEN:
                    Screen.fullScreen = uc.rtBool;
                    fullScreenChangeHot = 1f;
                    break;
                case SettingUnitData.StandardSettingType.MASTER_VOLUME:
                    AudioConfig.masterVolume = uc.rtFloat;
                    break;
                case SettingUnitData.StandardSettingType.MUSIC_VOLUME:
                    AudioConfig.musicVolume = uc.rtFloat;
                    break;
                case SettingUnitData.StandardSettingType.SFX_VOLUME:
                    AudioConfig.sfxVolume = uc.rtFloat;
                    break;
                case SettingUnitData.StandardSettingType.VOICE_VOLUME:
                    AudioConfig.voiceVolume = uc.rtFloat;
                    break;
                case SettingUnitData.StandardSettingType.EXIT_GAME:
                    break;
                case SettingUnitData.StandardSettingType.EXIT_MENU:
                    break;
                case SettingUnitData.StandardSettingType.LANGUAGE_SELECTION:
                    Local.ChangeLanguage(uc.rtString);
                    break;
                default:
                    break;
            }
        }

        public SettingModel Init(TextAsset data, HeartGame heartGame)
        {
            this.defaultData = data;
            unitControls.Clear();
            persistence = new SettingPersistence(heartGame);
            ReadData();
            Load();
            Enforce();
            return this;
        }

        internal void Enforce()
        {
            foreach (var uc in unitControls)
            {
                Enforce(uc);
            }
        }

        internal void ReadData()
        {
            var data = defaultData.text;
            var settingDatas = ReadSettings(data);
            foreach (var sd in settingDatas)
            {
                var suc = new SettingUnitRealTime();
                suc.settingData = sd;
                suc.ReadDefaultData();
                unitControls.Add(suc);
            }
        }

        public static List<SettingUnitData> ReadSettings(string data)
        {
            List<SettingUnitData> settingsList = new List<SettingUnitData>();

            string[] lines = data.Split('\n');

            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                {
                    string[] values = line.Split(',');

                    SettingUnitData settingData = new SettingUnitData
                    {
                        id = values[0].Trim(),
                        titleTexstring = values[1].Trim(),
                        settingType = EnumHelper<SettingUnitData.SettingType>.TryGetEnumFromName(values[2].Trim(), out var settingTypeEnum)
                            ? settingTypeEnum
                            : SettingUnitData.SettingType.BUTTON,
                        standardSettingType = EnumHelper<SettingUnitData.StandardSettingType>.TryGetEnumFromName(values[3].Trim(), out var standardSettingTypeEnum)
                            ? standardSettingTypeEnum
                            : SettingUnitData.StandardSettingType.FULLSCREEN,
                    };

                    // Add the code to handle the "Default Value" field
                    if (values.Length > 4)
                    {
                        string defaultValueString = values[4].Trim();

                        // Determine the data type and assign the value to the appropriate property
                        if (int.TryParse(defaultValueString, out int intValue))
                            settingData.defaultValueInt = intValue;
                        else if (float.TryParse(defaultValueString, out float floatValue))
                            settingData.defaultValueFloat = floatValue;
                        else if (bool.TryParse(defaultValueString, out bool boolValue))
                            settingData.defaultValueBool = boolValue;
                        else
                            settingData.defaultValueString = defaultValueString;
                    }

                    settingsList.Add(settingData);
                }
            }

            return settingsList;
        }

        public void SetInt(SettingUnitData.StandardSettingType setting, int data)
        {
            foreach (var uc in this.unitControls)
            {
                if (uc.settingData.standardSettingType == setting)
                {
                    uc.rtInt = data;
                    Enforce(uc);
                }
            }
            SaveData();
        }

        public void SetString(SettingUnitData.StandardSettingType setting, string data)
        {
            foreach (var uc in this.unitControls)
            {
                if (uc.settingData.standardSettingType == setting)
                {
                    uc.rtString = data;
                    Enforce(uc);
                }
            }
            SaveData();
        }

        public void ManualUpdate(float dt)
        {
            if (fullScreenChangeHot > 0)
            {
                fullScreenChangeHot -= dt;
            }

        }

        internal bool CheckForDiscrepancies()
        {
            var discrepant = false;
            foreach (var uc in unitControls)
            {
                switch (uc.settingData.standardSettingType)
                {
                    case SettingUnitData.StandardSettingType.FULLSCREEN:
                        if (uc.rtBool != Screen.fullScreen && fullScreenChangeHot <= 0)
                        {
                            uc.rtBool = Screen.fullScreen;
                            Debug.Log(uc.rtBool + " discrepant detected");
                            discrepant = true;
                        }
                        break;
                }
            }
            if (discrepant)
                SaveData();
            return discrepant;
        }

        private void Load()
        {
            persistence.LoadMethod(unitControls);
        }

        public class SettingUnitData
        {
            public string id;
            public string titleTexstring;
            public SettingType settingType;
            public StandardSettingType standardSettingType;
            internal int? defaultValueInt;
            internal float? defaultValueFloat;
            internal bool? defaultValueBool;
            internal string defaultValueString;

            public enum StandardSettingType
            {
                FULLSCREEN, EXIT_GAME, EXIT_MENU, MASTER_VOLUME, MUSIC_VOLUME, SFX_VOLUME, VOICE_VOLUME,
                LANGUAGE_SELECTION, DELETE_DATA, 
                PP_COLOR_CORRECTION, PP_BLOOM, PP_TONE, PP_SCANLINE, PP_VIGNETTE,
                SHOW_CREDITS, EXPORT_SAVE, IMPORT_SAVE, DISCORD_SERVER, CUSTOM_CHOICE_1
            }

            public enum SettingType
            {
                BUTTON, SWITCH, SLIDER
            }
        }
    }

    [Serializable]
    public class SettingCustomChoice 
    {
        public string id;
        public string upperText;
        public List<string> choiceKeys;
        
    }
}