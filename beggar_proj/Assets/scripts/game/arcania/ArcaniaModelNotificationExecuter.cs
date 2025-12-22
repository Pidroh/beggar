using Codice.Client.Commands;
using System.Collections.Generic;

public static class ArcaniaModelNotificationExecuter
{
    // very simple single model
    public static void Report(ArcaniaModel model, RuntimeUnit source, IDPointer target, float value, ResourceChangeType changeType) 
    {
        List<ArcaniaModelNotificationDataUnit> notificationList = model.notificationData.notificationUnits;
        if (notificationList.Count == 0) 
        {
            ArcaniaModelNotificationDataUnit unitC = new();
            unitC.Subunits.Add(new());
            notificationList.Add(unitC);
        }
        var unit = notificationList[0];
        unit.sourceUnit = source;
        ArcaniaModelNotificationDataUnit.ModifySubunit modifySubunit = unit.Subunits[0];
        modifySubunit.value = value;
        modifySubunit.target = target;
        modifySubunit.changeType = changeType;
    }
}

public class ArcaniaModelNotificationData 
{
    public List<ArcaniaModelNotificationDataUnit> notificationUnits = new();
}

public class ArcaniaModelNotificationDataUnit
{
    public RuntimeUnit sourceUnit;

    public List<ModifySubunit> Subunits = new();

    public class ModifySubunit 
    {
        public IDPointer target;
        public float value;
        public ResourceChangeType changeType;

        public bool active;
    }
}