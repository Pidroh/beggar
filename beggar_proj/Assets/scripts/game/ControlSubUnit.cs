public class ControlSubUnit 
{
    protected readonly MainGameControl _control;
    public ArcaniaUnits _arcaniaUnits => _control.arcaniaModel.arcaniaUnits;
    public ArcaniaModel _model => _control.arcaniaModel;

    public ControlSubUnit(MainGameControl ctrl)
    {
        _control = ctrl;
    }
}
