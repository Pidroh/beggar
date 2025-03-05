using System;
using System.Collections.Generic;
using UnityEngine;

namespace JLayout {
    public class JsonInterpreter
    {
        public static void ReadJson(string rawJsonText, LayoutDataMaster layoutMaster) 
        {
            var parentNode = SimpleJSON.JSON.Parse(rawJsonText);
            foreach (var item in parentNode)
            {
                if (item.Key == "layouts") 
                {
                    ReadLayouts(item.Value, layoutMaster);
                }
            }
        }

        private static void ReadLayouts(SimpleJSON.JSONNode value, LayoutDataMaster layoutMaster)
        {
            foreach (var layoutEntry in value.Children)
            {
                var ld = new LayoutData();
                foreach (var pair in layoutEntry)
                {
                    switch (pair.Key)
                    {
                        case "id":
                            ld.Id = pair.Value.AsString;
                            layoutMaster.LayoutDatas.GetOrCreatePointer(ld.Id).data = ld;
                            break;
                        case "color_id":
                            ld.ColorReference = layoutMaster.ColorDatas.GetOrCreatePointer(pair.Value.AsString);
                            break;
                        case "axis_mode":
                            ld.AxisModes = ReadAxis(pair.Value.Children);
                            break;
                        case "padding":
                            ld.Padding = ReadPadding(pair.Value.Children);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static RectOffset ReadPadding(IEnumerable<SimpleJSON.JSONNode> children)
        {
            var index = 0;
            var ro = new RectOffset();
            foreach (var item in children)
            {
                var n = item.AsInt;
                if (index == 0) ro.top = n;
                if (index == 1) ro.right = n;
                if (index == 2) ro.bottom = n;
                if (index == 3) ro.left = n;
            }
            return ro;
        }

        private static AxisMode[] ReadAxis(IEnumerable<SimpleJSON.JSONNode> children)
        {
            var ams = new AxisMode[2];
            var index = 0;
            foreach (var c in children)
            {
                if (!EnumHelper<AxisMode>.TryGetEnumFromName(c.AsString, out var v)) 
                {
                    Debug.LogError("Enum not existant? "+c.AsString);
                }
                ams[index] = v;
                index++;
            }
            return ams;
        }
    }

    public class LayoutDataMaster 
    {
        public PointerHolder<LayoutData> LayoutDatas = new();
        public PointerHolder<ColorData> ColorDatas = new();


        

    }

    public class ColorData
    { 
    }

    public class LayoutUnit 
    { 

    }

    public enum AxisMode 
    { 
        // Fill up a percentage of the parent. If size not set, assumes 100%
        PARENT_SIZE_PERCENT, 
        // The size is set by the element, using DPI adaptative pixels
        SELF_SIZE, 
        // The minimum size necessary to contain children (taking into account padding)
        CONTAIN_CHILDREN, 
        // Fills up the space left behind by siblings
        FILL_REMAINING_SIZE, 
        // Use up as much space as necessary by the font and the text. In the case of width, will not go above parent width
        TEXT_PREFERRED
    }

    public class PointerHolder<T>
    {
        public Dictionary<string, Pointer<T>> PointerMap = new();

        public Pointer<T> GetOrCreatePointer(string id)
        {
            if (PointerMap.TryGetValue(id, out var pointer))
            {
            }
            else
            {
                pointer = new Pointer<T>();
                PointerMap[id] = pointer;
            }
            return pointer;
        }

        
    }

    public class Pointer<T>
    {
        public string Id;
        public T data;
    }

    public class LayoutData
    {
        public string Id { get; internal set; }
        public Pointer<ColorData> ColorReference { get; internal set; }
        public RectOffset Padding { get; internal set; }

        public AxisMode[] AxisModes;
    }


}
