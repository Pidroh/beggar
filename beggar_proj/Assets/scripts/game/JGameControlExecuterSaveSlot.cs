using HeartUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public static class JGameControlExecuterSaveSlot
{
    public static void ManualUpdate(MainGameControl mgc)
    {
        var cd = mgc.JControlData;
        bool willSkipInputNextFrame = false;
        if (mgc.JControlData.SaveSlots.ImportingSlotSave.HasValue && mgc.JControlData.SaveSlots.FileUtilities.UploadedBytes != null)
        {
            var slot = mgc.JControlData.SaveSlots.ImportingSlotSave.Value;
            mgc.JControlData.SaveSlots.ImportingSlotSave = null;
            var _1 = ListPool<string>.Get(out var titles);
            var _2 = ListPool<string>.Get(out var content);
            ZipUtilities.ExtractZipFromBytes(mgc.JControlData.SaveSlots.FileUtilities.UploadedBytes, titles, content);
            mgc.JControlData.SaveSlots.FileUtilities.ResetBytes();
            willSkipInputNextFrame = true;
            if (titles.Count == 1 && content.Count == 1 && titles[0] == "exported_unit")
            {
                var slotKey = JGameControlDataSaveSlot.SlotSaveKeys[slot];
                var pus = mgc.HeartGame.config.PersistenceUnits;
                foreach (var item in pus)
                {
                    if (item.Key == slotKey) 
                    {
                        var ptu = new PersistentTextUnit(item, mgc.HeartGame);
                        ptu.Save(content[0]);
                    }
                }
            }
        }
        if (mgc.JControlData.SaveSlots.ActionHappenedLastFrameSoSkipActions) 
        {
            mgc.JControlData.SaveSlots.ActionHappenedLastFrameSoSkipActions = willSkipInputNextFrame;
            return;
        }
        var hasEmptySlot = SaveSlotExecution.HasEmptySlot(mgc.JControlData.SaveSlots.ModelData);
        for (int slot = 0; slot < cd.SaveSlots.saveSlots.Count; slot++)
        {
            var slotD = mgc.JControlData.SaveSlots.ModelData.saveSlots[slot];
            JGameControlDataSaveSlot.ControlSaveSlotUnit item = cd.SaveSlots.saveSlots[slot];
            item.copyButton.MainExecuteButton.SetVisible(hasEmptySlot && slotD.hasSave);
            if (item.newGameButton.TaskClicked)
            {
                mgc.SaveArcaniaMainSlot();
                SaveSlotExecution.ChangeSlotAndSaveSlotData(mgc.HeartGame, mgc.JControlData.SaveSlots.ModelData, slot);
                mgc.ReloadScene();
            }
            if (item.exportButton.TaskClicked)
            {
                willSkipInputNextFrame = true;
                item.exportButton.ConsumeClick();
                // save before exporting
                if (slot == mgc.JControlData.SaveSlots.ModelData.currentSlot) 
                {
                    mgc.SaveArcaniaMainSlot();
                }
                if (mgc.ArcaniaPersistence.saveUnit.TryLoadRawText(out var rawText))
                {
                    var zipBytes = ZipUtilities.CreateZipBytesFromVirtualFiles(new List<string>(new string[] { "exported_unit" }), new List<string>(new string[] { rawText }));
                    new FileUtilities().ExportBytes(zipBytes, $"beggar_single_savedata{System.DateTime.Now.ToString("yyyy_M_d_H_m_s")}", "beggar");
                }
            }
            if (item.importButton?.TaskClicked ?? false)
            {
                willSkipInputNextFrame = true;
                item.exportButton.ConsumeClick();
                mgc.JControlData.SaveSlots.ImportingSlotSave = slot;
                mgc.JControlData.SaveSlots.FileUtilities.ImportFileRequest("beggar");
                Debug.Log("import file request...?");
            }
            if (item.copyButton?.TaskClicked ?? false) 
            {
                int slotTarget = SaveSlotExecution.CopySlotToEmptySlot(slot, mgc.JControlData.SaveSlots);
            } 
            if (item.deleteButton?.TaskClicked ?? false) 
            {
                
            }
        }
        mgc.JControlData.SaveSlots.ActionHappenedLastFrameSoSkipActions = willSkipInputNextFrame;
    }
}
