//using UnityEngine.U2D;
using System;
using UnityEngine;

namespace HeartUnity.View
{
    [Serializable]
    public class AnimationUnitData {
        public float delay;
        public float duration;
        public Vector3 offset;
        public OffsetMode offsetMode;
        public float transparency;
        public TransparencyMode transparencyMode;

        // public float TotalTime => duration + delay;

        [Serializable]
        public enum TransparencyMode
        {
            TO_TRANSPARENCY, FROM_TRANSPARENCY
        }

        [Serializable]
        public enum OffsetMode { 
            MOVE_TO_OFFSET, MOVE_FROM_OFFSET
        }
    }
}
