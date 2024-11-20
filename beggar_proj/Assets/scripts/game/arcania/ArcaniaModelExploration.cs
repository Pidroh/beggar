using static ArcaniaModel;

public class ArcaniaModelExploration : ArcaniaModelSubmodule
{
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


    }
}
