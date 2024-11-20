using System.Collections.Generic;

public class LocationRuntime
{
    public int progress;
    public ConfigLocation configLocation;
    public RuntimeUnit RuntimeUnit;
    public List<IDPointer> Encounters = new();

    public LocationRuntime(RuntimeUnit ru, ConfigLocation cl)
    {
        this.RuntimeUnit = ru;
        this.RuntimeUnit.Location = this;
        configLocation = cl;
    }

    public float ProgressRatio => progress / configLocation.Length;


    /*
    internal void Load(LocationProgress locationP)
    {

    }
    */
}
