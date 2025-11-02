using HeartUnity;
using System.Globalization;

public static class LoadingScreenControl 
{
    public static string[] SlotSaveKeys => JGameControlDataSaveSlot.SlotSaveKeys;

    /*
     * performs loading one by one, for both normal game and archive loading
     */
    public static void ManualUpdate(MainGameControl mgc, LoadingScreenSetup.LoadingScreenRuntimeData loadingData)
    {
        loadingData.loadingProgress += 5;
        loadingData.TextLayout.SetTextRaw(0, $"Loading: {loadingData.loadingProgress}%");
        LoadingScreenSetup.LoadingScreenRuntimeData.State previousState = loadingData.state;
        switch (previousState)
        {
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.START:
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.MODEL:
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.LOADING_PERSISTENCE:
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.CANVAS_TAB_MENU:
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.CANVAS_MAIN_RUNTIME_UNITS:
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.CANVAS_MISC:
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.OVER:
                NormalGameLoading(mgc, loadingData);
                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_LOADING_PERSISTENCE:
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_MODEL:
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_CANVAS_TAB_MENU:
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_CANVAS_MAIN_RUNTIME_UNITS:
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_CANVAS_MISC:
                ArchiveLoading(mgc, loadingData);
                break;
            default:
                break;
        }
    }

    private static void ArchiveLoading(MainGameControl mgc, LoadingScreenSetup.LoadingScreenRuntimeData loadingData)
    {
        LoadingScreenSetup.LoadingScreenRuntimeData.State previousState = loadingData.state;
        switch (previousState)
        {
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_LOADING_PERSISTENCE:
                {
                    loadingData.ArchiveLoadPersistenceState = ArchiveScreenControlExecuter.LoadUpArchive(mgc, loadingData.ArchiveLoadPersistenceState);
                    if (loadingData.ArchiveLoadPersistenceState.Value.over)
                    {
                        loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_MODEL;
                    }
                }
                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_MODEL:
                bool jsonOver = ModelLoading(mgc, loadingData);
                if (jsonOver)
                {
                    loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_CANVAS_TAB_MENU;
                    ArcaniaArchiveModelExecuter.AfterModelLoadingOver(mgc.arcaniaModel);
                }
                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_CANVAS_TAB_MENU:
                MainGameControlSetupJLayout.SetupGameCanvasTabMenuInstantiation(mgc, true);
                loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_CANVAS_MAIN_RUNTIME_UNITS;
                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_CANVAS_MAIN_RUNTIME_UNITS:
                MainGameControlSetupJLayout.SetupGameCanvasMainRuntimeUnits(mgc, new MainGameControlSetupJLayout.SetupGameCanvasMainRuntimeUnitConfig 
                { 
                    ArchiveMode = true
                });
                // temp
                loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_CANVAS_MISC;
                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.ARCHIVE_CANVAS_MISC:
                loadingData.TextLayout.SetVisibleSelf(false);
                ControlSetupArchiveJLayout.SetupArchiveExclusiveElements(mgc);
                loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.OVER;
                break;
            default:
                break;
        }
    }

    private static void NormalGameLoading(MainGameControl mgc, LoadingScreenSetup.LoadingScreenRuntimeData loadingData)
    {
        LoadingScreenSetup.LoadingScreenRuntimeData.State previousState = loadingData.state;
        switch (previousState)
        {
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.START:
                MainGameControlSetupJLayout.SetupLocalizationSingleStep(mgc, out bool hasLocali);
                loadingData.hasLocalizationFile = hasLocali;
                loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.MODEL;
                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.MODEL:
                bool jsonOver = ModelLoading(mgc, loadingData);
                if (jsonOver)
                {
                    // next state is different if loading up the archive
                    loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.LOADING_PERSISTENCE;
                }

                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.LOADING_PERSISTENCE:

                mgc.RobustDeltaTime = new();
                LoadSlotAndCommons(mgc);
                // Let the model run once so you can finish up setup with the latest info on visibility
                mgc.arcaniaModel.ManualUpdate(0);
                loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.CANVAS_TAB_MENU;
                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.CANVAS_TAB_MENU:
                MainGameControlSetupJLayout.SetupGameCanvasTabMenuInstantiation(mgc,false);
                loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.CANVAS_MAIN_RUNTIME_UNITS;
                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.CANVAS_MAIN_RUNTIME_UNITS:
                MainGameControlSetupJLayout.SetupGameCanvasMainRuntimeUnits(mgc, default);
                loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.CANVAS_MISC;
                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.CANVAS_MISC:
                MainGameControlSetupJLayout.SetupGameCanvasMisc(mgc);
                loadingData.TextLayout.SetVisibleSelf(false);
                loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.OVER;
                break;

            case LoadingScreenSetup.LoadingScreenRuntimeData.State.OVER:
                // nothing happens here
                break;
            default:
                break;
        }
    }

    private static bool ModelLoading(MainGameControl mgc, LoadingScreenSetup.LoadingScreenRuntimeData loadingData)
    {
        // common normal game
        loadingData.ModelJsonState = JsonReader.ReadJsonStepByStep(mgc.ResourceJson, mgc.arcaniaModel.arcaniaUnits, loadingData.hasLocalizationFile, loadingData.ModelJsonState);
        bool jsonOver = loadingData.ModelJsonState.Value.readerState == JsonReader.JsonReaderState.JsonReaderStateMode.OVER;
        if (jsonOver)
        {
            // final model setup
            mgc.arcaniaModel.FinishedSettingUpUnits();
        }

        return jsonOver;
    }

    public static void LoadSlotAndCommons(MainGameControl mgc)
    {
        int slotNumber = 3;
        HeartGame heartGame = mgc.HeartGame;
        var slotData = SaveSlotExecution.LoadSlotModel(slotNumber, heartGame);
        mgc.JControlData.SaveSlots.ModelData = slotData;
        var currentSaveSlot = slotData.currentSlot;

        SaveSlotExecution.InitCurrentSlotIfNoSave(slotData, "nobody");

        var slot = currentSaveSlot;
        var key = JGameControlDataSaveSlot.SlotSaveKeys[slot];
        mgc.ArcaniaPersistence = new(heartGame, key);
        mgc.ArcaniaPersistence.Load(mgc.arcaniaModel.arcaniaUnits, mgc.arcaniaModel.Exploration);
        mgc.JControlData.SaveSlots.PlayTimeOfActiveSlot = heartGame.PlayTimeControl.Register("beggar_unit", slotData.CurrentSlotUnit.playTimeSeconds);
        heartGame.CommonDataLoad();

    }
}
