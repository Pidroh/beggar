#if PLATFORM_SWITCH
using System;
using UnityEngine;
using static HeartUnity.MainGameConfig;

namespace HeartUnity
{
    public class NintendoSwitchPersistentTextUnit 
    {
        public const string MountName = "MySave";
        readonly string filePath;
        public class CrossSceneData
        {
#pragma warning disable 0414
            public nn.fs.FileHandle fileHandle;
#pragma warning restore 0414
            public float LatestSaveTime { get; internal set; }
        }
        public CrossSceneData _crossSceneData;
        public NintendoSwitchPersistentTextUnit(PersistenceUnit unit, nn.account.Uid userId) 
        {
            filePath = string.Format("{0}:/{1}", MountName, unit.Key);
            Init(userId);
        }

        private void Init(nn.account.Uid userId)
        {
            Debug.Log("NSSL First Init");
            _crossSceneData = new CrossSceneData();
#pragma warning disable 0414
            _crossSceneData.fileHandle = new nn.fs.FileHandle();
#pragma warning restore 0414

        }

        public void Save(string dataT)
        {
            Debug.Log("NSSL Save");
            var fileHandle = _crossSceneData.fileHandle;
            byte[] data = System.Text.Encoding.UTF8.GetBytes(dataT);
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
            nn.fs.File.SetSize(fileHandle, data.LongLength);
            result = nn.fs.File.Write(fileHandle, offset, data, data.LongLength, nn.fs.WriteOption.Flush);
            result.abortUnlessSuccess();

            nn.fs.File.Close(fileHandle);
            Debug.Log("NSSL Save file commit");
            result = nn.fs.FileSystem.Commit(MountName);
            result.abortUnlessSuccess();

            UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
            _crossSceneData.LatestSaveTime = UnityEngine.Time.unscaledTime;

            nn.Result OpenFileHandleForSaving(ref nn.fs.FileHandle fileHandle)
            {
                return nn.fs.File.Open(ref fileHandle, filePath, nn.fs.OpenFileMode.Write | nn.fs.OpenFileMode.AllowAppend);
            }
        }

        public string LoadPlayerPrefs()
        {
            Debug.Log("NSSL Load");
            var fileHandle = _crossSceneData.fileHandle;
            nn.fs.EntryType entryType = 0;
            nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, filePath);
            Debug.Log($"NSSL Load file {filePath}");
            if (nn.fs.FileSystem.ResultPathNotFound.Includes(result)) { return null; }
            Debug.Log($"NSSL Load success check I");
            result.abortUnlessSuccess();

            result = nn.fs.File.Open(ref fileHandle, filePath, nn.fs.OpenFileMode.Read);
            Debug.Log($"NSSL Load success check II");
            result.abortUnlessSuccess();

            long fileSize = 0;
            result = nn.fs.File.GetSize(ref fileSize, fileHandle);
            Debug.Log($"NSSL Load success check III {fileSize}");
            result.abortUnlessSuccess();

            byte[] data = new byte[fileSize];
            result = nn.fs.File.Read(fileHandle, 0, data, fileSize);
            Debug.Log($"NSSL Load success check IV ");
            result.abortUnlessSuccess();

            nn.fs.File.Close(fileHandle);

            Debug.Log($"NSSL Load file ");
            return System.Text.Encoding.UTF8.GetString(data);

        }

    }
    public class NintendoSwitchSaveLoadPrefs
    {
        private const string mountName = "MySave";
        private const string fileName = "PlayerPrefsSave";
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

        public NintendoSwitchSaveLoadPrefs Init(out bool firstInitHappened)
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

        public void SavePlayerPrefs()
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

        public void LoadPlayerPrefs()
        {
            Debug.Log("NSSL Load");
            var fileHandle = _crossSceneData.fileHandle;
            nn.fs.EntryType entryType = 0;
            nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, filePath);
            Debug.Log($"NSSL Load file {filePath}");
            if (nn.fs.FileSystem.ResultPathNotFound.Includes(result)) { return; }
            Debug.Log($"NSSL Load success check I");
            result.abortUnlessSuccess();

            result = nn.fs.File.Open(ref fileHandle, filePath, nn.fs.OpenFileMode.Read);
            Debug.Log($"NSSL Load success check II");
            result.abortUnlessSuccess();

            long fileSize = 0;
            result = nn.fs.File.GetSize(ref fileSize, fileHandle);
            Debug.Log($"NSSL Load success check III {fileSize}");
            result.abortUnlessSuccess();

            byte[] data = new byte[fileSize];
            result = nn.fs.File.Read(fileHandle, 0, data, fileSize);
            Debug.Log($"NSSL Load success check IV ");
            result.abortUnlessSuccess();

            nn.fs.File.Close(fileHandle);

            Debug.Log($"NSSL Load player prefs ");
            UnityEngine.Switch.PlayerPrefsHelper.rawData = data;
        }
    }
}
#endif