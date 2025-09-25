using HeartUnity;
using System.Collections.Generic;

public static class JGameControlExecuterSaveSlot 
{
    public static void ManualUpdate(MainGameControl mgc) 
    {
        var cd = mgc.JControlData;
        for (int slot = 0; slot < cd.SaveSlots.saveSlots.Count; slot++)
        {
            JGameControlDataSaveSlot.ControlSaveSlotUnit item = cd.SaveSlots.saveSlots[slot];
            if (item.newGameButton.TaskClicked) 
            {
                mgc.SaveArcaniaMainSlot();
                SaveSlotExecution.ChangeSlotAndLoadCurrentScene(mgc.HeartGame, mgc.JControlData.SaveSlots.ModelData, slot);
            }
            if (item.exportButton.TaskClicked) 
            {
                if (mgc.ArcaniaPersistence.saveUnit.TryLoadRawText(out var rawText)) 
                {
                    var zipBytes = ZipUtilities.CreateZipBytesFromVirtualFiles(new List<string>(new string[] { "exported_unit" }), new List<string>(new string[] { rawText }));
                    new FileUtilities().ExportBytes(zipBytes, $"beggar_single_savedata{System.DateTime.Now.ToString("yyyy_M_d_H_m_s")}", "beggar");
                }
            }
            if (item.importButton.TaskClicked) 
            {
                mgc.JControlData.SaveSlots.ImportingSlotSave = true;
            }
            if (mgc.JControlData.SaveSlots.ImportingSlotSave) 
            { 

            }
        }
    }
}
