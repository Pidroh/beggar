using HeartUnity;
using HeartUnity.View;
using JLayout;
using System.Collections.Generic;
using UnityEngine;
using static TitleScreenRuntimeData;

public class TitleScreenRuntimeData
{
    public List<(TitleButtons, JRTControlUnit)> TitleButtonsJCUs = new();

    public JLayoutRuntimeUnit TextLayout { get; internal set; }

    public enum TitleButtons 
    { 
        PLAY_GAME, 
        STEAM,
        SETTINGS
    }
}



public class TitleScreenSetup
{
    public static readonly TitleButtons[] ButtonsToMake = new TitleButtons[] { TitleButtons.PLAY_GAME, TitleButtons.STEAM, TitleButtons.SETTINGS };
    public static void Setup(MainGameControl mgc, TitleScreenRuntimeData titleScreenData)
    {
        var canvas = mgc.JLayoutRuntime.jLayCanvas;
        JLayCanvasChild jLayCanvasChild = canvas.children[0];
        // JLayCanvasChild jLayCanvasChild = canvas.Overlays[0];
        jLayCanvasChild.SavePivot();
        var parentLayout = jLayCanvasChild.LayoutRuntimeUnit;

        {
            var titleTexts = JCanvasMaker.CreateLayout("title_texts", mgc.JLayoutRuntime);
            var lc = parentLayout.AddLayoutAsChild(titleTexts);
            titleTexts.SetTextRaw(0, Local.GetText(mgc.ResourceJson.gameTitleText));
            titleTexts.SetTextRaw(1, Local.GetText(mgc.ResourceJson.gameSubTitleText) + 
                $"\n{mgc.HeartGame.config.majorVersion}.{mgc.HeartGame.config.versionNumber}.{mgc.HeartGame.config.patchVersion}"
                );
            titleScreenData.TextLayout = titleTexts;
        }
        foreach (var buttonT in ButtonsToMake)
        {
            var titleButton = JCanvasMaker.CreateLayout("centered_simple_button", mgc.JLayoutRuntime);
            var lc = parentLayout.AddLayoutAsChild(titleButton);
            
            string rawText = buttonT switch
            {
                TitleButtons.PLAY_GAME => Local.GetText("Play Game"),
                TitleButtons.STEAM => Local.GetText("Wishlist on Steam"),
                TitleButtons.SETTINGS => Local.GetText("Settings"),
                _ => "Unknown"
            };

            titleButton.ButtonChildren[0].Item1.SetTextRaw(0, rawText);
            titleButton.ButtonChildren[0].Item1.ImageChildren[1].UiUnit.ActiveSelf = false;
            JRTControlUnit jCU = new();
            jCU.MainLayout = titleButton;
            jCU.MainExecuteButton = new JButtonAccessor(titleButton, 0);
            
            titleScreenData.TitleButtonsJCUs.Add((buttonT, jCU));
        }
        //canvas.ShowOverlay();
        /*
        var endingTextLayout = JCanvasMaker.CreateLayout("ending_text", mgc.JLayoutRuntime);
        titleScreenData.TitleScreenLayout = endingTextLayout;
        parentLayout.AddLayoutAsChild(endingTextLayout);
        
        endingTextLayout.SetTextRaw(0, "Beggar's Journey");
        endingTextLayout.SetTextRaw(1, "");
        
        var buttonLayout = endingTextLayout.LayoutChildren[0].LayoutRU;
        buttonLayout.ButtonChildren[0].Item1.SetTextRaw(0, "Start Game");
        titleScreenData.StartGameButton = buttonLayout;*/
    }
}