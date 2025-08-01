using HeartUnity;

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

                    // TODO(break these down a bit further apart)
                    mgc.RobustDeltaTime = new();
                    mgc.ArcaniaPersistence = new(mgc.HeartGame);
                    mgc.ArcaniaPersistence.Load(mgc.arcaniaModel.arcaniaUnits, mgc.arcaniaModel.Exploration);
                    mgc.HeartGame.CommonDataLoad();
                    // Let the model run once so you can finish up setup with the latest info on visibility
                    mgc.arcaniaModel.ManualUpdate(0);
                    MainGameControlSetupJLayout.SetupGameCanvas(mgc);

                    loadingData.TextLayout.SetVisibleSelf(false);
                    loadingData.state = LoadingScreenSetup.LoadingScreenRuntimeData.State.OVER;
                }
                break;
            case LoadingScreenSetup.LoadingScreenRuntimeData.State.OVER:
                // nothing happens here
                break;
            default:
                break;
        }
    }
    
}
