using HeartUnity;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.SceneManagement;

// this code will likely eventually be moved to engine side, so it shouldn't be dependent on game 
public static class SaveSlotExecution
{

    public static SaveSlotModelData LoadSlotModel(int slotNumber, HeartGame heartGame)
    {
        var ssp = new SaveSlotModelData.SaveSlotPersistence(heartGame);
        SaveSlotModelData slotModel = new();
        if (ssp.saveUnit.TryLoad(out var slotData))
        {
            slotModel = new();
            slotModel.currentSlot = slotData.currentSaveSlot;
            foreach (var slotU in slotData.persistenceUnits)
            {
                SaveSlotModelData.SaveSlotUnit unit = new();
                unit.hasSave = slotU.hasSave;
                unit.lastSaveTime = System.DateTime.ParseExact(slotU.lastSaveTime, "yyMMdd_HHmmss", CultureInfo.InvariantCulture);
                unit.playTimeSeconds = slotU.playTimeSeconds;
                unit.representativeText = slotU.representativeText;
                slotModel.saveSlots.Add(unit);
            }
        }
        while (slotModel.saveSlots.Count < slotNumber)
        {
            SaveSlotModelData.SaveSlotUnit slot = new();
            slotModel.saveSlots.Add(slot);
        }

        // if there are too many slots, remove them
        slotModel.saveSlots.RemoveRange(slotNumber, slotModel.saveSlots.Count - slotNumber);
        return slotModel;
    }

    public static void ChangeSlotAndSaveSlotData(HeartGame heartGame, SaveSlotModelData model, int newSlot)
    {
        model.currentSlot = newSlot;
        SaveData(model, heartGame);
    }

    private static void SaveData(SaveSlotModelData model, HeartGame heartGame)
    {
        var p = new SaveSlotModelData.SaveSlotPersistenceData();
        p.currentSaveSlot = model.currentSlot;
        foreach (var item in model.saveSlots)
        {
            p.persistenceUnits.Add(new()
            {
                playTimeSeconds = item.playTimeSeconds,
                representativeText = item.representativeText,
                lastSaveTime = item.lastSaveTime.ToString("yyMMdd_HHmmss"),
                hasSave = item.hasSave
            });
        }
        new SaveSlotModelData.SaveSlotPersistence(heartGame).saveUnit.Save(p);
    }

    internal static bool HasEmptySlot(SaveSlotModelData modelData)
    {
        foreach (var item in modelData.saveSlots)
        {
            if (!item.hasSave) return true;
        }
        return false;
    }

    internal static int? CopySlotToEmptySlot(int slot, SaveSlotModelData model)
    {
        var sourceSlot = model.saveSlots[slot];
        for (int slotTarget = 0; slotTarget < model.saveSlots.Count; slotTarget++)
        {
            SaveSlotModelData.SaveSlotUnit targetSlot = model.saveSlots[slotTarget];
            if (!targetSlot.hasSave) 
            {
                targetSlot.CopyFrom(sourceSlot);
                return slotTarget;
            }
        }
        return null;
    }

    internal static void InitCurrentSlotIfNoSave(SaveSlotModelData slotData, string representativeTextKey)
    {
        var csu = slotData.CurrentSlotUnit;
        csu.hasSave = true;
        csu.lastSaveTime = DateTime.Now;
        csu.playTimeSeconds = 0;
        csu.representativeText = representativeTextKey;
    }

    internal static void DeleteSlot(SaveSlotModelData modelData, int slot)
    {
        modelData.saveSlots[slot].hasSave = false;
    }
}



public class SaveSlotModelData
{
    public List<SaveSlotUnit> saveSlots = new();
    public int currentSlot;
    public SaveSlotUnit CurrentSlotUnit => saveSlots[currentSlot];

    public class SaveSlotUnit
    {
        public string representativeText;
        public DateTime lastSaveTime;
        public int playTimeSeconds;
        public bool hasSave;

        internal void CopyFrom(SaveSlotUnit sourceSlot)
        {
            this.representativeText = sourceSlot.representativeText;
            this.playTimeSeconds = sourceSlot.playTimeSeconds;
            this.lastSaveTime = sourceSlot.lastSaveTime;
            this.hasSave = sourceSlot.hasSave;
        }
    }

    [Serializable]
    public class SaveSlotPersistenceUnit
    {
        public string representativeText;
        public string lastSaveTime;
        public int playTimeSeconds;
        public bool hasSave;
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
