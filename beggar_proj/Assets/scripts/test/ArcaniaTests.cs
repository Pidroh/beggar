using UnityEditor;

public class ArcaniaTests 
{
    public void ExecuteTests() 
    {
        ArcaniaModel arcaniaModel = new();
        var testConfig = AssetDatabase.LoadAssetAtPath<ArcaniaTestConfig>("Assets/Editor/Configs/ArcaniaTestConfig.asset");
        JsonReader.ReadJson(arcaniaModel.arcaniaUnits, testConfig.testDatas);
    }
}