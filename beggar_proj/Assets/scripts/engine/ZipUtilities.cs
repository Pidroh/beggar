using ICSharpCode.SharpZipLib.Zip;
using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
//using UnityEngine.U2D;

namespace HeartUnity
{
    public class FileUtilities
    {
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
        var bytes = CreateZipBytes();
        DownloadFile(gameObject.name, "OnFileDownload", "exported_save.zip", bytes, bytes.Length);
    }

    // Called from browser
    public void OnFileDownload() {
        output.text = "File Successfully Downloaded";
    }
#else

        void ExportBytesInternal(byte[] bytes, string suggestedFile)
        {
            var path = StandaloneFileBrowser.SaveFilePanel("Exporting file", "", suggestedFile, "arc");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, bytes);
            }
        }
#endif
    }
    public class ZipUtilities
    {
        public static byte[] CreateZipBytesFromVirtualFiles(List<string> fileNames, List<string> fileContent)
        {
            using MemoryStream memoryStream = new();
            using ZipOutputStream s = new ZipOutputStream(memoryStream);
            for (int i = 0; i < fileNames.Count; i++)
            {
                string name = fileNames[i];
                string content = fileContent[i];
                var entry = new ZipEntry(Path.GetFileName(name));
                entry.DateTime = DateTime.Now;
                s.PutNextEntry(entry);

                byte[] fileBytes = Encoding.UTF8.GetBytes(content);
                s.Write(fileBytes, 0, fileBytes.Length);
                s.CloseEntry();
            }
            s.Finish();
            return memoryStream.ToArray();
        }
    }
}