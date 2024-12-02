using UnityEngine;

[CreateAssetMenu(fileName = "ArcaniaGameConfiguration", menuName = "Arcania/Arcania Game Configuration", order = 1)]
public class FileBuildConfigurations : ScriptableObject
{
    public List<Entry> entries;

    [Serializable]
    public class Entry
    {
        public string tag;
        public string copyFileTag;
        public string outputPath;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    }
}


public class DeleteMe : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
