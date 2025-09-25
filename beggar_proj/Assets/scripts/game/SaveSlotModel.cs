using HeartUnity;
using System;
using System.Collections.Generic;

public class SaveSlotExecuterIO 
{ 

}



public class SaveSlotModel 
{
    public List<SaveSlotUnit> saveSlots = new();
    
    public class SaveSlotUnit 
    {
        public string representativeText; 
        public DateTime lastSaveTime;
        public int playTimeSeconds;
    }

    [Serializable]
    public class SaveSlotPersistenceUnit
    {
        public string representativeText;
        public string lastSaveTime;
        public int playTimeSeconds;
    }

    [Serializable]
    public class SaveSlotPersistenceData
    {
        public List<SaveSlotPersistenceUnit> persistenceUnits = new();
        public int currentSaveSlot;
    }

    public class SaveSlotPersistence
    {
        public SaveDataUnit<SaveSlotPersistenceData> saveUnit;

        public SaveSlotPersistence(HeartGame heartGame)
        {
            saveUnit = new SaveDataUnit<SaveSlotPersistenceData>("save_slots", heartGame);
        }
    }
}
