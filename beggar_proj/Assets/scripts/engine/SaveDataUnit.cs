using UnityEngine;
using static HeartUnity.MainGameConfig;
//using UnityEngine.U2D;

namespace HeartUnity
{
    public class SaveDataUnit<T>
    {
        private PersistentTextUnit textUnit;

        internal SaveDataUnit(PersistenceUnit unit, HeartGame heartGame)
        {
            textUnit = new PersistentTextUnit(unit, heartGame);
        }

        public SaveDataUnit(string key, HeartGame heartGame)
        {
            textUnit = new PersistentTextUnit(key, heartGame);
        }

        public void Save(T obj)
        {
            var json = JsonUtility.ToJson(obj);
            textUnit.Save(json);
            Debug.Log($"Data save {json}");

        }
        public bool TryLoad(out T obj)
        {
            var location = textUnit.mainSaveLocation;
            {
                if (TryLoadFromLocation(location, out T objMain))
                {
                    obj = objMain;
                    return true;
                }
            }
            if (TryLoadFromLocation(textUnit.backupSaveLocation, out T objBack))
            {
                obj = objBack;
                return true;
            }
            obj = default(T);
            return false;
        }

        private bool TryLoadFromLocation(string location, out T objMain)
        {
            var foundData = textUnit.TryLoad(location, out var jsonData);
            
            if (foundData)
            {
                try
                {
                    Debug.Log($"Data loaded {jsonData}");
                    objMain = JsonUtility.FromJson<T>(jsonData);
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.Log($"Data loaded exception");
                    Debug.Log($"Data loaded exception message {ex.Message}");
                }

            }
            Debug.Log($"Data loaded default {location}");
            objMain = default(T);
            return false;
        }

       
    }
}