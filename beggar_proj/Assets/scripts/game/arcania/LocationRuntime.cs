using System.Collections.Generic;

public class LocationRuntime
{

    
    public ConfigLocation configLocation;
    public RuntimeUnit RuntimeUnit;
    public List<IDPointer> Encounters = new();

    public LocationRuntime(RuntimeUnit ru, ConfigLocation cl)
    {
        this.RuntimeUnit = ru;
        this.RuntimeUnit.Location = this;
        configLocation = cl;
    }

    // Managed in ArcaniaModelExploration
    // public int progress;
    // public float ProgressRatio => progress / configLocation.Length;


    /*
    internal void Load(LocationProgress locationP)
    {

    }
    */
}
