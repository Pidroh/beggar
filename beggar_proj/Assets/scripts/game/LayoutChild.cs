using HeartUnity.View;
using System.Collections.Generic;
using UnityEngine;

public class ButtonWithExpandable 
{
    public UIUnit MainButton;
    public IconButton ExpandButton;
    public LayoutChild LayoutChild;
    public List<GameObject> ExpandTargets = new();
    
}

public class LayoutChild 
{
    public RectTransform RectTransform;
    public Vector2Null FixedSize;
}

public struct Vector2Null {
    public float? X;
    public float? Y;
}   