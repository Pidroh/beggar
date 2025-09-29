using System;
using TMPro;
using UnityEngine;

namespace HeartUnity.View
{
    public static class TextMeshProExtensions 
    {
        public static void SetFontSizePhysical(this TextMeshProUGUI text, int referenceFontSize) {
            text.fontSize = referenceFontSize * RectTransformExtensions.DpiScaleFromDefault;
        }
    }
    public static class RectTransformExtensions
    {
        public const float MilimiterToPixelFallback = 96f / 25.4f;
        public const float PixelToMilimiterFallback = 25.4f / 96f;
        public static float DpiScaleFromDefault => EngineView.dpi / 96f;
        public static float DefaultPixelSizeToPhysicalPixelSize => PixelToMilimiterFallback * RectTransformExtensions.MilimeterToPixel;

        public static float MilimeterToPixel => EngineView.dpi <= 0 ? MilimiterToPixelFallback : EngineView.dpi / 25.4f;
        public static float PixelToMilimeter => EngineView.dpi <= 0 ? PixelToMilimiterFallback : 25.4f / EngineView.dpi;
        public static void AnchorToCorners(this RectTransform transform)
        {
            if (transform == null)
                throw new ArgumentNullException("transform");

            if (transform.parent == null)
                return;

            var parent = transform.parent.GetComponent<RectTransform>();

            Vector2 newAnchorsMin = new Vector2(transform.anchorMin.x + transform.offsetMin.x / parent.rect.width,
                              transform.anchorMin.y + transform.offsetMin.y / parent.rect.height);

            Vector2 newAnchorsMax = new Vector2(transform.anchorMax.x + transform.offsetMax.x / parent.rect.width,
                              transform.anchorMax.y + transform.offsetMax.y / parent.rect.height);

            transform.anchorMin = newAnchorsMin;
            transform.anchorMax = newAnchorsMax;
            transform.offsetMin = transform.offsetMax = new Vector2(0, 0);
        }

        public static void LimitDPIBasedOnPhysicalScreenAdjustedWidth(float maxAdjustedWidth) 
        {
            var maximumDpi = (Screen.width / maxAdjustedWidth) * 96f;
            EngineView.SetMaxDpi(maximumDpi);
        }

        public static void SetOffsets(this RectTransform trans, RectOffset offsets)
        {
            int left = offsets.left;
            int bottom = offsets.bottom;

            int right = -offsets.right;
            int top = -offsets.top;
            SetOffsets(trans, left, bottom, right, top);
        }

        public static void SetOffsets(this RectTransform trans, int left, int bottom, int right, int top)
        {
            trans.offsetMin = new Vector2(left, bottom);
            trans.offsetMax = new Vector2(right, top);
        }

        public static void SetOffsets(this RectTransform trans, int allValues) => SetOffsets(trans, allValues, allValues, allValues, allValues);

        public static void SetPivotAndAnchors(this RectTransform trans, Vector2 aVec)
        {
            trans.pivot = aVec;
            trans.anchorMin = aVec;
            trans.anchorMax = aVec;
        }

        public static Vector2 GetSize(this RectTransform trans)
        {
            return trans.rect.size;
        }

        public static float GetWidth(this RectTransform trans)
        {
            return trans.rect.width;
        }

        public static float GetHeight(this RectTransform trans)
        {
            return trans.rect.height;
        }

        public static float GetWidthMilimeters(this RectTransform trans) => GetWidth(trans) * PixelToMilimeter;
        public static float GetHeightMilimeters(this RectTransform trans) => GetHeight(trans) * PixelToMilimeter;

        public static void SetSize(this RectTransform trans, Vector2 newSize)
        {
            Vector2 oldSize = trans.rect.size;
            Vector2 deltaSize = newSize - oldSize;
            trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
            trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
        }

        public static void SetWidthMilimeters(this RectTransform trans, float sizeMM, int minSize = int.MaxValue) => trans.SetWidth(sizeMM * MilimeterToPixel > minSize ? minSize : sizeMM * MilimeterToPixel);

        public static void SetHeightMilimeters(this RectTransform trans, float sizeMM) => trans.SetHeight(sizeMM * MilimeterToPixel);

        public static void SetSizeMilimeters(this RectTransform trans, int index, float sizeMM) 
        {
            if (index == 0) trans.SetWidthMilimeters(sizeMM);
            if (index == 1) trans.SetHeightMilimeters(sizeMM);
        }

        public static void SetWidth(this RectTransform trans, float newSize)
        {
            SetSize(trans, new Vector2(newSize, trans.rect.size.y));
        }

        public static void SetHeight(this RectTransform trans, float newSize)
        {
            SetSize(trans, new Vector2(trans.rect.size.x, newSize));
        }

        public static void SetBottomYToParent(this RectTransform trans, float distance)
        {
            var index = 1;
            var anchor = 0f;
            // make position relative to bottom of parent
            trans.SetAnchorsByIndex(index, anchor);
            var deltaOffsetY = trans.offsetMax.y - trans.offsetMin.y;
            trans.offsetMin = new Vector2(trans.offsetMin.x, 0 + distance);
            trans.offsetMax = new Vector2(trans.offsetMax.x, deltaOffsetY + distance);
            //trans.SetBottomLocalY(bottomY);
        }

        public static void SetTopYToParent(this RectTransform trans, float distance)
        {
            var index = 1;
            var anchor = 1f;
            // make position relative to bottom of parent
            trans.SetAnchorsByIndex(index, anchor);
            var deltaOffsetY = trans.offsetMax.y - trans.offsetMin.y;
            var disMin = -distance - trans.GetSize()[index]; //; //- trans.GetSize()[index];
            trans.offsetMin = new Vector2(trans.offsetMin.x, disMin);
            trans.offsetMax = new Vector2(trans.offsetMax.x, deltaOffsetY + disMin);
        }

        public static void SetLeftXToParent(this RectTransform trans, float distance)
        {
            var index = 0;
            trans.SetAnchorsByIndex(index, 0);
            var deltaOffsetX = trans.offsetMax[index] - trans.offsetMin[index];
            trans.offsetMin = new Vector2(0 + distance, trans.offsetMin.y);
            trans.offsetMax = new Vector2(deltaOffsetX + distance, trans.offsetMax.y);
        }

        public static void SetCenterXToParent(this RectTransform trans, float distance)
        {
            var index = 0;
            trans.SetAnchorsByIndex(index, 0.5f);
            
            var deltaOffsetX = trans.offsetMax[index] - trans.offsetMin[index];
            trans.offsetMin = new Vector2(0 + distance - deltaOffsetX*0.5f, trans.offsetMin.y);
            trans.offsetMax = new Vector2(deltaOffsetX * 0.5f + distance, trans.offsetMax.y);
        }

        public static void SetRightXToParent(this RectTransform trans, float distance)
        {
            var index = 0;
            // make position relative to bottom of parent
            trans.SetAnchorsByIndex(index, 1);
            var deltaOffset = trans.offsetMax[index] - trans.offsetMin[index];
            var size = trans.GetSize()[index];
            trans.offsetMin = new Vector2(0 - distance - size, trans.offsetMin.y);
            trans.offsetMax = new Vector2(deltaOffset - distance - size, trans.offsetMax.y);
            //trans.SetBottomLocalY(bottomY);
        }

        public static void SetPivotByIndex(this RectTransform trans, int index, float value) 
        {
            var pivot = trans.pivot;
            pivot[index] = value;
            trans.pivot = pivot;
        }
        public static void SetAnchorsByIndex(this RectTransform trans, int index, float value)
        {
            var am = trans.anchorMin;
            var amM = trans.anchorMax;
            am[index] = value;
            amM[index] = value;
            trans.anchorMin = am;
            trans.anchorMax = amM;
        }

        public static void SetAnchorMaxByIndex(this RectTransform trans, int index, float value)
        {
            var amM = trans.anchorMax;
            amM[index] = value;
            trans.anchorMax = amM;
        }

        public static void SetAnchorMinByIndex(this RectTransform trans, int index, float value)
        {
            var amM = trans.anchorMin;
            amM[index] = value;
            trans.anchorMin = amM;
        }

        public static void SetBottomLocalY(this RectTransform trans, float bottomY)
        {
            trans.localPosition = new Vector3(trans.localPosition.x, bottomY + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
        }

        public static void SetTopLocalY(this RectTransform trans, float newPos)
        {
            trans.localPosition = new Vector3(trans.localPosition.x, newPos - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
        }

        public static void SetBottomLeftLocalPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
        }

        public static void SetLeftLocalX(this RectTransform trans, float newPos)
        {
            trans.localPosition = new Vector3(newPos + (trans.pivot.x * trans.rect.width), trans.localPosition.y, trans.localPosition.z);
        }

        public static void SetCenterLocalAxis(this RectTransform trans, int axis, float newPos)
        {
            var lp = trans.localPosition;
            lp[axis] = newPos + ((trans.pivot[axis] - 0.5f) * trans.GetSize()[axis]);
            trans.localPosition = lp;
        }

        public static float GetRightLocalX(this RectTransform trans)
        {
            return trans.localPosition.x + trans.pivot.x * trans.rect.width;
        }

        public static float GetLeftLocalX(this RectTransform trans)
        {
            return trans.localPosition.x - (1 - trans.pivot.x) * trans.rect.width;
        }

        public static float GetBottomLocalY(this RectTransform trans)
        {
            return trans.localPosition.y - (1 - trans.pivot.y) * trans.rect.height;
        }

        public static float GetTopLocalY(this RectTransform trans)
        {
            return trans.localPosition.y + trans.pivot.y * trans.rect.height;
        }

        public static void SetRightLocalX(this RectTransform trans, float newPos)
        {
            trans.localPosition = new Vector3(newPos - ((1f - trans.pivot.x) * trans.rect.width), trans.localPosition.y, trans.localPosition.z);
        }

        public static void SetLocalX(this RectTransform trans, float newPos)
        {
            trans.localPosition = new Vector3(newPos, trans.localPosition.y, trans.localPosition.z);
        }

        public static void SetLocalY(this RectTransform trans, float newPos)
        {
            trans.localPosition = new Vector3(trans.localPosition.x, newPos, trans.localPosition.z);
        }

        public static void SetLocalXY(this RectTransform trans, float newPosX, float newPosY)
        {
            trans.localPosition = new Vector3(newPosX, newPosY, trans.localPosition.z);
        }

        public static void SetTopLeftLocalPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
        }

        public static void SetBottomRightLocalPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
        }

        public static void SetRightTopLocalPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
        }

        public static void SetOffsetMaxByIndex(this RectTransform trans, int index, float v)
        {
            var offsets =  trans.offsetMax;
            offsets[index] = v;
            trans.offsetMax = offsets;
        }

        public static void SetOffsetMinByIndex(this RectTransform trans, int index, float value)
        {
            var offsets = trans.offsetMin;
            offsets[index] = value;
            trans.offsetMin = offsets;
        }


        public static void FillParent(this RectTransform trans)
        {
            trans.anchorMin = new Vector2(0, 0);
            trans.anchorMax = new Vector2(1, 1);
            trans.offsetMin = Vector2.zero;
            trans.offsetMax = Vector2.zero;
            
        }

        public static void FillParentWidth(this RectTransform trans) => FillParentByAxisIndex(trans, 0);
        public static void FillParentHeight(this RectTransform trans) => FillParentByAxisIndex(trans, 1);

        public static void FillParentByAxisIndex(this RectTransform trans, int axisIndex)
        {
            trans.SetAnchorMinByIndex(axisIndex, 0f);
            trans.SetAnchorMaxByIndex(axisIndex, 1f);
            trans.SetOffsetMinByIndex(axisIndex, 0f);
            trans.SetOffsetMaxByIndex(axisIndex, 0f);
        }

        public static RectTransform CreateFullSizeChild(this RectTransform trans, string name) 
        {
            GameObject childGO = new GameObject(name);
            childGO.transform.SetParent(trans, false);
            RectTransform rectTransform = childGO.AddComponent<RectTransform>();
            rectTransform.FillParent();
            return rectTransform;
        }
    }
}