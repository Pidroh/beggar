using UnityEditor;

public class ArcaniaTests 
{
    public void ExecuteTests() 
    {
        var testConfig = AssetDatabase.LoadAssetAtPath<ArcaniaTestConfig>("Assets/Editor/Configs/ArcaniaTestConfig.asset");
    }
}