using JLayout;
using UnityEngine;

public enum TitleScreenState
{
    Continue,
    StartGame
}

public static class TitleScreenControl
{
    public static TitleScreenState ManualUpdate(TitleScreenRuntimeData titleScreenData)
    {

        if (titleScreenData.StartGameJCU.TaskClicked) 
        {
            return TitleScreenState.StartGame;
        }
        // Check if Start Game button was clicked
        if (titleScreenData.StartGameButton != null && 
            titleScreenData.StartGameButton.ButtonChildren.Count > 0 &&
            titleScreenData.StartGameButton.IsButtonClicked(0))
        {
            return TitleScreenState.StartGame;
        }
        
        return TitleScreenState.Continue;
    }
}