#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif
using System;
using UnityEngine;

namespace HeartUnity
{
    public static class URLOpener
    {
        /** 
         * Opens both on Steam API and the browser, 
         * this is because Steam can be slow on some devices
         */
        public static void OpenSteamURL(string url, uint appId)
        {

#if !DISABLESTEAMWORKS
            if (appId > 0 && SteamManager.Initialized)
            {
                AppId_t appId_t = new AppId_t(appId);
                SteamFriends.ActivateGameOverlayToStore(appId_t, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
            }
#endif
            Application.OpenURL(url);
        }

        internal static void OpenURL(string url)
        {
            Application.OpenURL(url);
        }
    }
}