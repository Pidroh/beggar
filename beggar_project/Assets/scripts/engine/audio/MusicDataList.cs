using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HeartUnity
{
    public class MusicDataList : MonoBehaviour
    {
        public List<MusicUnitData> musicDatas;
    }


    [Serializable]
    public class MusicUnitData
    {
        public string key;
        public AudioClip sourceFile;
        public float volumeMultiplier = 1f;
        public float fadeIntoItself = -1f;
    }



}