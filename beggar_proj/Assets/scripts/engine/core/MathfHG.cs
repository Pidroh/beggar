#nullable enable

using System;

namespace HeartEngineCore
{
    /// <summary>
    /// Minimal substitute for UnityEngine.Mathf without Unity dependency.
    /// </summary>
    public static class Mathf
    {
        public const float PI = (float)Math.PI;
        public const float Deg2Rad = PI / 180f;
        public const float Rad2Deg = 180f / PI;
        public const float Epsilon = float.Epsilon;

        public static float Abs(float f) => Math.Abs(f);
        public static int Abs(int value) => Math.Abs(value);

        public static float Min(float a, float b) => Math.Min(a, b);
        public static int Min(int a, int b) => Math.Min(a, b);

        public static float Max(float a, float b) => Math.Max(a, b);
        public static int Max(int a, int b) => Math.Max(a, b);

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float Clamp01(float value) => Clamp(value, 0f, 1f);

        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);

        public static float Pow(float f, float p) => (float)Math.Pow(f, p);

        public static float Sqrt(float f) => (float)Math.Sqrt(f);

        public static float Floor(float f) => (float)Math.Floor(f);
        public static int FloorToInt(float f) => (int)Math.Floor(f);

        public static float Ceil(float f) => (float)Math.Ceiling(f);
        public static int CeilToInt(float f) => (int)Math.Ceiling(f);

        public static float Round(float f) => (float)Math.Round(f);
        public static int RoundToInt(float f) => (int)Math.Round(f);

        public static float Sign(float f) => f < 0f ? -1f : 1f;
    }
}
