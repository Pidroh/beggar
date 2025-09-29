using HeartUnity;
using JLayout;
using System.Collections.Generic;

public class JGameControlDataSaveSlot 
{
    public static string[] SlotSaveKeys { get; } = new string[] { "maindata", "maindata_1", "maindata_2" };
    public JRTControlUnit forceSaveButton { get; internal set; }
    public SaveSlotModelData ModelData { get; internal set; }
    public List<JRTControlUnit> slotControlUnits = new();
    public PlaytimeUnit PlayTimeOfActiveSlot;

    // which slot you are importing from
    public int? ImportingSlotSave { get; internal set; }
    public FileUtilities FileUtilities { get; internal set; }
    public bool ActionHappenedLastFrameSoSkipActions { get; internal set; }

    public List<ControlSaveSlotUnit> saveSlots = new();
    public class ControlSaveSlotUnit 
    {
        public JRTControlUnit newGameOrLoadGameButton { get; internal set; }
        public JRTControlUnit deleteButton { get; internal set; }
        public JRTControlUnit exportButton { get; internal set; }
        public JRTControlUnit importButton;
        public JRTControlUnit copyButton { get; internal set; }
        public JLayTextAccessor TextForTimeStuff { get; internal set; }
        public JLayTextAccessor TextForFlavor { get; internal set; }
    }
}

