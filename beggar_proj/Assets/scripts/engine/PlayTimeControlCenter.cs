//using UnityEngine.U2D;

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeartUnity
{
    public class PlayTimeControlCenter
    {
        public static Dictionary<string, DateTime> dateTimePreviousScene = new();
        private bool _inited;
        public List<PlaytimeUnit> units = new();

        public PlaytimeUnit MainTime { get; private set; }

        internal PlayTimeControlCenter() 
        { 
        }

        public PlaytimeUnit Register(string id, int savedTime) 
        {
            if (_inited) Debug.LogError("Register play time before init");
            var pu = new PlaytimeUnit(id);
            pu.playTime = savedTime;
            units.Add(pu);
            return pu;
        }
        public void Update()
        {
            foreach (var item in units)
            {
                item.Update();
            }
        }

        public void Load(CommonPlayerSaveData commonSaveData)
        {
            var mainTime = Register("engine_main_time", savedTime: commonSaveData.TotalPlayTimeSeconds);
            this.MainTime = mainTime;
            // get time between scene transitions
            Init();
        }

        private void Init()
        {
            _inited = true;
            foreach (var item in units)
            {
                if (dateTimePreviousScene.TryGetValue(item.id, out var dT))
                {
                    if (dT < DateTime.Now)
                    {
                        var secs = Mathf.Min((float)(DateTime.Now - dT).TotalSeconds, 60f);
                        item.playTime += Mathf.CeilToInt(secs);
                    }
                }
            }
            dateTimePreviousScene.Clear();
        }

        internal void BeforeChangeScene()
        {
            foreach (var item in units)
            {
                dateTimePreviousScene.Add(item.id, DateTime.Now);
            }
        }

        

        public static string ConvertSecondsToTimeFormat(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        internal void FeedSaveCommonData(CommonPlayerSaveData common)
        {
            common.TotalPlayTimeSeconds = (int) MainTime.playTime;
        }
    }

    public class PlaytimeUnit 
    {
        public int PlayTimeToShow => Mathf.CeilToInt(playTime);
        public float playTime;
        public string id;

        // don't instantiate outside
        internal PlaytimeUnit(string id)
        {
            this.id = id;
        }

        public string PlayTimeToShowAsString => PlayTimeControlCenter.ConvertSecondsToTimeFormat(PlayTimeToShow);

        // should not be called externally from the engine since it's called by the center
        internal void Update()
        {
            playTime += Time.unscaledDeltaTime;
        }
    }
}