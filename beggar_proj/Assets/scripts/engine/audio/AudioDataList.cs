using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeartUnity
{
    public class AudioDataList : MonoBehaviour
    {
        public List<AudioUnitData> audioDatas;
        [Serializable]
        public class AudioUnitData
        {
            public string key;
            public AudioClip sourceFile;
            public AudioClip[] sourceFiles;
            public float volumeMultiplier = 1f;

            public bool IsMultipleFile => sourceFiles != null && sourceFiles.Length > 0;

            internal AudioClip RandomAudioFile()
            {
                return sourceFiles[UnityEngine.Random.Range(0, sourceFiles.Length)];
            }
        }
    }



}