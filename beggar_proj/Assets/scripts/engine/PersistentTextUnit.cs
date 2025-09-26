using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static HeartUnity.MainGameConfig;
//using UnityEngine.U2D;

namespace HeartUnity
{
    public class PersistentTextUnit 
    {
#if UNITY_SWITCH
        NintendoSwitchPersistentTextUnit switchTextUnit;
#endif
        internal string mainSaveLocation;
        internal string backupSaveLocation;
        public bool forcePlayerPrefs = false;
        public bool isWebGL = false;
        public bool IsPlayerPrefs => forcePlayerPrefs;
        public static readonly List<PersistenceUnit> DefaultSaveDataUnits = new List<PersistenceUnit>() {
            new PersistenceUnit()
            {
                Key = SettingPersistence.Key,
                ForcePrefs = false
            },
            new PersistenceUnit()
            {
                Key = HeartGame.DefaultCommonsSaveDataKey,
                ForcePrefs = false
            },
        };

        public PersistentTextUnit(string key, HeartGame heartGame) {
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
            Init(unit, heartGame);
        }

        public PersistentTextUnit(PersistenceUnit unit, HeartGame heartGame)
        {
            Init(unit, heartGame);

        }

        internal void Init(PersistenceUnit unit, HeartGame heartGame)
        {
#if UNITY_WEBGL
            isWebGL = true;
#endif
            forcePlayerPrefs = unit.ForcePrefs || isWebGL;

            var key = unit.Key;
            var backupKey = key + "_backup";
#if UNITY_SWITCH && !UNITY_EDITOR
            switchTextUnit = new(unit, heartGame.crossSceneData.UserId);
            return;
#endif
#if UNITY_SWITCH && UNITY_EDITOR
            forcePlayerPrefs = true;
#endif
            if (IsPlayerPrefs)
            {
                mainSaveLocation = key;
                backupSaveLocation = backupKey;
                return;
            }
#if !UNITY_SWITCH
            mainSaveLocation = Application.persistentDataPath + "/" + key;
            backupSaveLocation = Application.persistentDataPath + "/" + backupKey;
#endif
        }
#if UNITY_SWITCH && !UNITY_EDITOR
        internal bool TryLoad(string location, out string jsonData)
        {
            jsonData = switchTextUnit.LoadPlayerPrefs();
            return jsonData != null;
        }

        public void Save(string data)
        {
            switchTextUnit.Save(data);
        }
#else
        internal bool TryLoad(out string jsonData) 
        {
            if (TryLoad(mainSaveLocation, out var d1)) 
            {
                jsonData = d1;
                return true;
            }
            if (TryLoad(backupSaveLocation, out var d2))
            {
                jsonData = d2;
                return true;
            }
            jsonData = null;
            return false;
        }
        internal bool TryLoad(string location, out string jsonData)
        {
            if (IsPlayerPrefs)
            {
                Debug.Log("Load File player prefs " + location);
                return TryLoadPlayerPrefs(location, out jsonData);
            }
            else
            {
                return TryLoadFileSystem(location, out jsonData);
            }

        }

        private static bool TryLoadFileSystem(string location, out string data)
        {
            try
            {
                var mainFileExist = File.Exists(location);

                if (mainFileExist)
                {
                    data = File.ReadAllText(location);
                    Debug.Log("Load File ok "+location);
                    return true;
                }
            }
            catch (System.Exception)
            {
                Debug.Log("Load File problem serialization" + location);
            }
            Debug.Log("Load File problem " + location);
            data = null;
            return false;
        }

        private static bool TryLoadPlayerPrefs(string location, out string jsonData)
        {
            if (PlayerPrefs.HasKey(location))
            {
                try
                {
                    jsonData = PlayerPrefs.GetString(location);
                    return true;
                }
                catch (System.Exception)
                {
                }
            }
            jsonData = null;
            return false;
        }

        private void SaveLocalPrefs(string json)
        {
            PlayerPrefs.SetString(mainSaveLocation, json);
            PlayerPrefs.SetString(backupSaveLocation, json);
            Debug.Log("Save local prefs "+mainSaveLocation);
            Debug.Log("Save local prefs backup" + backupSaveLocation);
            PlayerPrefs.Save();
            return;
        }

        private void SaveFileSystem(string json)
        {
            File.WriteAllText(backupSaveLocation, json);
            File.WriteAllText(mainSaveLocation, json);
            Debug.Log("Save file "+mainSaveLocation);
            Debug.Log("Save file back " + backupSaveLocation);
        }

        public void Save(string data)
        {
            if (IsPlayerPrefs)
            {
                SaveLocalPrefs(data);
                return;
            }
            SaveFileSystem(data);
        }
#endif


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