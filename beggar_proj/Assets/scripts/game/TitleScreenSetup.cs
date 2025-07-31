using HeartUnity;
using HeartUnity.View;
using JLayout;
using UnityEngine;

public class TitleScreenRuntimeData
{
    public JLayoutRuntimeUnit StartGameButton { get; set; }
    public JLayoutRuntimeUnit TitleScreenLayout { get; set; }
    public JRTControlUnit StartGameJCU { get; internal set; }
}

public class TitleScreenSetup
{
    public static void Setup(MainGameControl mgc, TitleScreenRuntimeData titleScreenData)
    {
        var canvas = mgc.JLayoutRuntime.jLayCanvas;
        var parentLayout = canvas.children[0].LayoutRuntimeUnit;

        {
            var fleeButtonLayout = JCanvasMaker.CreateLayout("exploration_simple_button", mgc.JLayoutRuntime);
            var lc = parentLayout.AddLayoutAsChild(fleeButtonLayout);
            fleeButtonLayout.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("Flee"));
            JRTControlUnit jCU = new();
            jCU.MainLayout = fleeButtonLayout;
            jCU.MainExecuteButton = new JButtonAccessor(fleeButtonLayout, 0);
            fleeButtonLayout.ButtonChildren[0].Item1.ImageChildren[1].UiUnit.ActiveSelf = false;

            titleScreenData.StartGameJCU = jCU;
        }
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