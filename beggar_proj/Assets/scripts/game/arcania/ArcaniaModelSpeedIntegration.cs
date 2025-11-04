public static class ArcaniaModelSpeedIntegration 
{
    public static void ManualUpdate(ArcaniaModel model) 
    {
        #region check if existing
        for (int i = 0; i < model.speedIntegrationData.multiplierRuntimeUnits.Length; i++)
        {
            if (model.speedIntegrationData.multiplierRuntimeUnits[i] != null) continue;
            model.speedIntegrationData.multiplierRuntimeUnits[i] = model.FindRuntimeUnit(UnitType.TASK, ArcaniaSpeedIntegrationData.runtimeUnitIds[i]);
        }
        #endregion
        #region feed parameters
        System.Collections.Generic.List<ArcaniaSpeedIntegrationData.MultiplierTypes> multiplierTs = EnumHelper<ArcaniaSpeedIntegrationData.MultiplierTypes>.GetAllValues();
        for (int i = 0; i < multiplierTs.Count; i++)
        {
            var mul = model.speedIntegrationData.multiplierRuntimeUnits[i].GetSpeedMultiplier();
            ArcaniaSpeedIntegrationData.MultiplierTypes item = multiplierTs[i];
            switch (item)
            {
                case ArcaniaSpeedIntegrationData.MultiplierTypes.GLOBAL:
                    model.speedParameters.globalMultiplier = mul;
                    break;
                case ArcaniaSpeedIntegrationData.MultiplierTypes.EXPLORATION:
                    model.speedParameters.explorationMultiplier = mul;
                    break;
                case ArcaniaSpeedIntegrationData.MultiplierTypes.SKILL:
                    model.speedParameters.skillStudyingMultiplier = mul;
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
