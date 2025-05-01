using System;



public static class JGameControlExecuterEnding
{
    public const int ENDING_COUNT = 3;
    public static string[] endingUnitIds = new string[ENDING_COUNT] { "ponderexistence", "ponderhappiness", "ponderreligion" };
    public static string[] endingPrefix = new string[ENDING_COUNT] { "You have become one with existence", "You are seeking happiness with your cats", "You are closer to the Divine" };
    public static string[] endingMessageSnippet = new string[ENDING_COUNT] { "I'm the beggar's journey", "The beggar's journey is the cat", "The beggar's journey is in the scriptures" };
    public static string endingMessage = "GAME CLEARED \n$PART1$. \n At least until more content is added. \n\n Let me know you finished the game by sending me: \"$PART2$\".\n\n\n You can comment on the Reddit post, email, the Discord channel, etc";
    
    
    internal static void ManualUpdate(MainGameControl mgc, JGameControlDataHolder controlData, float dt)
    {
        if (controlData.EndingData.SettingsButton.Item2.UiUnit.Clicked) 
        {
            mgc.GoToSettings();
        }
        TryShowEnding(mgc, controlData);
    }

    private static void TryShowEnding(MainGameControl mainGameControl, JGameControlDataHolder controlData)
    {
        var dynamicCanvas = controlData.LayoutRuntime.jLayCanvas;
        if (dynamicCanvas == null) return;
        if (dynamicCanvas.OverlayVisible) return;
        for (int i = 0; i < controlData.EndingData.runtimeUnits.Length; i++)
        {
            RuntimeUnit ru = controlData.EndingData.runtimeUnits[i];
            // older versions might not have all the endings
            if (ru == null) continue;
            if (ru.Value <= 0) continue;
            dynamicCanvas.ShowOverlay();

            var message = endingMessage;
            message = message.Replace("$PART1$", endingPrefix[i]).Replace("$PART2$", endingMessageSnippet[i]);
            controlData.EndingLayout.LayoutRU.SetTextRaw(0, "Intermediary Ending");
            controlData.EndingLayout.LayoutRU.SetTextRaw(1, message);
            controlData.EndingLayout.LayoutRU.SetVisibleSelf(true);
            return;
        }
    }
}

public class JEndingGameData
{
    public RuntimeUnit[] runtimeUnits = new RuntimeUnit[JGameControlExecuterEnding.ENDING_COUNT] { null, null, null };

    public (JLayout.JLayoutRuntimeUnit, JLayout.JLayoutChild) SettingsButton { get; internal set; }
}