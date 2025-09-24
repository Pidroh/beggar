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
    public class SaveSlotPersistence 
    {
        public List<SaveSlotPersistenceUnit> persistenceUnits = new();
        public int currentSaveSlot;
    }
}
