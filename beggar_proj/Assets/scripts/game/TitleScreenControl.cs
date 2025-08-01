using HeartUnity;

public enum TitleScreenState
{
    Continue,
    StartGame
}

public static class TitleScreenControl
{
    public static TitleScreenState ManualUpdate(MainGameControl mgc, TitleScreenRuntimeData titleScreenData)
    {
        mgc.JLayoutRuntime.jLayCanvas.children[0].ForceCenterX = true;
        foreach (var pair in titleScreenData.TitleButtonsJCUs)
        {
            if (pair.Item2.TaskClicked)
            {
                switch (pair.Item1)
                {
                    case TitleScreenRuntimeData.TitleButtons.PLAY_GAME:
                        mgc.JLayoutRuntime.jLayCanvas.children[0].ForceCenterX = false;
                        mgc.JLayoutRuntime.jLayCanvas.children[0].ApplySavedPivot();
                        // hide stuff
                        {
                            foreach (var pairHide in titleScreenData.TitleButtonsJCUs)
                            {
                                pairHide.Item2.MainLayout.SelfUIUnit.Active = false;
                            }
                            titleScreenData.TextLayout.SelfUIUnit.Active = false;
                        }
                        return TitleScreenState.StartGame;
                    case TitleScreenRuntimeData.TitleButtons.STEAM:
                        URLOpener.OpenSteamURL("https://store.steampowered.com/app/" + mgc.HeartGame.config.urls.steamPageAppId + "/?utm_source=insidegame&utm_medium=title_screen", mgc.HeartGame.config.urls.steamPageAppId);
                        break;
                    case TitleScreenRuntimeData.TitleButtons.SETTINGS:
                        mgc.GoToSettings();
                        break;
                    default:
                        break;
                }
            }
        }


        return TitleScreenState.Continue;
    }
}