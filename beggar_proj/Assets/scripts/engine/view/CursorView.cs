//using UnityEngine.U2D;

using System;
using System.Collections.Generic;
using UnityEngine;
using static HeartUnity.View.CursorManager;

namespace HeartUnity.View
{
    public class CursorView : MonoBehaviour
    {
        public float cursorSpeed = 100;
        public UIUnit mainChild;
        public List<CursorGraphicMode> graphicModes;
        public float timeBetweenFrames;
        [Serializable] 
        public class CursorConfig 
        {
            public CursorPositionBehavior behavior;
            public float rotation = 0;
            public Vector3 scale = Vector3.one;
        }
        public List<CursorConfig> cursorConfigs;
    }

    [Serializable]
    public class CursorGraphicMode {
        public Vector3 offsetLocalPos;
        public Sprite spriteChange;
    }
}