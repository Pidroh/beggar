
using UnityEngine;


namespace HeartUnity.View
{
    public class UICreationHelper
    {
        public UIUnitManager manager;
        public int currentLayer;
        private Canvas backupCanvas;

        public UICreationHelper(UIUnitManager manager, int currentLayer)
        {
            this.manager = manager;
            this.currentLayer = currentLayer;
            if (manager == null) {
                backupCanvas = GameObject.FindObjectOfType<Canvas>();
            }
        }

        public T Instantiate<T>(T obj, bool active = true) where T : UIUnit
        {
            obj.gameObject.SetActive(false);
            var returnObj = GameObject.Instantiate(obj, manager.layerParents[currentLayer].transform);
            returnObj.gameObject.SetActive(active);
            returnObj.Init();
            return returnObj;
        }

        public int ToMaxLayer()
        {
            if (manager == null || manager.layerParents == null) return 0;
            int cl = currentLayer;
            currentLayer = manager.layerParents.Count - 1;
            return cl;
        }

        public T InstantiateObject<T>(T obj, bool active = true) where T : MonoBehaviour
        {

            Transform parent = manager == null ? backupCanvas.transform : manager.layerParents[currentLayer].transform;
            var rtn = GameObject.Instantiate(obj, parent);
            rtn.gameObject.SetActive(active);
            return rtn;
        }

        public Transform InstantiateObject(Transform obj, bool active = true)
        {
            var rtn = GameObject.Instantiate(obj, manager.layerParents[currentLayer].transform);
            rtn.gameObject.SetActive(active);
            return rtn;
        }

        public GameObject InstantiateObject(GameObject obj, bool active = true)
        {
            var rtn = GameObject.Instantiate(obj, manager.layerParents[currentLayer].transform);
            rtn.gameObject.SetActive(active);
            return rtn;
        }
    }
}