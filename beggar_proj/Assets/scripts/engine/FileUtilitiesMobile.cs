
#if UNITY_ANDROID
using System;
using System.IO;
using UnityEngine;
//using UnityEngine.U2D;

namespace HeartUnity
{
    public class FileUtilities
    {
        public byte[] UploadedBytes;

        public FileUtilities()
        {
        } 

        public void ImportFileRequest(string extension)
        {
            NativeFilePicker.PickFile(path => 
            {
                UploadedBytes = File.ReadAllBytes(path);
            });
        }

        internal void ExportBytes(byte[] bytes, string suggestedFileName, string extension)
        {
            // write temporary path
            var path = Path.Combine(Application.temporaryCachePath, $"{suggestedFileName}.{extension}");
            File.WriteAllBytes(path, bytes);
            NativeFilePicker.ExportFile(path);
        }
    }
}
#endif