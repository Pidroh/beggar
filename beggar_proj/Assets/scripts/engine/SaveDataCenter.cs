﻿using System.Collections.Generic;
using UnityEngine.Pool;
using static HeartUnity.MainGameConfig;

namespace HeartUnity
{
    public class SaveDataCenter
    {
        public static byte[] GenerateExportSave()
        {
            using var _1 = ListPool<string>.Get(out var listName);
            using var _2 = ListPool<string>.Get(out var listContent);
            ProcessPersistenceUnitList(HeartGame.GetConfig().PersistenceUnits);
            ProcessPersistenceUnitList(PersistentTextUnit.DefaultSaveDataUnits);
            void ProcessPersistenceUnitList(List<PersistenceUnit> units)
            {
                foreach (var u in units)
                {
                    var ptu = new PersistentTextUnit(u.Key, null);
                    var foundData = ptu.TryLoad(ptu.mainSaveLocation, out var data);
                    if (foundData) 
                    {
                        listName.Add(u.Key);
                        listContent.Add(data);
                    }
                }
            }
            return ZipUtilities.CreateZipBytesFromVirtualFiles(listName, listContent);
        }

        internal static void ImportSave(List<string> names, List<string> content)
        {
            ProcessPersistenceUnitList(HeartGame.GetConfig().PersistenceUnits);
            ProcessPersistenceUnitList(PersistentTextUnit.DefaultSaveDataUnits);
            void ProcessPersistenceUnitList(List<PersistenceUnit> units)
            {
                foreach (var u in units)
                {
                    if (!names.Contains(u.Key)) continue;
                    var index = names.IndexOf(u.Key);
                    var ptu = new PersistentTextUnit(u.Key, null);
                    ptu.Save(content[index]);
                }
            }
        }

        public void Init() 
        {
        }

        public void HardwareLoad() 
        {
        }
    }
}