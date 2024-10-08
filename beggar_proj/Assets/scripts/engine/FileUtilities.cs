
#if !UNITY_SWITCH
using SFB;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
//using UnityEngine.U2D;

namespace HeartUnity
{
    public class FileUtilities
    {
        private FileUtilitiesMonoBehavior _monoBehaviour;
        public byte[] UploadedBytes => _monoBehaviour.UploadedBytes;

        public FileUtilities()
        {
            var gameObjectForMessaging = new GameObject("FileUtilityMB");
            _monoBehaviour = gameObjectForMessaging.AddComponent<FileUtilitiesMonoBehavior>();
        }
        public void ExportBytes(byte[] bytes, string suggestedFileName, string extension)
        {
            ExportBytesInternal(bytes, suggestedFileName, extension);
        }

        public void ImportFileRequest(string extension)
        {
            StartImportInternal("hg");
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        //
        // WebGL
        //
        [DllImport("__Internal")]
        private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);

        // Broser plugin should be called in OnPointerDown.
        void ExportBytesInternal(byte[] bytes, string suggestedFile, string extension)
        {
            DownloadFile(_monoBehaviour.gameObject.name, "OnFileDownload", $"{suggestedFile}.{extension}", bytes, bytes.Length);
        }
#else

        void ExportBytesInternal(byte[] bytes, string suggestedFile, string extension)
        {
            var path = StandaloneFileBrowser.SaveFilePanel("Exporting file", "", suggestedFile, "hg");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, bytes);
            }
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        //
        // WebGL
        //
        [DllImport("__Internal")]
        private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

        private void StartImportInternal(string extension)
        {
            UploadFile(_monoBehaviour.gameObject.name, "OnFileUpload", extension, false);
        }
#else
        private void StartImportInternal(string extension)
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("Select file to import", "", extension, false);
            if (paths.Length > 0)
            {
                _monoBehaviour.OnFileUpload(new System.Uri(paths[0]).AbsoluteUri);
            }
        }
#endif


    }
}
#endif