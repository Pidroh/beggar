using SFB;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
//using UnityEngine.U2D;

namespace HeartUnity
{
    public class FileUtilities
    {
        private FileUtilitiesMonoBehavior _monoBehaviour;

        public FileUtilities()
        {
            var gameObjectForMessaging = new GameObject("FileUtilityMB");
            _monoBehaviour = gameObjectForMessaging.AddComponent<FileUtilitiesMonoBehavior>();
        }
        public void ExportBytes(byte[] bytes, string suggestedFileName)
        {
            ExportBytesInternal(bytes, suggestedFileName);
        }
#if UNITY_WEBGL && !UNITY_EDITOR
        //
        // WebGL
        //
        [DllImport("__Internal")]
        private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);

        // Broser plugin should be called in OnPointerDown.
        void ExportBytesInternal(byte[] bytes, string suggestedFile)
        {
            DownloadFile(_monoBehaviour.gameObject.name, "OnFileDownload", suggestedFile, bytes, bytes.Length);
        }
#else

        void ExportBytesInternal(byte[] bytes, string suggestedFile)
        {
            var path = StandaloneFileBrowser.SaveFilePanel("Exporting file", "", suggestedFile, "hg");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, bytes);
            }
        }
#endif
    }
}