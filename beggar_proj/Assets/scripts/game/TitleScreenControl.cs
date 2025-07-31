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
        // Update the layout runtime for button processing
        if (titleScreenData.LayoutRuntime != null)
        {
            JLayoutRuntimeExecuter.ManualUpdate(titleScreenData.LayoutRuntime);
        }
        
        // Check if Start Game button was clicked
        if (titleScreenData.StartGameButton != null && 
            titleScreenData.StartGameButton.ButtonChildren.Count > 0 &&
            titleScreenData.StartGameButton.IsButtonClicked(0))
        {
            HideTitleScreen(titleScreenData);
            return TitleScreenState.StartGame;
        }
        
        return TitleScreenState.Continue;
    }

    private static void HideTitleScreen(TitleScreenRuntimeData titleScreenData)
    {
        if (titleScreenData.TitleCanvas != null)
        {
            titleScreenData.TitleCanvas.canvasGO.SetActive(false);
        }
    }
}