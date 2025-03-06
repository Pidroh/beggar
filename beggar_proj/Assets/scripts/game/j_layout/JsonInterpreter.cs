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
                ld.commons = ReadCommons(layoutMaster, layoutEntry);
                layoutMaster.LayoutDatas.GetOrCreatePointer(ld.Id).data = ld;
                if (layoutEntry.HasKey("children")) 
                {
                    ld.Children = ReadChildren(layoutEntry["children"].Children, layoutMaster);
                }
            }
        }

        private static LayoutCommons ReadCommons(LayoutDataMaster layoutMaster, SimpleJSON.JSONNode layoutEntry)
        {
            var ld = new LayoutCommons();
            foreach (var pair in layoutEntry)
            {
                switch (pair.Key)
                {
                    case "id":
                        ld.Id = pair.Value.AsString;
                        break;
                    case "color_id":
                        ld.ColorReference = layoutMaster.ColorDatas.GetOrCreatePointer(pair.Value.AsString);
                        break;
                    case "axis_mode":
                        ld.AxisModes = ReadAxis(pair.Value.Children);
                        break;
                    case "axis_mode":
                        ld.PositionModes = ReadEnumDoubleArray<PositionMode>(pair.Value.Children);
                        break;
                    case "padding":
                        ld.Padding = ReadPadding(pair.Value.Children);
                        break;
                    case "size":
                        ld.Size = ReadVector2Int(pair.Value.Children);
                        break;
                    case "min_size":
                        ld.MinSize = ReadVector2Int(pair.Value.Children);
                        break;
                    case "step_sizes":
                        ld.StepSizes = ReadArrayOfArrayOfInt(pair.Value.Children);
                        break;
                    default:
                        break;
                }
            }
            return ld;
        }

        private static List<List<int>> ReadArrayOfArrayOfInt(IEnumerable<SimpleJSON.JSONNode> children)
        {
            List<List<int>> ml = new();
            foreach (var item in children)
            {
                var l = new List<int>();
                foreach (var item2 in item.Children)
                {
                    l.Add(item2.AsInt);
                }
                ml.Add(l);
            }
            return ml;
        }

        private static List<LayoutChildData> ReadChildren(IEnumerable<SimpleJSON.JSONNode> children, LayoutDataMaster master)
        {
            foreach (var childEntry in children)
            {
                LayoutChildData childData = new();
                childData.Commons = ReadCommons(master, childEntry);
                if (!EnumHelper<ChildType>.TryGetEnumFromName(childEntry["type"], out var cT)) Debug.LogError("");
                switch (cT)
                {
                    case ChildType.button:
                        childData.ButtonRef = master.ButtonDatas.GetOrCreatePointer(childData.Id);
                        break;
                    case ChildType.text:
                        childData.TextRef = master.TextDatas.GetOrCreatePointer(childData.Id);
                        break;
                    case ChildType.image:
                        break;
                    default:
                        break;
                }

            }
        }

        private static Vector2Int ReadVector2Int(IEnumerable<SimpleJSON.JSONNode> children)
        {
            Vector2Int v = new();
            int i = 0;
            foreach (var item in children)
            {
                if (i == 0) v.x = item.AsInt;
                if (i == 1) v.y = item.AsInt;
                i++;
            }
            return v;
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
                index++;
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
                    Debug.LogError("Enum not existant? " + c.AsString);
                }
                ams[index] = v;
                index++;
            }
            return ams;
        }

        private static T[] ReadEnumDoubleArray<T>(IEnumerable<SimpleJSON.JSONNode> children) where T : System.Enum
        {
            var ams = new T[2];
            var index = 0;
            foreach (var c in children)
            {
                if (!EnumHelper<T>.TryGetEnumFromName(c.AsString, out var v))
                {
                    Debug.LogError("Enum not existant? " + c.AsString);
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
        public PointerHolder<ButtonData> ButtonDatas = new();
        public PointerHolder<TextData> TextDatas = new();




    }

    public class ColorData
    {
    }

    public class LayoutUnit
    {

    }

    public enum ChildType 
    { 
        button, text, image

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

    public enum PositionMode 
    { 
        LEFT_ZERO, RIGHT_ZERO, CENTER, SIBLING_DISTANCE,
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

    public class LayoutCommons 
    {
        public Pointer<ColorData> ColorReference { get; internal set; }
        public RectOffset Padding { get; internal set; }
        public string Id { get; internal set; }
        public PositionMode[] PositionModes { get; internal set; }

        public Vector2Int Size;
        public Vector2Int MinSize;
        public AxisMode[] AxisModes;
        public List<List<int>> StepSizes;
    }

    public class LayoutData
    {
        public string Id => commons.Id;
        public LayoutCommons commons;
        public List<LayoutChildData> Children = new();
    }

    public class ButtonData { }

    public class TextData { }

    public class LayoutChildData
    {
        public LayoutCommons Commons { get; internal set; }
        public string Id => Commons.Id;

        public Pointer<TextData> TextRef { get; internal set; }
        public Pointer<ButtonData> ButtonRef { get; internal set; }
    }
}
