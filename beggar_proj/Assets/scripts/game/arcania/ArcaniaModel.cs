using arcania;
using HeartEngineCore;
using System.Collections.Generic;

public class ConfigResource
{
    public bool Stressor;

    public string HeuristicIntegration { get; set; }
}

public class ConfigHint
{
    public IDPointer hintTargetPointer { get; set; }
}

public class ConfigEncounter
{
    public int Length { get; set; }
}

public class ConfigLocation
{
    public int Length { get; set; }
}
public class ConfigSkill
{
    public int LearningDifficultyLevel;
}

public class ConfigHouse
{
    public int AvailableSpace;
}

public class ConfigFurniture
{
    public int SpaceConsumed;
}

public class ArcaniaSpeedParameters 
{
    public float globalMultiplier = 1f;
    public float skillStudyingMultiplier = 1f;
    public float explorationMultiplier = 1f;
}

public class ArcaniaSpeedIntegrationData 
{
    
    public RuntimeUnit[] multiplierRuntimeUnits = new RuntimeUnit[EnumHelper<MultiplierTypes>.GetAllValues().Count];
    public static string[] runtimeUnitIds = new string[] 
    {
        "IN_GAME_SPEED",
        "IN_GAME_SPEED_EXPLORATION",
        "IN_GAME_SPEED_SKILL"
    };
    public enum MultiplierTypes 
    {
        GLOBAL,
        EXPLORATION,
        SKILL
    }
}

public class ArcaniaModelMiscData 
{
    public bool hasAccessToArchive;
}

public class ArcaniaModel
{
    public List<LogUnit> LogUnits = new();
    public ArcaniaUnits arcaniaUnits = new ArcaniaUnits();
    public ArcaniaModelMiscData modelMiscData = new();
    public ArcaniaModelExploration Exploration;
    public ArcaniaModelActionRunner Runner;
    public ArcaniaModelHousing Housing;
    float _oneSecondCounter;
    public DialogModel Dialog = new();
    public ArcaniaSpeedParameters speedParameters = new();
    public ArcaniaSpeedIntegrationData speedIntegrationData = new();
    public ArcaniaArchiveModelData archiveDataPreviouslyCalculated;
    public ArcaniaModelNotificationData notificationData = new();

    public bool SaveSlotOnlyMode { get; set; }

    public ArcaniaModel()
    {
        Runner = new(this);
        Housing = new(this);
        Exploration = new(this);
        arcaniaUnits.datas[UnitType.DOT] = new();
    }

    public void ApplyResourceChanges(RuntimeUnit parent, ResourceChangeType changeType)
    {
        var changes = parent.ConfigTask.GetResourceChangeList(changeType);
        foreach (var mod in parent.ModsSelfAsIntermediary)
        {
            if (mod.ModType != ModType.ResourceChangeChanger) continue;
            if (mod.ResourceChangeType != changeType) continue;
            if (mod.Source.Value == 0) continue;
            float valueChange = mod.Source.Value * mod.Value;
            foreach (var item in mod.Target.RuntimeUnits)
            {
                item.ChangeValue(valueChange);
            }
            ArcaniaModelNotificationExecuter.Report(this, parent, mod.Target, valueChange, changeType);
            //mod.Target.
        }
        foreach (var c in changes)
        {
            float? vChanged = null;
            
            foreach (var ru in c.IdPointer)
            {
                switch (c.ModificationType)
                {
                    case ResourceChange.ResourceChangeModificationType.NormalChange:
                        vChanged = ru.ChangeValueByResourceChange(parent, c.valueChange, changeType);
                        break;
                    case ResourceChange.ResourceChangeModificationType.XpChange:
                        // Mods not supported for now
                        int v = (int)c.valueChange.getValue(RandomHG.Range(0f, 1f));
                        vChanged = v;
                        ru.Skill.xp += v;
                        break;
                    default:
                        break;
                }

            }

            if (vChanged.HasValue) 
            {
                ArcaniaModelNotificationExecuter.Report(this, parent, c.IdPointer, vChanged.Value, changeType);
            }
        }
    }

    public void ChangeValue(RuntimeUnit runtimeUnit, int valueChange)
    {
        runtimeUnit.ChangeValue(valueChange);
    }

    public void ManualUpdate(float dt)
    {
        dt = this.speedParameters.globalMultiplier * dt;
        Dialog.ManualUpdate();
        Runner.ManualUpdate(dt);
        Exploration.ManualUpdate(dt);
        ArcaniaModelDotCode.Update(this, dt);
        _oneSecondCounter += dt;
        var applyRateNumber = 0;
        while (_oneSecondCounter > 1f)
        {
            _oneSecondCounter -= 1f;
            applyRateNumber++;
        }
        foreach (var pair in arcaniaUnits.datas)
        {

            foreach (var item in pair.Value)
            {
                bool updateRequireStatusResult = item.UpdateRequireStatus();
                if (pair.Key == UnitType.ENCOUNTER) continue;
                if (pair.Key == UnitType.TAB) continue;
                if (pair.Key == UnitType.TAG) continue;

                if (item.Visible) 
                {
                    if (item.UnlockNotification == UnlockNotification.Locked) 
                    {
                        item.UnlockNotification = UnlockNotification.UnlockedAndUnseen;
                        
                    }
                }

                if (updateRequireStatusResult)
                {
                    var unlockLog = pair.Key != UnitType.DOT;
                    if (unlockLog)
                    {
                        LogUnits.Add(new LogUnit()
                        {
                            logType = LogUnit.LogType.UNIT_UNLOCKED,
                            Unit = item
                        });
                    }

                }
                for (int i = 0; i < applyRateNumber; i++)
                {
                    item.ApplyRate();
                }
            }
        }
    }

    public class ArcaniaModelSubmodule
    {
        public ArcaniaModel _model;

        public ArcaniaModelSubmodule(ArcaniaModel arcaniaModel)
        {
            _model = arcaniaModel;
        }
    }


    public bool CanAfford(List<ResourceChange> changes)
    {
        foreach (var rc in changes)
        {
            if (rc.valueChange.BothEqual(0f)) continue;
            rc.IdPointer.CheckValidity();
            if (rc.IdPointer.RuntimeUnit.CanFullyAcceptChange(rc.valueChange)) continue;
            return false;
        }
        return true;
    }




    public bool DoChangesMakeADifference(RuntimeUnit ru, ResourceChangeType changeType)
    {
        using var _1 = DictionaryPool<IDPointer, FloatRangePure>.Get(out var dict);
        // Account for total normal effect changes
        foreach (var rc in ru.ConfigTask.GetResourceChangeList(changeType))
        {
            if (rc.valueChange.BothEqual(0f)) continue;
            if (dict.TryGetValue(rc.IdPointer, out var amt))
            {
                dict[rc.IdPointer] = amt + rc.valueChange;
            }
            else
            {
                dict[rc.IdPointer] = rc.valueChange;
            }
        }
        // Account for mods where parent is the intermediary
        foreach (var item in ru.ModsSelfAsIntermediary)
        {
            if (item.ResourceChangeType != changeType) continue;
            if (item.Source.Value == 0) continue;
            var totalValue = item.Value * item.Source.Value;
            if (totalValue == 0) continue;
            if (dict.TryGetValue(item.Target, out var amt))
            {
                dict[item.Target] = amt + totalValue;
            }
            else
            {
                dict[item.Target] = new FloatRangePure(totalValue, totalValue);
            }
        }

        // do the final calculation to see if it is meaningful
        foreach (var item in dict)
        {
            var IdPointer = item.Key;
            var valueChange = item.Value;
            if (valueChange.BothEqual(0f)) continue;
            if (valueChange.BiggerThan(0f) && IdPointer.IsAllMaxed()) continue;
            if (valueChange.SmallerThan(0f) && IdPointer.IsAllZero()) continue;
            // only 1 thing needs to be meaningful
            return true;

        }
        return false;
    }

    private RuntimeUnit FindRuntimeUnitInternal(UnitType type, string v)
    {
        if (!arcaniaUnits.datas.TryGetValue(type, out var units)) return null;
        foreach (var item in units)
        {
            if (item.ConfigBasic.Id == v) return item;
        }
        return null;
    }

    public RuntimeUnit FindRuntimeUnit(string id)
    {
        var types = EnumHelper<UnitType>.GetAllValues();
        foreach (var t in types)
        {
            if (!arcaniaUnits.datas.ContainsKey(t)) continue;
            var ru = FindRuntimeUnitInternal(t, id);
            if (ru == null) continue;
            return ru;
        }
        Logger.Log($"Runtime unit of ID |{id}| NOT FOUND");
        return null;

    }

    public RuntimeUnit FindRuntimeUnit(UnitType type, string v)
    {
        var ru = FindRuntimeUnitInternal(type, v);
        if (ru != null) return ru;
        Logger.Log($"Runtime unit of type |{type}| and ID |{v}| NOT FOUND");
        return null;
    }

    public void FinishedSettingUpUnits()
    {
        Exploration.FinishedSettingUpUnits();
    }

    public void EnforceHeuristicIntegration()
    {
        if (archiveDataPreviouslyCalculated != null)
        {
            foreach (var item in arcaniaUnits.UnitsIntegratedWithHeuristic)
            {
                ArcaniaArchiveModelData.ArchiveEuristics? euristic = null;
                if (item.ConfigResource.HeuristicIntegration == "TOTAL_ITEMS")
                {
                    euristic = ArcaniaArchiveModelData.ArchiveEuristics.Total;
                }
                if (!euristic.HasValue) continue;
                foreach (var eurD in archiveDataPreviouslyCalculated.euristicDatas)
                {
                    if (eurD.EuristicType != euristic) continue;
                    item.SetValue(eurD.current);
                    break;
                }
            }
        }
    }

    public class DialogModel
    {
        public DialogRuntime ActiveDialog;
        public DialogState dialogState;
        public int? pickedOption;

        public bool ShouldShow => dialogState == DialogState.ACTIVE;

        public void ShowDialog(DialogRuntime dialogRuntime)
        {
            ActiveDialog = dialogRuntime;
            dialogState = DialogModel.DialogState.ACTIVE;
            pickedOption = null;
        }

        public void ManualUpdate()
        {
            if (pickedOption.HasValue)
            {
                switch (dialogState)
                {
                    case DialogState.INACTIVE:
                        break;
                    case DialogState.ACTIVE:
                        dialogState = DialogState.RESULT_HAPPENED_THIS_FRAME;
                        break;
                    case DialogState.RESULT_HAPPENED_THIS_FRAME:
                        pickedOption = null;
                        dialogState = DialogState.INACTIVE;
                        break;
                    default:
                        break;
                }
            }
        }

        public void DialogComplete(int pickedOption)
        {
            // only goes to result complete on next frame
            this.pickedOption = pickedOption;
        }

        public bool HasResult(out int option)
        {
            option = pickedOption.HasValue ? pickedOption.Value : -1;
            return this.dialogState == DialogState.RESULT_HAPPENED_THIS_FRAME;
        }

        public enum DialogState
        {
            INACTIVE,
            ACTIVE,
            RESULT_HAPPENED_THIS_FRAME,
        }
    }


}


public class ConfigBasic
{
    public string Id;
    public string Desc;
    public int Max;
    public string name;

    public ConditionalExpression Require { get; set; }
    public List<IDPointer> Tags { get; } = new();
    public UnitType UnitType { get; set; }
    public string SpriteKey { get; set; }
    public bool AboveMax { get; set; }
    public bool HasDesc => !string.IsNullOrWhiteSpace(Desc);
}

public enum UnitType
{
    RESOURCE, TASK, HOUSE, CLASS, SKILL, FURNITURE, TAB, DIALOG, LOCATION, ENCOUNTER,
    DOT, HINT, TAG
}

public static class ModReplaceKeys
{
    public const string MAX = "$MAX$";
    public const string RATE = "$RATE$";
    public const string SPEED = "$SPEED$";
    //public const string MAXSPACE = "$MAXSPACE$";
    public const string SPACEOCCUPIED = "$SPACEOCCUPIED$";
    public const string SUCCESSRATE = "$SUCCESSRATE$";
}

public class ModRuntime
{
    public ModType ModType;
    public float Value;
    public RuntimeUnit Source;
    public IDPointer Intermediary;
    public IDPointer Target;

    public ResourceChangeType? ResourceChangeType { get; set; }
    public string SourceJsonKey { get; set; }
    public string HumanText { get; set; }
    public string HumanTextIntermediary { get; set; }
    public string HumanTextTarget { get; set; }
}

public class DotConfig
{
    public int Duration { get; set; }
    public bool Toggle { get; set; }
}

public class ResourceChange
{
    public IDPointer IdPointer;
    public FloatRangePure valueChange;
    public ResourceChangeModificationType ModificationType = ResourceChangeModificationType.NormalChange;
    public enum ResourceChangeModificationType
    {
        NormalChange,
        XpChange
    }
}



public class DialogRuntime
{
    public string Id;
    public string Title;
    public string Content;
    public List<IDPointer> TagPointers = new();
}

public class TagRuntime
{
    private string _tagName;
    public string tagName
    {
        get => RuntimeUnit?.Name ?? _tagName;
        set
        {
            _tagName = value;
        }
    }
    public List<RuntimeUnit> UnitsWithTag = new();

    public List<DialogRuntime> Dialogs = new();

    public TagRuntime(string tagName)
    {
        this.tagName = tagName;
    }

    public RuntimeUnit RuntimeUnit { get; set; }
}
