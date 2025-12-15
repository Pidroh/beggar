using System.Collections.Generic;
using UnityEngine;

namespace JLayout
{
    public class LayoutCommons
    {
        public static RectOffset ZeroOffset = new RectOffset(0, 0, 0, 0);
        public RectOffset _padding;
        public RectOffset Padding { get => _padding ?? ZeroOffset; set => _padding = value; }
        public string Id { get; internal set; }
        public PositionMode[] PositionModes { get; internal set; }
        public TextHorizontal TextHorizontalMode { get; internal set; } = TextHorizontal.LEFT;
        public Vector2Int PositionOffsets { get; internal set; }
        public ColorSet ColorSet { get; internal set; }
        public bool UseLayoutCommons { get; internal set; }
        public bool IgnoreLayout { get; internal set; }

        public Vector2Int Size;
        public Vector2Int MinSize;
        public AxisMode[] AxisModes;
        public List<List<int>> StepSizes;
    }
}
