using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static HeartUnity.MainGameConfig;
//using UnityEngine.U2D;

namespace HeartUnity
{
    public class SaveDataUnit<T>
    {
        private string mainSaveLocation;
        private string backupSaveLocation;
        public bool forcePlayerPrefs = false;
        public bool isWebGL = false;
        public bool IsPlayerPrefs => forcePlayerPrefs && isWebGL;
        public static readonly List<PersistenceUnit> DefaultSaveDataUnits = new List<PersistenceUnit>() {
            new PersistenceUnit()
            {
                Key = SettingPersistence.Key,
                ForcePrefs = true
            }
        };

        public SaveDataUnit(string key)
        {
            var config = HeartGame.GetConfig();
            PersistenceUnit FindUnit(List<PersistenceUnit> persistenceUnits)
            {
                if (persistenceUnits != null && persistenceUnits.Count > 0)
                {
                    foreach (var item in persistenceUnits)
                    {
                        if (item.Key == key)
                        {
                            return item;
                        }
                    }
                }
                return null;
            }
            var unit = FindUnit(config.PersistenceUnits);
            if (unit == null) unit = FindUnit(DefaultSaveDataUnits);
            if (unit == null)
            {
                Debug.LogError($"Engine Error: Persistence key not found {key}");
            }
            else 
            {
                forcePlayerPrefs = unit.ForcePrefs;
            }
#if UNITY_WEBGL
            isWebGL = true;
#endif

            var backupKey = key + "_backup";
            if (IsPlayerPrefs)
            {
                mainSaveLocation = key;
                backupSaveLocation = backupKey;
                return;
            }
            mainSaveLocation = Application.persistentDataPath + "/" + key;
            backupSaveLocation = Application.persistentDataPath + "/" + backupKey;


        }

        public void Save(T obj)
        {
            var json = JsonUtility.ToJson(obj);
            if (IsPlayerPrefs)
            {
                SaveLocalPrefs(json);
                return;
            }
            SaveFileSystem(json);
        }

        private void SaveLocalPrefs(string json)
        {
            PlayerPrefs.SetString(mainSaveLocation, json);
            PlayerPrefs.SetString(backupSaveLocation, json);
            PlayerPrefs.Save();
            return;
        }

        private void SaveFileSystem(string json)
        {
            System.IO.File.WriteAllText(backupSaveLocation, json);
            System.IO.File.WriteAllText(mainSaveLocation, json);
        }


        public bool TryLoad(out T obj)
        {
            var location = mainSaveLocation;
            {
                if (TryLoadFromLocation(location, out T objMain))
                {
                    obj = objMain;
                    return true;
                }
            }
            if (TryLoadFromLocation(backupSaveLocation, out T objBack))
            {
                obj = objBack;
                return true;
            }
            obj = default(T);
            return false;
        }

        private bool TryLoadFromLocation(string location, out T objMain)
        {
            if (IsPlayerPrefs)
            {
                return TryLoadPlayerPrefs(location, out objMain);
            }
            return TryLoadFileSystem(location, out objMain);
        }

        private static bool TryLoadFileSystem(string location, out T objMain)
        {
            try
            {
                var mainFileExist = File.Exists(location);

                if (mainFileExist)
                {
                    var text = File.ReadAllText(location);
                    objMain = JsonUtility.FromJson<T>(text);
                    return true;
                }
            }
            catch (System.Exception)
            {
            }
            objMain = default(T);
            return false;
        }

        private static bool TryLoadPlayerPrefs(string location, out T objMain)
        {
            if (PlayerPrefs.HasKey(location))
            {
                try
                {
                    objMain = JsonUtility.FromJson<T>(PlayerPrefs.GetString(location));
                    return true;
                }
                catch (System.Exception)
                {
                }
            }
            objMain = default(T);
            return false;
        }

        public void Delete()
        {
            Delete(mainSaveLocation);
            Delete(backupSaveLocation);
        }

        private void Delete(string location)
        {
            var mainFileExist = File.Exists(location);

            if (mainFileExist)
            {
                File.Delete(location);
            }
        }
    }
}