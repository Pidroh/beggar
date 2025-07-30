using HeartUnity;
using HeartUnity.View;
using JLayout;
using UnityEngine;

public class TitleScreenRuntimeData
{
    public JLayCanvas TitleCanvas { get; set; }
    public JLayoutRuntimeUnit StartGameButton { get; set; }
    public JLayoutRuntimeUnit TitleScreenLayout { get; set; }
}

public class TitleScreenSetup
{
    public static void Setup(MainGameControl mgc, TitleScreenRuntimeData titleScreenData)
    {
        var runtime = new JLayoutRuntimeData();
        runtime.DefaultFont = mgc.Font;
        runtime.ImageSprites = mgc.ResourceJson.spritesForLayout;
        runtime.CurrentColorSchemeId = 0;
        
        LayoutDataMaster layoutMaster = new LayoutDataMaster();
        JsonInterpreter.ReadJson(mgc.ResourceJson.layoutJsonForTitle.text, layoutMaster);
        runtime.LayoutMaster = layoutMaster;
        
        var titleCanvas = JCanvasMaker.CreateCanvas(1, mgc.CanvasRequest, null, runtime);
        titleScreenData.TitleCanvas = titleCanvas;
        
        var parentLayout = titleCanvas.children[0].LayoutRuntimeUnit;
        
        var endingTextLayout = JCanvasMaker.CreateLayout("ending_text", runtime);
        titleScreenData.TitleScreenLayout = endingTextLayout;
        parentLayout.AddLayoutAsChild(endingTextLayout);
        
        endingTextLayout.SetTextRaw(0, "Beggar's Journey");
        endingTextLayout.SetTextRaw(1, "");
        
        var buttonLayout = endingTextLayout.LayoutChildren[0].LayoutRU;
        buttonLayout.ButtonChildren[0].Item1.SetTextRaw(0, "Start Game");
        titleScreenData.StartGameButton = buttonLayout;
        
        titleCanvas.ShowOverlay();
    }
}