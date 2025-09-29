using HeartUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public static class JGameControlExecuterSaveSlot
{
    public static string[] ClassPriorityTier { get; } = new string[] { "t_job", "t_tier0", "t_tier1", "t_tier2", "t_tier3", "t_tier4", "t_tier5" };
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
        // check if the save slot tab is visible and if not, interrupt this update
        foreach (var tabC in mgc.JControlData.TabControlUnits)
        {
            var tabData = tabC.TabData;
            if (!tabData.Tab.ContainsSaveSlots) continue;
            if (!JGameControlExecuter.IsTabVisibleAndShowing(mgc, tabC)) return;
        }
        if (mgc.JControlData.SaveSlots.ActionHappenedLastFrameSoSkipActions) 
        {
            mgc.JControlData.SaveSlots.ActionHappenedLastFrameSoSkipActions = willSkipInputNextFrame;
            return;
        }
        var hasEmptySlot = SaveSlotExecution.HasEmptySlot(mgc.JControlData.SaveSlots.ModelData);
        for (int slot = 0; slot < cd.SaveSlots.saveSlots.Count; slot++)
        {
            JGameControlExecuter.UpdateExpandLogicForUnit(mgc.JControlData.SaveSlots.slotControlUnits[slot]);
            var slotD = mgc.JControlData.SaveSlots.ModelData.saveSlots[slot];
            JGameControlDataSaveSlot.ControlSaveSlotUnit slotControlUnit = cd.SaveSlots.saveSlots[slot];
            slotControlUnit.importButton?.MainLayout.SetVisibleSelf(!slotD.hasSave);
            slotControlUnit.exportButton.MainLayout.SetVisibleSelf(slotD.hasSave);
            slotControlUnit.copyButton.MainLayout.SetVisibleSelf(hasEmptySlot && slotD.hasSave);
            bool notCurrentSlot = slot != mgc.JControlData.SaveSlots.ModelData.currentSlot;
            slotControlUnit.newGameOrLoadGameButton.MainLayout.SetVisibleSelf(notCurrentSlot);
            slotControlUnit.newGameOrLoadGameButton.MainExecuteButton.SetButtonTextRaw(slotD.hasSave ? Local.GetText("Load_game") : Local.GetText("New_game"));
            slotControlUnit.TextForTimeStuff.SetTextRaw($"{PlayTimeControlCenter.ConvertSecondsToTimeFormat(slotD.playTimeSeconds)}\n{Local.GetText("Last save: ")}{slotD.lastSaveTime.ToString("yy/MM/dd HH:mm:ss")}");
            slotControlUnit.TextForFlavor.SetTextRaw((notCurrentSlot ? "" : Local.GetText("Current slot") + "\n") +slotD.representativeText);
            if (slotControlUnit.newGameOrLoadGameButton.TaskClicked)
            {
                var isNewGame = slotD.hasSave == false;
                if (isNewGame) 
                {
                    SaveDataCenter.DeleteSaveFromKey(LoadingScreenControl.SlotSaveKeys[slot], mgc.HeartGame);
                }
                // has to call reload scene before changing slot, or else the slot information is saved to the wrong slot
                mgc.ReloadScene();
                SaveSlotExecution.ChangeSlotAndSaveSlotData(mgc.HeartGame, mgc.JControlData.SaveSlots.ModelData, slot);
                
            }
            if (slotControlUnit.exportButton.TaskClicked)
            {
                willSkipInputNextFrame = true;
                slotControlUnit.exportButton.ConsumeClick();
                // save before exporting
                if (slot == mgc.JControlData.SaveSlots.ModelData.currentSlot) 
                {
                    mgc.SaveGameAndCurrentSlot();
                }
                if (mgc.ArcaniaPersistence.saveUnit.TryLoadRawText(out var rawText))
                {
                    var zipBytes = ZipUtilities.CreateZipBytesFromVirtualFiles(new List<string>(new string[] { "exported_unit" }), new List<string>(new string[] { rawText }));
                    new FileUtilities().ExportBytes(zipBytes, $"beggar_single_savedata{System.DateTime.Now.ToString("yyyy_M_d_H_m_s")}", "beggar");
                }
            }
            if (slotControlUnit.importButton?.TaskClicked ?? false)
            {
                willSkipInputNextFrame = true;
                slotControlUnit.exportButton.ConsumeClick();
                mgc.JControlData.SaveSlots.ImportingSlotSave = slot;
                mgc.JControlData.SaveSlots.FileUtilities.ImportFileRequest("beggar");
                Debug.Log("import file request...?");
            }
            if (slotControlUnit.copyButton?.TaskClicked ?? false) 
            {
                int? slotTarget = SaveSlotExecution.CopySlotToEmptySlot(slot, mgc.JControlData.SaveSlots.ModelData);
                if (slotTarget.HasValue) 
                {
                    SaveDataCenter.CopyPersistentTextFromTwoKeys(sourceKey: JGameControlDataSaveSlot.SlotSaveKeys[slot], targetKey: JGameControlDataSaveSlot.SlotSaveKeys[slotTarget.Value], heartGame: mgc.HeartGame);
                }
            } 
            if (slotControlUnit.deleteButton?.TaskClicked ?? false) 
            {
                SaveSlotExecution.DeleteSlot(mgc.JControlData.SaveSlots.ModelData, slot);
            }
        }
        mgc.JControlData.SaveSlots.ActionHappenedLastFrameSoSkipActions = willSkipInputNextFrame;
    }
}
