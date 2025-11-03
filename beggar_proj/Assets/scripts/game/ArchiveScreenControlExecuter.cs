using HeartUnity;

public static class ArchiveScreenControlExecuter 
{

    public struct LoadUpArchiveState
    {
        public int slotNow;
        public bool over;
    }
    public static LoadUpArchiveState LoadUpArchive(MainGameControl mgc, LoadUpArchiveState? loadUpState)
    {
        loadUpState ??= new LoadUpArchiveState();
        var state = loadUpState.Value;
        var key = JGameControlDataSaveSlot.SlotSaveKeys[state.slotNow];
        var arcaniaPersistence = new ArcaniaPersistence(mgc.HeartGame, key);

        ArcaniaArchiveModelExecuter.LoadUpArchive(mgc.JControlData.archiveControlData.archiveData, arcaniaPersistence);
        state.slotNow++;
        state.over = JGameControlDataSaveSlot.SlotSaveKeys.Length <= state.slotNow;
        return state;
    }

    public static void ManualUpdate(MainGameControl mgc) 
    {
        if (mgc.JControlData.archiveControlData.ExitJCU.TaskClicked) 
        {
            mgc.BackToGame();
        }
    }

}

public class ArchiveControlData 
{
    public ArcaniaArchiveModelData archiveData = new();

    public JRTControlUnit ExitJCU { get; internal set; }
}