using HeartEngineCore;
using static ArcaniaModel;

public class ArcaniaModelHousing : ArcaniaModelSubmodule
{
    public object SpaceConsumed => CalculateConsumedSpace();

    public object TotalSpace => CalculateMaxSpace();

    public ArcaniaModelHousing(ArcaniaModel arcaniaModel) : base(arcaniaModel)
    {
    }

    public bool CanAcquireFurniture(RuntimeUnit ru)
    {
        if (ru.IsMaxed) return false;
        if (!_model.CanAfford(ru.ConfigTask.Cost)) return false;
        if (CalculateMaxSpace() < ru.ConfigFurniture.SpaceConsumed + CalculateConsumedSpace()) return false;
        return true;
    }

    public bool FurnitureNotMaxedButNotEnoughSpace(RuntimeUnit ru) 
    {
        if (ru.IsMaxed) return false;
        if (CalculateMaxSpace() < ru.ConfigFurniture.SpaceConsumed + CalculateConsumedSpace()) return true;
        return false;
    }

    public void AcquireFurniture(RuntimeUnit ru)
    {
        _model.ApplyResourceChanges(ru, ResourceChangeType.COST);
        ru.ChangeValue(1);
    }

    public bool CanRemoveFurniture(RuntimeUnit ru) 
    {
        return ru.Value >= 1;
    }

    public void RemoveFurniture(RuntimeUnit ru) 
    {
        ru.ChangeValue(-1);
    }

    private int CalculateMaxSpace()
    {
        var space = 0f;
        foreach (var mod in _model.arcaniaUnits.SpaceMods)
        {
            space += mod.Source.Value * mod.Value;
        }
        return MathfHG.CeilToInt(space);
    }

    public bool CanChangeHouse(RuntimeUnit ru)
    {
        // already in the house, so cannot change to it
        if (ru.Value != 0) return false;
        if (ru.ConfigHouse.AvailableSpace < CalculateConsumedSpace()) return false;
        if (!_model.CanAfford(ru.ConfigTask.Cost)) return false;
        return true;
    }

    public void ChangeHouse(RuntimeUnit ru)
    {
        var houses = _model.arcaniaUnits.datas[UnitType.HOUSE];
        // unequip all houses first
        foreach (var f in houses)
        {
            f.SetValue(0);
        }
        ru.SetValue(1);
    }

    private int CalculateConsumedSpace()
    {
        var space = 0;
        var furnitures = _model.arcaniaUnits.datas[UnitType.FURNITURE];
        foreach (var f in furnitures)
        {
            space += f.ConfigFurniture.SpaceConsumed * f.Value;
        }
        return space;
    }

    public bool IsLivingInHouse(RuntimeUnit data)
    {
        return data.Value > 0;
    }
}
