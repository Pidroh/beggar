﻿//using UnityEngine.U2D;

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

        public bool BothEqual(float v)
        {
            return min == v && max == v;
        }

        public bool SmallerThan(float v)
        {
            return min < v && max < v;
        }

        public bool BiggerThan(float v) => max > v && min > v;
        public bool BiggerOrEqual(float v) => max >= v && min >= v;
        public bool SmallerOrEqual(float v) => max <= v && min <= v;

        public static FloatRange operator +(FloatRange range, float v)
        {
            return new FloatRange(range.value1 + v, range.value2 + v);
        }

        public static FloatRange operator +(FloatRange range, FloatRange v)
        {
            return new FloatRange(range.value1 + v.value1, range.value2 + v.value2);
        }

    }
}