#nullable enable

using System;
using System.Numerics;

namespace HeartEngineCore
{
    /// <summary>
    /// Lightweight clone of UnityEngine.Random that avoids Unity dependencies.
    /// </summary>
    public static class RandomHG
    {
        private static readonly object SyncRoot = new object();
        private static Random _random = new Random();

        /// <summary>
        /// Re-seeds the internal generator to make the sequence deterministic.
        /// </summary>
        public static void InitState(int seed)
        {
            lock (SyncRoot)
            {
                _random = new Random(seed);
            }
        }

        /// <summary>
        /// Returns a random float between 0.0 [inclusive] and 1.0 [inclusive].
        /// </summary>
        public static float value => Range(0f, 1f);

        /// <summary>
        /// Returns a random int between minInclusive [inclusive] and maxExclusive [exclusive].
        /// Matches UnityEngine.Random.Range(int, int) behaviour.
        /// </summary>
        public static int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            lock (SyncRoot)
            {
                return _random.Next(minInclusive, maxExclusive);
            }
        }

        /// <summary>
        /// Returns a random float between minInclusive [inclusive] and maxInclusive [inclusive].
        /// </summary>
        public static float Range(float minInclusive, float maxInclusive)
        {
            if (maxInclusive <= minInclusive)
            {
                return minInclusive;
            }

            var t = NextDoubleInclusive();
            return (float)(minInclusive + (maxInclusive - minInclusive) * t);
        }

        /// <summary>
        /// Returns a uniformly distributed point inside a unit circle.
        /// </summary>
        public static Vector2 insideUnitCircle
        {
            get
            {
                var angle = 2.0 * Math.PI * NextDoubleInclusive();
                var radius = Math.Sqrt(NextDoubleInclusive());
                return new Vector2(
                    (float)(Math.Cos(angle) * radius),
                    (float)(Math.Sin(angle) * radius));
            }
        }

        /// <summary>
        /// Returns a uniformly distributed point inside a unit sphere.
        /// </summary>
        public static Vector3 insideUnitSphere
        {
            get
            {
                while (true)
                {
                    var point = new Vector3(
                        Range(-1f, 1f),
                        Range(-1f, 1f),
                        Range(-1f, 1f));

                    if (point.LengthSquared() <= 1f)
                    {
                        return point;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a uniformly distributed point on the surface of a unit sphere.
        /// </summary>
        public static Vector3 onUnitSphere
        {
            get
            {
                var u = NextDoubleInclusive();
                var v = NextDoubleInclusive();
                var theta = 2.0 * Math.PI * u;
                var z = 2.0 * v - 1.0;
                var r = Math.Sqrt(Math.Max(0.0, 1.0 - z * z));

                return new Vector3(
                    (float)(r * Math.Cos(theta)),
                    (float)(r * Math.Sin(theta)),
                    (float)z);
            }
        }

        /// <summary>
        /// Returns a uniformly distributed random rotation.
        /// </summary>
        public static Quaternion rotation => RandomRotation();

        /// <summary>
        /// Alias of rotation, matches UnityEngine.Random.rotationUniform.
        /// </summary>
        public static Quaternion rotationUniform => RandomRotation();

        private static Quaternion RandomRotation()
        {
            // Algorithm from "Uniform Random Rotations", Ken Shoemake, Graphics Gems III.
            var u1 = NextDoubleInclusive();
            var u2 = NextDoubleInclusive();
            var u3 = NextDoubleInclusive();

            var sqrt1MinusU1 = Math.Sqrt(1.0 - u1);
            var sqrtU1 = Math.Sqrt(u1);

            var theta1 = 2.0 * Math.PI * u2;
            var theta2 = 2.0 * Math.PI * u3;

            var x = (float)(sqrt1MinusU1 * Math.Sin(theta1));
            var y = (float)(sqrt1MinusU1 * Math.Cos(theta1));
            var z = (float)(sqrtU1 * Math.Sin(theta2));
            var w = (float)(sqrtU1 * Math.Cos(theta2));

            return new Quaternion(x, y, z, w);
        }

        private static double NextDoubleInclusive()
        {
            // System.Random.NextDouble() is exclusive of 1.0, so use an int sample to reach [0,1].
            lock (SyncRoot)
            {
                return _random.Next(int.MaxValue) / (double)(int.MaxValue - 1);
            }
        }
    }
}
