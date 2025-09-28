//using UnityEngine.U2D;

using System;
using UnityEngine;

namespace HeartUnity
{
    public class PlayTimeControl
    {
        public static DateTime? dateTimePreviousScene;
        public int PlayTimeToShow => Mathf.CeilToInt(playTime);
        public float playTime;
        public string PlayTimeToShowAsString => ConvertSecondsToTimeFormat(PlayTimeToShow);
        public void Update()
        {
            playTime += Time.unscaledDeltaTime;
        }

        public void Init(CommonPlayerSaveData commonSaveData)
        {
            playTime = commonSaveData.TotalPlayTimeSeconds;
            // get time between scene transitions
            if (dateTimePreviousScene.HasValue)
            {
                if (dateTimePreviousScene < DateTime.Now)
                {
                    var secs = Mathf.Min((float)(DateTime.Now - dateTimePreviousScene.Value).TotalSeconds, 60f);
                    playTime += Mathf.CeilToInt(secs);
                }
                dateTimePreviousScene = null;
            }

        }

        internal void BeforeChangeScene()
        {
            dateTimePreviousScene = DateTime.Now;
        }

        

        public static string ConvertSecondsToTimeFormat(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

    }
}