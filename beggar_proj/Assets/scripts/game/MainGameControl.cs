using HeartUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainGameControl : MonoBehaviour
{

    public TextAsset ResourceJson;
    private DynamicCanvas dynamicCanvas;
    public List<TaskControlUnit> TaskControls = new();
    [SerializeField]
    public TMP_FontAsset Font;
    public Sprite ExpanderSprite;
    public CanvasMaker.CreateObjectRequest ButtonObjectRequest;

    // Start is called before the first frame update
    void Start()
    {
        ArcaniaUnits arcaniaDatas = new ArcaniaUnits();
        JsonReader.ReadJson(ResourceJson.text, arcaniaDatas);
        dynamicCanvas = CanvasMaker.CreateCanvas(1);
        foreach (var item in arcaniaDatas.datas[UnitType.TASK])
        {
            var layout = CanvasMaker.CreateLayout().SetFitHeight(true);
            var button = CanvasMaker.CreateButton(item.ConfigBasic.name, ButtonObjectRequest);
            var iconButton = CanvasMaker.CreateButtonWithIcon(ExpanderSprite);
            var bwe = new ButtonWithExpandable(button, iconButton);
            dynamicCanvas.children[0].AddLayoutAndParentIt(layout);
            layout.AddLayoutChildAndParentIt(bwe);
            button.SetTextRaw(item.ConfigBasic.name);
            var tcu = new TaskControlUnit();
            TaskControls.Add(tcu);
            tcu.bwe = bwe;
            
            var arrayOfChanges = item.ConfigTask.Cost;
            var rcgIndex = 0;
            if (arrayOfChanges != null) tcu.ChangeGroups[rcgIndex] = new TaskControlUnit.ResourceChangeGroup();
            foreach (var changeU in arrayOfChanges)
            {
                TripleTextView ttv = CanvasMaker.CreateTripleTextView(ButtonObjectRequest);
                layout.AddLayoutChildAndParentIt(ttv.LayoutChild);
                tcu.ChangeGroups[rcgIndex].tripleTextViews.Add(ttv);
            }
        }
    }

    public class TaskControlUnit {
        public ButtonWithExpandable bwe;
        public AutoList<ResourceChangeGroup> ChangeGroups = new();
        public ResourceChangeGroup CostGroup { get => ChangeGroups[0]; set => ChangeGroups[0] = value; }
        public ResourceChangeGroup ResultGroup { get => ChangeGroups[1]; set => ChangeGroups[1] = value; }
        public ResourceChangeGroup RunGroup { get => ChangeGroups[2]; set => ChangeGroups[2] = value; }
        public ResourceChangeGroup EffectGroup { get => ChangeGroups[3]; set => ChangeGroups[3] = value; }

        public class ResourceChangeGroup 
        {
            public AutoList<TripleTextView> tripleTextViews = new();
        }
        public void ManualUpdate() {
            bwe.ManualUpdate();
            foreach (var item in ChangeGroups)
            {
                if (item == null) continue;
                foreach (var ttv in item.tripleTextViews)
                {
                    ttv.ManualUpdate();
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        dynamicCanvas.ManualUpdate();
        foreach (var bwe in TaskControls)
        {
            bwe.ManualUpdate();
        }
    }
}
