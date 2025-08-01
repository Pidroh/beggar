using JLayout;
using UnityEngine;
using UnityEngine.Rendering;

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
                        return TitleScreenState.StartGame;
                        break;
                    case TitleScreenRuntimeData.TitleButtons.STEAM:
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