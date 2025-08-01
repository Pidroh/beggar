using JLayout;

public class LoadingScreenSetup 
{
    public static LoadingScreenRuntimeData Setup(MainGameControl mgc)
    {
        LoadingScreenRuntimeData loadingData = new();
        var canvas = mgc.JLayoutRuntime.jLayCanvas;
        JLayCanvasChild jLayCanvasChild = canvas.children[0];
        // JLayCanvasChild jLayCanvasChild = canvas.Overlays[0];
        jLayCanvasChild.SavePivot();
        var parentLayout = jLayCanvasChild.LayoutRuntimeUnit;

        {
            var titleTexts = JCanvasMaker.CreateLayout("title_texts", mgc.JLayoutRuntime);
            var lc = parentLayout.AddLayoutAsChild(titleTexts);
            titleTexts.SetTextRaw(0, "Loading: 0%");
            titleTexts.SetTextRaw(1, "");
            loadingData.TextLayout = titleTexts;
        }
        return loadingData;
    }
    public class LoadingScreenRuntimeData
    {
        public JLayoutRuntimeUnit TextLayout { get; internal set; }
        public bool hasLocalizationFile { get; internal set; }
        public JsonReader.JsonReaderState? ModelJsonState { get; internal set; }

        public State state = State.START;
        public int loadingProgress;
        public enum State 
        { 
            START,
            MODEL, 
            LOADING_PERSISTENCE,
            CANVAS,
            OVER
        }
    }
}
