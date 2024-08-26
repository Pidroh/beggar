//using UnityEngine.U2D;

using HeartUnity.View;
using System;
using System.Collections.Generic;
using UnityEngine;
using static HeartUnity.SettingModel;

namespace HeartUnity
{
    [Serializable]
    public class CommonPlayerSaveData 
    {
        public int TotalPlayTimeSeconds;
    }

    public class PlayTimeControl
    {
        public static DateTime? dateTimePreviousScene;
        public int PlayTimeToShow => Mathf.CeilToInt(playTime);
        public float playTime;
        public void Update()
        {
            playTime += Time.unscaledDeltaTime;
        }

        internal void Init(MiscPlayerStats playerMisc)
        {
            playTime = playerMisc.totalPlayTimeSeconds;
            // get time between scene transitions
            if (dateTimePreviousScene.HasValue)
            {
                if (dateTimePreviousScene < DateTime.Now)
                {
                    var secs = Mathf.Min((float)(DateTime.Now - dateTimePreviousScene.Value).TotalSeconds, 60f);
                    playTime += Mathf.CeilToInt(secs);
                }
                dateTimePreviousScene = null;
            }

        }

        internal void BeforeChangeScene()
        {
            dateTimePreviousScene = DateTime.Now;
        }
    }


    public class SettingPersistence
    {
        public SaveDataUnit<SettingPersistenceData> saveDataUnit = new("settings", true);

        internal void LoadMethod(List<SettingUnitRealTime> unitControls)
        {
            if (saveDataUnit.TryLoad(out var data))
            {
                foreach (var uc in unitControls)
                {
                    foreach (var su in data.settingUnits)
                    {
                        if (uc.settingData.id == su.id)
                        {
                            uc.rtBool = su.dataB;
                            uc.rtInt = su.dataI;
                            uc.rtFloat = su.dataF;
                            uc.rtString = su.dataS;
                        }
                    }
                }
            }
        }

        internal void SaveMethod(List<SettingUnitRealTime> unitControls)
        {
            var spd = new SettingPersistenceData();
            foreach (var uc in unitControls)
            {
                var spu = new SettingPersistenceUnit();
                spu.id = uc.settingData.id;
                spu.dataB = uc.rtBool;
                spu.dataI = uc.rtInt;
                spu.dataF = uc.rtFloat;
                spu.dataS = uc.rtString;
                spd.settingUnits.Add(spu);
            }
            saveDataUnit.Save(spd);
        }
    }

    [Serializable]
    public class SettingPersistenceData
    {
        public List<SettingPersistenceUnit> settingUnits = new List<SettingPersistenceUnit>();
    }

    [Serializable]
    public class SettingPersistenceUnit
    {
        public string id;
        public bool dataB;
        public float dataF;
        public int dataI;
        public string dataS;
    }
}