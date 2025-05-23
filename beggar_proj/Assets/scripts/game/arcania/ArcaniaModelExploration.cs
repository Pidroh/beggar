﻿using HeartUnity;
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

    public RuntimeUnit LastActiveLocation { get; private set; }
    public bool IsExplorationActive => ActiveEncounter != null;

    public float ExplorationRatio => ((float)locationProgress) / LastActiveLocation.Location.configLocation.Length;
    public float EncounterRatio => ActiveEncounter == null ? 0f : ((float)encounterProgress) / ActiveEncounter.ConfigEncounter.Length;

    public void ManualUpdate(float dt)
    {
        RuntimeUnit runningLocation = null;
        foreach (var task in _model.Runner.RunningTasks)
        {
            if (task.ConfigBasic.UnitType != UnitType.LOCATION) continue;
            runningLocation = task;
            break;
        }
        if (runningLocation == null) ActiveEncounter = null;
        if (runningLocation == null) return;
        var activeLocation = runningLocation;
        if (LastActiveLocation != activeLocation)
        {
            locationProgress = 0;
            LastActiveLocation = activeLocation;
        }

        EnsureEncounter(activeLocation);
        encounterProgress += dt;
        if (encounterProgress >= ActiveEncounter.ConfigEncounter.Length)
        {
            #region encounter won
            _model.ApplyResourceChanges(ActiveEncounter, ResourceChangeType.RESULT);
            // RESULT_ONCE should not be applied here because currently the encounter itself doesn't have a value
            ActiveEncounter = null;
            locationProgress++;
            #endregion
        }
        else
        {
            #region encounter progress

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
        else
        {
            EnsureEncounter(activeLocation);
        }
    }

    public void LoadLastActiveLocation(RuntimeUnit runtimeUnit)
    {
        LastActiveLocation = runtimeUnit;
    }

    public void Flee()
    {
        _model.Runner.StopAllOfType(UnitType.LOCATION);
    }

    private void EnsureEncounter(RuntimeUnit activeLocation)
    {
        #region Spawn encounter
        if (ActiveEncounter == null)
        {

            var ele = activeLocation.Location.Encounters.RandomElement();
            ActiveEncounter = ele.RuntimeUnit;
            encounterProgress = 0f;
        }
        #endregion
    }

    public void FinishedSettingUpUnits()
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

    public void UpdateLoopProgressedSecond()
    {
        if (ActiveEncounter != null)
        {
            _model.ApplyResourceChanges(ActiveEncounter, ResourceChangeType.EFFECT);
        }

    }
}
