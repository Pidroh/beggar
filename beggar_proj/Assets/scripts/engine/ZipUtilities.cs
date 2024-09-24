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

        public static void ExtractZipFromBytes(byte[] zipBytes, List<string> fileNames, List<string> fileContents)
        {
            using var memoryStream = new MemoryStream(zipBytes);
            using var zipInputStream = new ZipInputStream(memoryStream);

            ZipEntry entry;
            while ((entry = zipInputStream.GetNextEntry()) != null)
            {
                // Add the file name to the fileNames list
                fileNames.Add(entry.Name);

                // Read file content
                using var entryMemoryStream = new MemoryStream();
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = zipInputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    entryMemoryStream.Write(buffer, 0, bytesRead);
                }

                // Convert the byte array to a string (assuming text content)
                string content = Encoding.UTF8.GetString(entryMemoryStream.ToArray());

                // Add the content to the fileContents list
                fileContents.Add(content);
            }
        }
    }
}