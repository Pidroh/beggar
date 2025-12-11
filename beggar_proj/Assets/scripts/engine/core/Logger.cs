using System;

namespace HeartEngineCore
{
    public static class Logger 
    {
        public static void Log(string s) 
        {
#if UNITY_ENGINE
            Debug.log(s);
#endif
        }

        public static void LogError(string v)
        {
#if UNITY_ENGINE
            Debug.LogError(v);
#endif
        }
    }
}
