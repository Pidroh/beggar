#if PLATFORM_SWITCH
using System;
using UnityEngine;

namespace HeartUnity
{
    public class NintendoSwitchSaveLoad
    {
        private const string mountName = "MySave";
        private const string fileName = "MySaveData";
        private static readonly string filePath = string.Format("{0}:/{1}", mountName, fileName);

        public static CrossSceneData _crossSceneDataTemp;
        public CrossSceneData _crossSceneData;

        public float TimeSinceLastSave => Time.unscaledTime - _crossSceneData.LatestSaveTime;

        public class CrossSceneData
        {
            public nn.account.Uid userId;
#pragma warning disable 0414
            public nn.fs.FileHandle fileHandle;
#pragma warning restore 0414
            public float LatestSaveTime { get; internal set; }
        }

        public NintendoSwitchSaveLoad Init(out bool firstInitHappened)
        {
            firstInitHappened = false;
            Debug.Log("NSSL Init");
            if (_crossSceneDataTemp == null) 
            {
                firstInitHappened = true;
                FirstInit();
            }
            else
            {
                _crossSceneData = _crossSceneDataTemp;
            }
            _crossSceneDataTemp = null;
            return this;
            
        }

        public void BeforeChangeScene()
        {
            _crossSceneDataTemp = _crossSceneData;
        }

        private void FirstInit()
        {
            Debug.Log("NSSL First Init");
            _crossSceneData = new CrossSceneData();
#pragma warning disable 0414
            _crossSceneData.fileHandle = new nn.fs.FileHandle();
#pragma warning restore 0414
            nn.account.Account.Initialize();
            Debug.Log("NSSL Account Init");
            nn.account.UserHandle userHandle = new nn.account.UserHandle();
            Debug.Log("NSSL before open selected user");
            if (!nn.account.Account.TryOpenPreselectedUser(ref userHandle))
            {
                Debug.Log("NSSL before abort call");
                nn.Nn.Abort("Failed to open preselected user.");
            }
            Debug.Log("NSSL before user id");
            nn.Result result = nn.account.Account.GetUserId(ref _crossSceneData.userId, userHandle);
            Debug.Log("NSSL Try abort on user id");
            result.abortUnlessSuccess();
            result = nn.fs.SaveData.Mount(mountName, _crossSceneData.userId);
            Debug.Log("NSSL Try abort on mounting");
            result.abortUnlessSuccess();
        }

        public void Save()
        {
            Debug.Log("NSSL Save");
            var fileHandle = _crossSceneData.fileHandle;
            byte[] data = UnityEngine.Switch.PlayerPrefsHelper.rawData;
            Debug.Log($"NSSL Save size {data.LongLength}");

            UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();

            nn.Result result = OpenFileHandleForSaving(ref fileHandle);
            if (!result.IsSuccess())
            {
                Debug.Log($"NSSL Save no file");
                result = nn.fs.File.Create(filePath, data.LongLength);
                Debug.Log($"NSSL Save file created {result.IsSuccess()}");
                result = OpenFileHandleForSaving(ref fileHandle);
                Debug.Log($"NSSL Save file opened second try {result.IsSuccess()}");
            }
            Debug.Log("NSSL Save file open");
            result.abortUnlessSuccess();

            const int offset = 0;
            Debug.Log("NSSL Save file flush");
            result = nn.fs.File.Write(fileHandle, offset, data, data.LongLength, nn.fs.WriteOption.Flush);
            result.abortUnlessSuccess();

            nn.fs.File.Close(fileHandle);
            Debug.Log("NSSL Save file commit");
            result = nn.fs.FileSystem.Commit(mountName);
            result.abortUnlessSuccess();

            UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
            _crossSceneData.LatestSaveTime = UnityEngine.Time.unscaledTime;

            static nn.Result OpenFileHandleForSaving(ref nn.fs.FileHandle fileHandle)
            {
                return nn.fs.File.Open(ref fileHandle, filePath, nn.fs.OpenFileMode.Write | nn.fs.OpenFileMode.AllowAppend);
            }
        }

        public void Load()
        {
            Debug.Log("NSSL Load");
            var fileHandle = _crossSceneData.fileHandle;
            nn.fs.EntryType entryType = 0;
            nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, filePath);
            if (nn.fs.FileSystem.ResultPathNotFound.Includes(result)) { return; }
            result.abortUnlessSuccess();

            result = nn.fs.File.Open(ref fileHandle, filePath, nn.fs.OpenFileMode.Read);
            result.abortUnlessSuccess();

            long fileSize = 0;
            result = nn.fs.File.GetSize(ref fileSize, fileHandle);
            result.abortUnlessSuccess();

            byte[] data = new byte[fileSize];
            result = nn.fs.File.Read(fileHandle, 0, data, fileSize);
            result.abortUnlessSuccess();

            nn.fs.File.Close(fileHandle);

            UnityEngine.Switch.PlayerPrefsHelper.rawData = data;
        }
    }
}
#endif