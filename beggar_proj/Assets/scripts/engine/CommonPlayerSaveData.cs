//using UnityEngine.U2D;

using System;

namespace HeartUnity
{
    [Serializable]
    public class CommonPlayerSaveData
    {
        public int TotalPlayTimeSeconds;
    }

    public class CommonPlayerSaveDataPersistence
    {
        // if you ever implement multiple slots or something of the sort, you're gonna have to change this
        SaveDataUnit<CommonPlayerSaveData> SaveDataUnit;
        public CommonPlayerSaveDataPersistence(string key, HeartGame heartGame) 
        {
            SaveDataUnit = new SaveDataUnit<CommonPlayerSaveData>(key, heartGame);
        }
        public bool TryLoad(out CommonPlayerSaveData playerSaveData)
        {
            var r = SaveDataUnit.TryLoad(out var obj);
            playerSaveData = obj;
            return r;
        }

        public void Save(CommonPlayerSaveData playerSaveData) 
        {
            SaveDataUnit.Save(playerSaveData);
        }
    }
}