//using UnityEngine.U2D;

namespace HeartUnity
{
    public class FileImportHelper 
    {
        private bool _importingFile;

        public void CheckImported(FileUtilities fileUtilities) 
        {
            if (_importingFile && fileUtilities.UploadedBytes != null) 
            {
                _importingFile = false;

            }
        }
    }
}