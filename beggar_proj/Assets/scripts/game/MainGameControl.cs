using HeartUnity.View;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainGameControl : MonoBehaviour
{

    public TextAsset ResourceJson;
    public DynamicCanvas dynamicCanvas;
    public List<TabControlUnit> TabControlUnits = new();

    [SerializeField]
    public TMP_FontAsset Font;
    public Sprite ExpanderSprite;
    public CanvasMaker.CreateObjectRequest ButtonObjectRequest;
    public CanvasMaker.CreateButtonRequest ButtonRequest;
    public CanvasMaker.CreateCanvasRequest CanvasRequest;
    public CanvasMaker.CreateGaugeRequest SkillXPGaugeRequest;
    public ArcaniaModel arcaniaModel = new();

    public Color MainTextColor;

    public EngineView EngineView { get; internal set; }
    public float TimeMultiplier { get; private set; } = 1;
    public RuntimeUnit EndGameRuntimeUnit { get; internal set; }


    // Start is called before the first frame update
    void Start()
    {
        MainGameControlSetup.Setup(this);
    }

    // Update is called once per frame
    void Update()
    {
        EngineView.ManualUpdate();
        if (DebugMenuManager.CheckCommand("speed", out int v))
        {
            TimeMultiplier = v;
        }
        if (DebugMenuManager.CheckCommand("speed"))
        {
            TimeMultiplier = 1;
        }
        // Show end game
        if (EndGameRuntimeUnit.Value > 0)
        {

        }

        arcaniaModel.ManualUpdate(Time.deltaTime * TimeMultiplier);
        dynamicCanvas.ManualUpdate();
        // hide lower menu if all the tabs are visible
        dynamicCanvas.LowerMenus[0].SelfChild.Visible = dynamicCanvas.CalculateNumberOfVisibleHorizontalChildren() < arcaniaModel.arcaniaUnits.datas[UnitType.TAB].Count;


        for (int tabIndex = 0; tabIndex < TabControlUnits.Count; tabIndex++)
        {
            TabControlUnit tabControl = TabControlUnits[tabIndex];


            foreach (var sep in tabControl.Separators)
            {
                sep.Visible = false;
            }

            if (tabControl.SelectionButton.Button.Clicked)
            {
                dynamicCanvas.ShowChild(tabIndex);
            }
            if (!dynamicCanvas.children[tabIndex].SelfChild.Visible) continue;
            if (tabControl.TabData.Tab.ContainsLogs)
            {
                while (tabControl.LogControlUnits.Count < arcaniaModel.LogUnits.Count)
                {
                    MainGameControlSetup.CreateLogControlUnit(mgc: this, tabControl: tabControl, lp: dynamicCanvas.children[tabIndex], logUnit: arcaniaModel.LogUnits[tabControl.LogControlUnits.Count]);
                }
            }
            var UnitGroupControls = tabControl.UnitGroupControls;


            foreach (var pair in UnitGroupControls)
            {
                foreach (var tcu in pair.Value)
                {
                    var data = tcu.Data;
                    tcu.ManualUpdate();
                    bool visible = data.Visible;
                    tcu.bwe?.LayoutChild.RectTransform.parent.gameObject.SetActive(visible);
                    tcu.lwe?.LayoutChild.RectTransform.parent.gameObject.SetActive(visible);
                    if (!visible) continue;
                    if (tcu.ParentTabSeparator != null) tcu.ParentTabSeparator.Visible = true;
                    var modUnit = tcu.ModsUnit;
                    FeedMods(data, modUnit);

                    switch (pair.Key)
                    {

                        case UnitType.SKILL:
                            {

                                tcu.bwe.MainButton.ButtonEnabled = data.Skill.Acquired ? arcaniaModel.Runner.CanStudySkill(data) : arcaniaModel.Runner.CanAcquireSkill(data);
                                if (tcu.TaskClicked)
                                {
                                    if (data.Skill.Acquired) arcaniaModel.Runner.StudySkill(data);
                                    else arcaniaModel.Runner.AcquireSkill(data);

                                }

                            }
                            break;
                        case UnitType.HOUSE:
                            tcu.bwe.MainButton.Image.color = !arcaniaModel.Housing.IsLivingInHouse(data) ? ButtonRequest.MainBody.NormalColor : ButtonRequest.MainBody.SelectedColor;
                            tcu.bwe.MainButton.ButtonEnabled = arcaniaModel.Housing.CanChangeHouse(data);

                            if (tcu.TaskClicked)
                            {
                                if (!arcaniaModel.Housing.IsLivingInHouse(data)) arcaniaModel.Housing.ChangeHouse(data);
                            }

                            break;
                        case UnitType.FURNITURE:
                            {
                                tcu.ButtonAdd.Button.ButtonEnabled = arcaniaModel.Housing.CanAcquireFurniture(data);
                                tcu.ButtonRemove.Button.ButtonEnabled = arcaniaModel.Housing.CanRemoveFurniture(data);

                                if (tcu.ButtonAdd.Button.Clicked)
                                {
                                    arcaniaModel.Housing.AcquireFurniture(data);
                                }
                                if (tcu.ButtonRemove.Button.Clicked)
                                {
                                    arcaniaModel.Housing.RemoveFurniture(data);
                                }

                            }
                            break;
                        case UnitType.RESOURCE:
                            break;
                        case UnitType.TASK:

                        case UnitType.CLASS:
                            {
                                tcu.bwe.MainButton.ButtonEnabled = arcaniaModel.Runner.CanStartAction(data);

                                if (tcu.TaskClicked)
                                {
                                    arcaniaModel.Runner.StartAction(data);
                                }

                            }
                            break;
                        default:
                            break;
                    }
                }
            }

        }

        static void FeedMods(RuntimeUnit data, ModsControlUnit modUnit)
        {
            for (int i = 0; i < data.ModsOwned.Count; i++)
            {
                ModRuntime md = data.ModsOwned[i];

                var ttv = modUnit.ModTTVs[i];
                ttv.LayoutChild.Visible = md.ModType != ModType.Lock && ttv.LayoutChild.Visible;

                ttv.MainText.rawText = md.SourceJsonKey;
                ttv.SecondaryText.rawText = $"{md.Value}";
                ttv.TertiaryText.rawText = string.Empty;
                ttv.ManualUpdate();
            }
        }
    }
}
