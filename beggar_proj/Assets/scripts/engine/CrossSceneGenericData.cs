using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeartUnity
{
    public class CrossSceneGenericData
    {
        private static Dictionary<Type, object> dictPreviousSceneStaticTemp;
        private Dictionary<Type, object> dictPreviousScene;
        private Dictionary<Type, object> dictNextScene;

        public CrossSceneGenericData()
        {
            if (dictPreviousSceneStaticTemp != null)
            {
                dictPreviousScene = dictPreviousSceneStaticTemp;
                dictPreviousSceneStaticTemp = null;
            }

        }

        public T getDataFromPreviousScene<T>()
        {
            if (dictPreviousScene == null) return default;
            return (T)dictPreviousScene[typeof(T)];
        }

        public void RegisterForNextScene<T>(T data) 
        {
            dictNextScene ??= new();
            Type key = typeof(T);
#if UNITY_EDITOR
            if (dictNextScene.ContainsKey(key))
            {
                Debug.LogError(key + " key is duplicated - "+ key.Name);
            }
#endif
                dictNextScene[key] = data;
            
        }

        public void BeforeSceneChange()
        {
#if UNITY_EDITOR
            if (dictPreviousSceneStaticTemp != null)
            {
                Debug.LogError("Static temp variable leaked");
            }
#endif
            dictPreviousSceneStaticTemp = dictNextScene;
        }

        public bool TryGetDataFromPreviousScene<T>(out T arcaniaCrossScenePreviousScene)
        {
            arcaniaCrossScenePreviousScene = default;
            if (dictPreviousScene == null) return false;
            Type type = typeof(T);
            if(dictPreviousScene.TryGetValue(type, out var a))
            {
                arcaniaCrossScenePreviousScene = (T) a;
                return true;
            }
            return false;
        }
    }
}