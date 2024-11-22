using HeartUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaniaModel;

public class ArcaniaModelExploration : ArcaniaModelSubmodule
{
    public float encounterProgress;
    public int locationProgress;
    public RuntimeUnit ActiveEncounter;
    public List<RuntimeUnit> Stressors = new();
    public ArcaniaModelExploration(ArcaniaModel arcaniaModel) : base(arcaniaModel)
    {
    }

    public void ManualUpdate()
    {
        RuntimeUnit runningLocation = null;
        foreach (var task in _model.Runner.RunningTasks)
        {
            if (task.ConfigBasic.UnitType != UnitType.LOCATION) continue;
            runningLocation = task;
            break;
        }
        if (runningLocation == null) return;
        var activeLocation = runningLocation;
        #region Spawn encounter
        if (ActiveEncounter == null) {

            var ele = activeLocation.Location.Encounters.RandomElement();
            ActiveEncounter = ele.RuntimeUnit;
            encounterProgress = 0f;
        }
        #endregion
        encounterProgress += Time.deltaTime;
        if (encounterProgress >= ActiveEncounter.ConfigEncounter.Length)
        {
            #region encounter won
            _model.ApplyResourceChanges(ActiveEncounter, ResourceChangeType.RESULT);
            ActiveEncounter = null;
            locationProgress++;
            #endregion
        }
        else 
        {
            #region encounter progress
            _model.ApplyResourceChanges(ActiveEncounter, ResourceChangeType.EFFECT);
            foreach (var str in Stressors)
            {
                if (str.IsMaxed) 
                {
                    _model.Runner.InterruptTask(activeLocation);
                    return;
                }
            }
            #endregion 
        }
        if (locationProgress >= activeLocation.Location.configLocation.Length) 
        {
            locationProgress = 0;
            _model.Runner.CompleteTask(activeLocation);
        }
    }

    internal void FinishedSettingUpUnits()
    {
        foreach (var item in _model.arcaniaUnits.datas[UnitType.RESOURCE])
        {
            if (item.ConfigResource == null) continue;
            if ((item.ConfigResource.Stressor))
            {
                Stressors.Add(item);
            }
        }
        
    }
}
