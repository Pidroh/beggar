using JLayout;
using UnityEngine;

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
        if (titleScreenData.StartGameJCU.TaskClicked) 
        {
            mgc.JLayoutRuntime.jLayCanvas.children[0].ForceCenterX = false;
            mgc.JLayoutRuntime.jLayCanvas.children[0].ApplySavedPivot();
            return TitleScreenState.StartGame;
        }
        
        return TitleScreenState.Continue;
    }
}