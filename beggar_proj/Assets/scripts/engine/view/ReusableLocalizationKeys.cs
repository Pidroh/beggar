//using UnityEngine.U2D;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace HeartUnity.View
{
    public class ReusableLocalizationKeys
    {
        public static readonly string CST_YES = "Yes";
        public static readonly string CST_NO = "No";

        public static readonly string CST_DELETE_DATA_CONFIRMATION = "Permanently_delete_all_saved_data?";
        public static readonly string CST_CLOSE = "Close";
        public static readonly string CST_GO_WISHLIST = "Go wishlist";

        public static List<string> GetAllCSTs<T>()
        {
            var ret = new List<string>();
            Type myClassType = typeof(T);
            FieldInfo[] fields = myClassType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (FieldInfo field in fields)
            {
                if (field.Name.Contains("CST"))
                {
                    object value = field.GetValue(null);
                    ret.Add((string)value);
                }
            }
            return ret;
        }
    }
}

