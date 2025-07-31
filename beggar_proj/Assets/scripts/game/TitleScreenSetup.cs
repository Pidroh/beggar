using HeartUnity;
using HeartUnity.View;
using JLayout;
using UnityEngine;

public class TitleScreenRuntimeData
{
    public JRTControlUnit StartGameJCU { get; internal set; }
}

public class TitleScreenSetup
{
    public static void Setup(MainGameControl mgc, TitleScreenRuntimeData titleScreenData)
    {
        var canvas = mgc.JLayoutRuntime.jLayCanvas;
        JLayCanvasChild jLayCanvasChild = canvas.children[0];
        jLayCanvasChild.SavePivot();
        var parentLayout = jLayCanvasChild.LayoutRuntimeUnit;

        {
            var fleeButtonLayout = JCanvasMaker.CreateLayout("exploration_simple_button", mgc.JLayoutRuntime);
            var lc = parentLayout.AddLayoutAsChild(fleeButtonLayout);
            lc.PositionModeOverride = new PositionMode[] { PositionMode.LEFT_ZERO, PositionMode.SIBLING_DISTANCE_REVERSE };
            fleeButtonLayout.ButtonChildren[0].Item1.SetTextRaw(0, Local.GetText("Start Game"));
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