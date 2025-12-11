using HeartEngineCore;
using System.Collections.Generic;

namespace HeartEngineCore
{

    public static class RandomArrayIndexExtension
    {
        public static T RandomElement<T>(this T[] array)
        {
            return array[RandomHG.Range(0, array.Length)];
        }

        public static T RandomElement<T>(this List<T> array)
        {
            return array[RandomHG.Range(0, array.Count)];
        }

        public static T RandomTake<T>(this List<T> array)
        {
            int index = RandomHG.Range(0, array.Count);
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
                var r = RandomHG.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }
    }

}