using UnityEditor;
using UnityEngine;

public class ArcaniaTests
{
    [MenuItem("Arcania/Run Tests")]
    public static void RunTestsFromMenu()
    {
        ArcaniaTests tests = new();
        tests.ExecuteTests();
    }

    public static ArcaniaTestConfig LoadConfig()
    {
        string[] guids = AssetDatabase.FindAssets("t:ArcaniaTestConfig");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<ArcaniaTestConfig>(path);
        }

        Debug.LogWarning("ArcaniaTestConfig asset not found.");
        return null;
    }

    public void ExecuteTests()
    {
        ArcaniaModel arcaniaModel = new();
        var testConfig = LoadConfig();
        JsonReader.ReadJsonAllAtOnce(arcaniaModel.arcaniaUnits, testConfig.testDatas, false);
        arcaniaModel.FinishedSettingUpUnits();
        var ruA = arcaniaModel.FindRuntimeUnit("resource_a");
        var ruB = arcaniaModel.FindRuntimeUnit("resource_b");
        var ruAct = arcaniaModel.FindRuntimeUnit("action_a");
        var ruActMod = arcaniaModel.FindRuntimeUnit("action_a_mod");
        var valueB = ruB.Value;
        arcaniaModel.ManualUpdate(1.1f);
        if (valueB == ruB.Value)
        {
            UnityEngine.Debug.Log("Value did not change");
            arcaniaModel.ManualUpdate(1.1f);
        }
        arcaniaModel.Runner.StartActionExternally(ruAct);
        arcaniaModel.ManualUpdate(1.1f);
        if (ruActMod.Value != 0) 
        {
            UnityEngine.Debug.Log("dot started, should not");
            arcaniaModel.ManualUpdate(1.1f);
        }
        arcaniaModel.ManualUpdate(2.1f);
        if (ruActMod.Value == 0)
        {
            UnityEngine.Debug.Log("dot did not start");
            arcaniaModel.ManualUpdate(1.1f);
        }


    }
}