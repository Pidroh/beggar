using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeartUnity.View
{
    [Serializable]
    public class DragAndDropScheme
    {
        public List<UIUnit> movables = new List<UIUnit>();
        public List<UIUnit> areas = new List<UIUnit>();
    }

    [Serializable]
    public class DragAndDropManager
    {
        public List<DragAndDropScheme> schemes = new List<DragAndDropScheme>();
        public UIUnit draggingElement;

        public bool DraggingActive;

        private DragAndDropScheme activeScheme;
        public UIUnit resultArea;
        public UIUnit resultDraggable;
        public bool HasResult => resultArea != null && resultDraggable != null;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        public void Update()
        {
            if (!DraggingActive)
            {
                foreach (var scheme in schemes)
                {
                    foreach (var item in scheme.movables)
                    {
                        if (item.MouseDownThisFrame)
                        {
                            draggingElement = item;
                            DraggingActive = true;
                            activeScheme = scheme;
                            resultArea = null;
                            resultDraggable = null;
                        }
                    }

                }
            }
            if (DraggingActive)
            {
                draggingElement.transform.position = InputManager.CanvasMousePosition;

                if (draggingElement.MouseUpThisFrame)
                {
                    foreach (var item in activeScheme.areas)
                    {
                        if (item.CheckMouseInside())
                        {
                            resultArea = item;
                            resultDraggable = draggingElement;
                        }
                    }
                    DraggingActive = false;
                    //draggingElement = null;
                }
            }
            if (!Input.GetMouseButton(0))
            {
                DraggingActive = false;
                draggingElement = null;
            }
        }

        public void ClearResult()
        {
            resultArea = null;
            resultDraggable = null;
        }
    }
}