//using UnityEngine.U2D;

using HeartUnity.View;
using System;
using System.Collections.Generic;
using UnityEngine;
using static HeartUnity.SettingModel;

namespace HeartUnity
{


    public class SettingPersistence
    {
        public const string Key = "settings";
        public SaveDataUnit<SettingPersistenceData> saveDataUnit;

        public SettingPersistence(HeartGame heartGame) 
        {
             saveDataUnit = new(Key, heartGame);
        }

        internal void LoadMethod(List<SettingUnitRealTime> unitControls)
        {
            Debug.Log($"Setting load try");
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
            else
            {
                Debug.Log($"Setting load failed");
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