using HeartUnity;
using System.Globalization;

public static class LoadingScreenControl 
{
    /*
     * MainGameControlSetupJLayout.SetupModelDataAllAtOnce(this);
        RobustDeltaTime = new();
        ArcaniaPersistence = new(HeartGame);
        ArcaniaPersistence.Load(arcaniaModel.arcaniaUnits, arcaniaModel.Exploration);
        HeartGame.CommonDataLoad();
        // Let the model run once so you can finish up setup with the latest info on visibility
        arcaniaModel.ManualUpdate(0);
        MainGameControlSetupJLayout.SetupGameCanvas(this);
     */
    public static void ManualUpdate(MainGameControl mgc, LoadingScreenSetup.LoadingScreenRuntimeData loadingData) 
    {
        loadingData.loadingProgress += 5;
        loadingData.TextLayout.SetTextRaw(0, $"Loading: {loadingData.loadingProgress}%");
        LoadingScreenSetup.LoadingScreenRuntimeData.State previousState = loadingData.state;
        switch (previousState)
        {
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.START:
                MainGameControlSetupJLayout.SetupLocalizationSingleStep(mgc, out bool hasLocali);
                loadingData.hasLocalizationFile = hasLocali;
                loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.MODEL;
                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.MODEL:
                loadingData.ModelJsonState = JsonReader.ReadJsonStepByStep(mgc.ResourceJson, mgc.arcaniaModel.arcaniaUnits, loadingData.hasLocalizationFile, loadingData.ModelJsonState);
                if (loadingData.ModelJsonState.Value.readerState == JsonReader.JsonReaderState.JsonReaderStateMode.OVER) 
                {
                    // final model setup
                    mgc.arcaniaModel.FinishedSettingUpUnits();
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
                MainGameControlSetupJLayout.SetupGameCanvasTabMenuInstantiation(mgc);
                loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.CANVAS_MAIN_RUNTIME_UNITS;
                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.CANVAS_MAIN_RUNTIME_UNITS:
                MainGameControlSetupJLayout.SetupGameCanvasMainRuntimeUnits(mgc);
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

    public static void LoadSlotAndCommons(MainGameControl mgc)
    {
        int slotNumber = 4;
        HeartGame heartGame = mgc.HeartGame;
        SaveSlotModelData.SaveSlotPersistenceData slotData = SaveSlotExecution.LoadSlotModel(slotNumber, heartGame);
        var currentSaveSlot = slotData.currentSaveSlot;
        var key = "maindata";
        if (currentSaveSlot > 0)
        {
            key += $"_{currentSaveSlot}";
        }
        mgc.ArcaniaPersistence = new(heartGame, key);
        mgc.ArcaniaPersistence.Load(mgc.arcaniaModel.arcaniaUnits, mgc.arcaniaModel.Exploration);
        heartGame.CommonDataLoad();
    }

    
}
