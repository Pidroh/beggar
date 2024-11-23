public class ControlSubUnit 
{
    protected readonly MainGameControl _control;
    public ArcaniaUnits _arcaniaUnits => _control.arcaniaModel.arcaniaUnits;

    public ControlSubUnit(MainGameControl ctrl)
    {
        _control = ctrl;
    }
}
