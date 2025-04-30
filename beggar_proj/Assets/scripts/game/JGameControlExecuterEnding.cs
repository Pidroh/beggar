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
        
    }
}

public class JEndingGameData
{
    public RuntimeUnit[] runtimeUnits = new RuntimeUnit[JGameControlExecuterEnding.ENDING_COUNT] { null, null, null };
}