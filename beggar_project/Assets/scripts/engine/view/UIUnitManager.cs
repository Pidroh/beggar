using System.Collections.Generic;
using UnityEngine;

namespace HeartUnity.View
{
    public class UIUnitManager : MonoBehaviour
    {
        public List<GameObject> layerParents = new List<GameObject>();
        public GameObject layerHolder;

        public int HighestLayer => layerParents.Count - 1;

        [ContextMenu("update layer parents")]
        public void updateLayerParents() {
            var cc = layerHolder.transform.childCount;
            layerParents.Clear();
            for (int i = 0; i < cc; i++)
            {
                layerParents.Add(layerHolder.transform.GetChild(i).gameObject);
            }
        }
    }
}