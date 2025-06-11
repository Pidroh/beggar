using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace JLayout
{

    public class JsonInterpreter
    {
        public static void ReadJson(string rawJsonText, LayoutDataMaster layoutMaster)
        {
            var parentNode = SimpleJSON.JSON.Parse(rawJsonText);
            foreach (var item in parentNode)
            {
                switch (item.Key)
                {
                    case "general":
                        ReadGeneral(item.Value, layoutMaster);
                        break;
                    case "layouts":
                        ReadLayouts(item.Value, layoutMaster);
                        break;
                    case "buttons":
                        ReadButtons(item.Value, layoutMaster);
                        break;
                    case "texts":
                        ReadTexts(item.Value, layoutMaster);
                        break;
                    case "colors":
                        ReadColors(item.Value, layoutMaster);
                        break;
                    default:
                        break;
                }
            }
            #region test data
#if UNITY_EDITOR
            foreach (var item in layoutMaster.ColorDatas.PointerMap)
            {
                if (item.Value.data == null) 
                {
                    Debug.LogError("MISSING ID for color "+ item.Key);
                }
            }
#endif
            #endregion
        }

        private static void ReadGeneral(SimpleJSON.JSONNode value, LayoutDataMaster layoutMaster)
        {
            foreach (var pair in value)
            {
                switch (pair.Key)
                {
                    case "overlay_background_color":
                        layoutMaster.General.OverlayColor = layoutMaster.ColorDatas.GetOrCreatePointer(pair.Value.AsString);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void ReadColors(SimpleJSON.JSONNode value, LayoutDataMaster layoutMaster)
        {
            foreach (var colorEntry in value.Children)
            {
                var colorData = new ColorData();
                colorData.Id = colorEntry["id"].AsString;
                var colorCodeArray = colorEntry["colors"].AsArray;
                var colors = new Color[colorCodeArray.Count];
                for (int i = 0; i < colors.Length; i++)
                {
                    string colorCode = colorCodeArray[i].AsString;
                    if (!colorCode.Contains("#")) colorCode = "#" + colorCode;
                    if (ColorUtility.TryParseHtmlString(colorCode, out Color c))
                    {
                        colors[i] = c;
                    }
                    else
                    {
                        Debug.LogError("color code is wrong? " + colorCode);
                    }
                }
                colorData.Colors = colors;
                layoutMaster.ColorDatas.Bind(colorData.Id, colorData);
            }
        }

        private static void ReadButtons(SimpleJSON.JSONNode value, LayoutDataMaster layoutMaster)
        {
            foreach (var buttonEntry in value.Children)
            {
                var buttonData = new ButtonData();
                buttonData.LayoutData = ReadLayout(layoutMaster, buttonEntry);
                layoutMaster.ButtonDatas.Bind(buttonData.id, buttonData);
            }
        }

        private static void ReadTexts(SimpleJSON.JSONNode value, LayoutDataMaster layoutMaster)
        {
            foreach (var textEntry in value.Children)
            {
                var textData = new TextData();
                textData.Id = textEntry["id"].AsString;

                textData.ColorSet = CreateColorSet(textEntry, layoutMaster.ColorDatas);
                textData.Size = textEntry["size"].AsInt;

                layoutMaster.TextDatas.Bind(textData.Id, textData);
                if (textEntry.HasKey("alias_ids"))
                {
                    var aliases = textEntry["alias_ids"].AsArray;
                    foreach (var item in aliases.Children)
                    {
                        layoutMaster.TextDatas.Bind(item.AsString, textData);
                    }
                }
            }
        }

        private static readonly Dictionary<string, ColorSetType> ColorBinders = new Dictionary<string, ColorSetType> 
        {
            { "color_id", ColorSetType.NORMAL },
            { "color_id_clicked", ColorSetType.CLICKED },
            { "color_id_pressed", ColorSetType.PRESSED },
            { "color_id_hovered", ColorSetType.HOVERED },
            { "color_id_disabled", ColorSetType.DISABLED },
            { "color_id_active", ColorSetType.ACTIVE },
        };

        private static ColorSet CreateColorSet(SimpleJSON.JSONNode textEntry, PointerHolder<ColorData> colorDatas)
        {
            ColorSet colorSet = null;
            foreach (var item in ColorBinders)
            {
                if (textEntry.HasKey(item.Key))
                {
                    colorSet ??= new ColorSet();
                    colorSet.ColorDatas[item.Value] = colorDatas.GetOrCreatePointer(textEntry[item.Key].AsString);
                }
            }
            
            return colorSet;
        }

        private static void ReadLayouts(SimpleJSON.JSONNode value, LayoutDataMaster layoutMaster)
        {
            foreach (var layoutEntry in value.Children)
            {
                LayoutData ld = ReadLayout(layoutMaster, layoutEntry);
                layoutMaster.LayoutDatas.Bind(ld.Id, ld);
            }
        }

        private static LayoutData ReadLayout(LayoutDataMaster layoutMaster, SimpleJSON.JSONNode layoutEntry)
        {
            var ld = new LayoutData();
            ld.commons = ReadCommons(layoutMaster, layoutEntry);
            if (layoutEntry.HasKey("fixed_children"))
            {
                ld.Children = ReadChildren(layoutEntry["fixed_children"].Children, layoutMaster);
            }
            if (layoutEntry.HasKey("clickable"))
            {
                ld.Clickable = layoutEntry["clickable"].AsBool;
                
            }

            return ld;
        }

        private static LayoutCommons ReadCommons(LayoutDataMaster layoutMaster, SimpleJSON.JSONNode layoutEntry)
        {
            var ld = new LayoutCommons();
            ld.ColorSet = CreateColorSet(layoutEntry, layoutMaster.ColorDatas);
            foreach (var pair in layoutEntry)
            {
                switch (pair.Key)
                {
                    case "id":
                        ld.Id = pair.Value.AsString;
                        break;
                    case "axis_mode":
                        ld.AxisModes = ReadAxis(pair.Value.Children);
                        break;
                    case "position_mode":
                        ld.PositionModes = ReadEnumDoubleArray<PositionMode>(pair.Value.Children);
                        break;
                    case "position":
                        ld.PositionOffsets = ReadVector2Int(pair.Value.Children);
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
                    case "text_horizontal":
                        ld.TextHorizontalMode = EnumHelper<TextHorizontal>.TryGetEnumFromName(pair.Value.AsString, out var mode) ? mode : mode;
                        break;
                    case "use_layout_commons":
                        ld.UseLayoutCommons = pair.Value.AsBool;
                        break;
                    default:
                        break;
                }
            }
            // ld.PositionModes ??= new PositionMode[2] { PositionMode.CENTER, PositionMode.CENTER };
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
            List<LayoutChildData> l = new();
            foreach (var childEntry in children)
            {
                LayoutChildData childData = new();
                l.Add(childData);
                childData.Commons = ReadCommons(master, childEntry);
                if (!EnumHelper<ChildType>.TryGetEnumFromName(childEntry["type"], out var cT)) Debug.LogError("");
                switch (cT)
                {
                    case ChildType.layout:
                        childData.LayoutRef = master.LayoutDatas.GetOrCreatePointer(childData.Id);
                        break;
                    case ChildType.button:
                        childData.ButtonRef = master.ButtonDatas.GetOrCreatePointer(childData.Id);
                        break;
                    case ChildType.text:
                        childData.TextRef = master.TextDatas.GetOrCreatePointer(childData.Id);
                        break;
                    case ChildType.image:
                        childData.ImageKey = childEntry["image"].AsString;
                        break;
                }

            }
            return l;
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
        public GeneralData General = new();
        public PointerHolder<LayoutData> LayoutDatas = new();
        public PointerHolder<ColorData> ColorDatas = new();
        public PointerHolder<ButtonData> ButtonDatas = new();
        public PointerHolder<TextData> TextDatas = new();
    }

    public class GeneralData 
    {
        public Pointer<ColorData> OverlayColor;
    }

    public class ColorSet 
    {
        public Dictionary<ColorSetType, Pointer<ColorData>> ColorDatas = new();
    }

    public class ColorSetResolved
    {
        public Dictionary<ColorSetType, ColorData> ColorDatas = new();
    }

    public enum ColorSetType 
    {
        NORMAL, CLICKED, ACTIVE, DISABLED, HOVERED, PRESSED
    }

    public class ColorData
    {
        public Color[] Colors { get; internal set; }
        public string Id { get; internal set; }
    }

    public class LayoutUnit
    {

    }

    public enum ChildType
    {
        button, text, image, layout
    }

    public enum TextHorizontal
    {
        RIGHT, LEFT, CENTER
    }

    public enum AxisMode
    {
        // Fill up a percentage of the parent. If size not set, assumes 100% (actually always 100% for now)
        PARENT_SIZE_PERCENT,
        // The size is set by the element, using DPI adaptative pixels
        SELF_SIZE,
        // The minimum size necessary to contain children (taking into account padding)
        CONTAIN_CHILDREN,
        // Fills up the space left behind by siblings
        FILL_REMAINING_SIZE,
        // Use up as much space as necessary by the font and the text. Not applicable to width
        TEXT_PREFERRED,
        // Similar to text preferred, but will over extend to use the step sizes depending on text size
        STEP_SIZE_TEXT,
        // PARENT_SIZE_PERCENT, but ignores padding
        PARENT_SIZE_PERCENT_RAW
    }

    public enum PositionMode
    {
        // The origin is on the left, only for X
        LEFT_ZERO, 
        // origin on the right, so the right side aligns with right side of parent
        RIGHT_ZERO, 
        // center
        CENTER, 
        // center ignoring padding
        CENTER_RAW, 
        // aligns a side of the thing with the opposite side of the previous sibling (left with right or top with bottom)
        SIBLING_DISTANCE,
        // same as above, but the sides are inverted
        SIBLING_DISTANCE_REVERSE, 
        // positioning with a pivot that fits a gauge. Ignores padding
        RAW_FOR_GAUGE, 
        // same as above but does not ignore padding
        FOR_GAUGE,
        // the origin is on the top, only for Y
        TOP_ZERO,
        // origin bottom, y only
        BOTTOM_ZERO,
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
                pointer.Id = id;
                PointerMap[id] = pointer;
            }
            return pointer;
        }

        internal void Bind(string id, T buttonData)
        {
            var p = GetOrCreatePointer(id);
            p.Id = id;
            if (p.data != null) 
            {
                Debug.LogError("data was bound twice, repeated id? "+id);
            }
            p.data = buttonData;
        }

        internal T GetData(string id)
        {
            return PointerMap[id].data;
        }
    }

    public class Pointer<T>
    {
        public string Id;
        public T data;
    }

    public class LayoutData
    {
        public string Id => commons.Id;

        public bool Clickable { get; internal set; }

        public LayoutCommons commons;
        public List<LayoutChildData> Children = new();
    }

    public class ButtonData
    {
        public string id => LayoutData.Id;
        public LayoutData LayoutData { get; internal set; }
    }

    public class TextData
    {
        public ColorSet ColorSet { get; internal set; }
        public int Size { get; internal set; }
        public string Id { get; internal set; }

        internal Color GetNormalColor(int currentColorSchemeId) => ColorSet.ColorDatas[ColorSetType.NORMAL].data.Colors[currentColorSchemeId];
    }

    public class LayoutChildData
    {
        public LayoutCommons Commons { get; internal set; }
        public string Id => Commons.Id;
        public Pointer<LayoutData> LayoutRef;
        public Pointer<TextData> TextRef { get; internal set; }
        public Pointer<ButtonData> ButtonRef { get; internal set; }
        public string ImageKey { get; internal set; }
    }
}
