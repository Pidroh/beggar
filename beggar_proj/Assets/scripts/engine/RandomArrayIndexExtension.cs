using System.Collections.Generic;
using UnityEngine;

namespace HeartUnity
{

    public static class RandomArrayIndexExtension
    {
        public static T RandomElement<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        public static T RandomElement<T>(this List<T> array)
        {
            return array[Random.Range(0, array.Count)];
        }

        public static T RandomTake<T>(this List<T> array)
        {
            int index = Random.Range(0, array.Count);
            T element = array[index];
            array.RemoveAt(index);
            return element;
        }

        public static void Shuffle<T>(this IList<T> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }
    }

}