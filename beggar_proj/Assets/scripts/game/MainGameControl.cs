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
            tcu.Data = item;
            
            
            for (int i = 0; i < 4; i++)
            {
                var arrayOfChanges = item.ConfigTask.GetResourceChangeList(i);
                var rcgIndex = i;
                if (arrayOfChanges != null) tcu.ChangeGroups[rcgIndex] = new TaskControlUnit.ResourceChangeGroup();
                foreach (var changeU in arrayOfChanges)
                {
                    TripleTextView ttv = CanvasMaker.CreateTripleTextView(ButtonObjectRequest);
                    layout.AddLayoutChildAndParentIt(ttv.LayoutChild);
                    tcu.ChangeGroups[rcgIndex].tripleTextViews.Add(ttv);
                }
            }
            
        }
    }

    public class TaskControlUnit {
        public ButtonWithExpandable bwe;
        public AutoList<ResourceChangeGroup> ChangeGroups = new();
        internal RuntimeUnit Data;

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
            for (int i = 0; i < ChangeGroups.Count; i++)
            {
                ResourceChangeGroup item = ChangeGroups[i];
                var resourceChanges = Data.ConfigTask.GetResourceChangeList(i);
                if (item == null) continue;

                for (int ttvIndex = 0; ttvIndex < item.tripleTextViews.Count; ttvIndex++)
                {
                    TripleTextView ttv = item.tripleTextViews[ttvIndex];
                    var rc = resourceChanges[ttvIndex];

                    RuntimeUnit ru = rc.IdPointer.RuntimeUnit;
                    ttv.MainText.SetTextRaw(ru.Name);
                    ttv.SecondaryText.SetTextRaw($"{rc.valueChange}");
                    ttv.TertiaryText.SetTextRaw($"({ru.Value} / {ru.Max})");
                    ttv.ManualUpdate();
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        dynamicCanvas.ManualUpdate();
        foreach (var tcu in TaskControls)
        {
            tcu.ManualUpdate();
        }
    }
}
