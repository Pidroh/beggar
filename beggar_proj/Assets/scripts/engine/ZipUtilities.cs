using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
//using UnityEngine.U2D;

namespace HeartUnity
{
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