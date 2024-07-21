//using UnityEngine.U2D;

using System;
using UnityEngine;

namespace HeartUnity
{
    [Serializable]
    public struct FloatRange {
        [SerializeField]
        float value1;
        [SerializeField]
        float value2;

        public FloatRange(float value1, float value2)
        {
            this.value1 = value1;
            this.value2 = value2;
        }

        public float min => value1;
        public float max => value2;

        public float getValue(float key) {
            return key * value2 +(1 - key) * value1;
        }
    }
}