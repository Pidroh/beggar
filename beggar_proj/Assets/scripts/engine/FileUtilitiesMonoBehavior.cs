using System;
using System.Collections;
using UnityEngine;
//using UnityEngine.U2D;

namespace HeartUnity
{
    public class FileUtilitiesMonoBehavior : MonoBehaviour 
    {
        public byte[] UploadedBytes { get; private set; }

        // Called from browser
        public void OnFileDownload()
        {

        }

        public void OnFileUpload(string url)
        {
            StartCoroutine(OutputRoutine(url));
        }

        public IEnumerator OutputRoutine(string url)
        {
            var loader = new WWW(url);
            yield return loader;
            UploadedBytes = loader.bytes;
        }

        internal void ResetBytes()
        {
            UploadedBytes = null;
        }
    }
}