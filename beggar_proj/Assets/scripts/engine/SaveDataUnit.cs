using UnityEngine;
using static HeartUnity.MainGameConfig;
//using UnityEngine.U2D;

namespace HeartUnity
{
    public class SaveDataUnit<T>
    {
        private PersistentTextUnit textUnit;

        internal SaveDataUnit(PersistenceUnit unit)
        {
            textUnit = new PersistentTextUnit(unit);
        }

        public SaveDataUnit(string key)
        {
            textUnit = new PersistentTextUnit(key);
        }

        public void Save(T obj)
        {
            var json = JsonUtility.ToJson(obj);
            textUnit.Save(json);

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
                    objMain = JsonUtility.FromJson<T>(jsonData);
                    return true;
                }
                catch (System.Exception)
                {
                }

            }
            objMain = default(T);
            return false;
        }

       
    }
}