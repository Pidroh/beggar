using UnityEngine;
using UnityEngine.UI;

namespace HeartUnity.View
{
    public class EngineView : MonoBehaviour
    {
        public Canvas canvas;
        public struct EngineViewInitializationParameter
        {
            public Canvas canvas;
        }

        // access this through an instance of heart game
        internal static EngineView CreateEngineViewThroughCode(EngineViewInitializationParameter param)
        {
            var go = new GameObject();
            var ev = go.AddComponent<EngineView>();
            ev.canvas = param.canvas;
            ev.canvasScaler = ev.canvas.GetComponent<CanvasScaler>();
            return ev;
        }

        public CanvasScaler canvasScaler;
        public UIUnitManager unitManager;
        public UICreationHelper creationHelper;
        private DebugMenuManager debugMenuManager;
        public GameObject prefabHolder;
        public AnimationUnitProcessor animationUnitProcessor = new();
        public SpriteAnimationProcessor spriteAnimProcessor = new();
        public PixelPerfectViewManager pixelPerfectManager;
        public SelectableManager selectableManager = new();
        public InputManager inputManager = new();
        private MouseAsSpriteInfo mouseView;
        public ReusableMenuPrefabs reusableMenuPrefabs;

        public static float dpi => overwrittenDpi.HasValue ? overwrittenDpi.Value : Screen.dpi;
        private static float? overwrittenDpi;
        private static float? previousDpi;
        public static bool DpiChanged;
        private bool pixelPerfectScale;
        public bool DisabledAutoScaling;

        internal void Init(int initialLayer)
        {
            UIUnit.EngineView = this;
            if(prefabHolder != null)
                prefabHolder.SetActive(false);
            creationHelper = new UICreationHelper(unitManager, initialLayer);
            debugMenuManager = new DebugMenuManager();
            DebugMenuManager.Instance = debugMenuManager;
            selectableManager.inputManager = inputManager;
            inputManager.ApplyPreviousSceneLatestDevice();
            RefreshScale();
            var config = HeartGame.GetConfig();
            if (config == null) return;
            if (config.viewConfig.cursorView != null && unitManager != null)
            {
                var previousLayer = creationHelper.currentLayer;
                creationHelper.currentLayer = unitManager.HighestLayer - 1;
                var cursorView = creationHelper.InstantiateObject(config.viewConfig.cursorView);
                selectableManager.cursorManager.SetCursor(cursorView);
                creationHelper.currentLayer = previousLayer;
                cursorView.gameObject.SetActive(false);
            }
            var mouse = config.viewConfig.mouseAsSprite;
            if(mouse != null && unitManager != null){
                var previousLayer = creationHelper.currentLayer;
                creationHelper.currentLayer = unitManager.HighestLayer - 1;
                mouseView = creationHelper.InstantiateObject(config.viewConfig.mouseAsSprite);
                creationHelper.currentLayer = previousLayer;
            }
        }

        

        public static bool IsVisibleOnCamera(RectTransform target)
        {
            return EngineView.IsFullyVisibleFrom(target, Camera.main);
        }

        public void PlayAnimation(AnimationUnitList anim)
        {
            animationUnitProcessor.Play(anim, null);
        }

        public void PlayAnimation(AnimationUnitList anim, AnimationUnitList.RealtimeAnimationConfig rtConfig)
        {
            animationUnitProcessor.Play(anim, rtConfig);
        }

        public void PlayAnimation(SpriteAnimation anim)
        {
            spriteAnimProcessor.Play(anim);
        }

        public void ChangeCursorLayer(int layer)
        {
            var newParent = unitManager.layerParents[layer];
            selectableManager.cursorManager.cursorView.transform.parent = newParent.transform;
        }

        public static T GetNextSiblingOrNearbySibling<T>(Transform transform) where T : MonoBehaviour
        {
            // Get the index of the current transform in its parent's child list
            int index = transform.GetSiblingIndex();

            // Check siblings after this one
            if (index + 1 < transform.parent.childCount)
            {
                Transform nextSibling = transform.parent.GetChild(index + 1);
                T component = nextSibling.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }

            // Check siblings before this one
            if (index > 0)
            {
                Transform previousSibling = transform.parent.GetChild(index - 1);
                T component = previousSibling.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }

            // No matching sibling found
            return null;
        }

        public void ManualUpdate()
        {
            animationUnitProcessor.Update(Time.deltaTime);
            spriteAnimProcessor.ManualUpdate(Time.deltaTime);
            

            if (debugMenuManager != null)
                debugMenuManager.ManualUpdate();
            inputManager.ManualUpdate();
            selectableManager.ManualUpdate();
            if (mouseView != null)
            {
                Vector3 mousePosition = Input.mousePosition;

                
                var mouseInsideScreen =  mousePosition.x >= 0 && mousePosition.x <= Screen.width &&
                       mousePosition.y >= 0 && mousePosition.y <= Screen.height;
                Cursor.visible = !mouseInsideScreen;
                mouseView.gameObject.SetActive(this.inputManager.LatestInputDevice == InputManager.InputDevice.MOUSE);
                Vector3 pos = GetCanvasMousePosition();
                mouseView.transform.position = pos;
            }
            InputManager.CanvasMousePosition = GetCanvasMousePosition();
            RefreshScale();
        }

        public Vector3 GetCanvasMousePosition()
        {
            if (mouseView == null) return Input.mousePosition;
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = mouseView.transform.parent.position.z - Camera.main.transform.position.z;
            var pos = Camera.main.ScreenToWorldPoint(mousePosition);
            pos.z = mouseView.transform.parent.position.z;
            return pos;
        }

        private void RefreshScale()
        {
            if (pixelPerfectScale || DisabledAutoScaling) return;
            var maxScale = 1;
            int scaleX = Mathf.FloorToInt(Screen.width / 640);
            int scaleY = Mathf.FloorToInt(Screen.height / 360);
            if (scaleX > maxScale)
            {
                maxScale = scaleX;
            }
            if (scaleY > maxScale)
            {
                maxScale = scaleY;
            }
            canvasScaler.scaleFactor = maxScale;
            canvas.scaleFactor = maxScale;
        }

        /// <summary>
        /// Counts the bounding box corners of the given RectTransform that are visible from the given Camera in screen space.
        /// </summary>
        /// <returns>The amount of bounding box corners that are visible from the Camera.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        private static int CountCornersVisibleFrom(RectTransform rectTransform, Camera camera)
        {
            Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
            Vector3[] objectCorners = new Vector3[4];
            rectTransform.GetWorldCorners(objectCorners);

            int visibleCorners = 0;
            //Vector3 tempScreenSpaceCorner; // Cached
            for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
            {
                //tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
                //if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
                if (screenBounds.Contains(objectCorners[i])) // If the corner is inside the screen
                {
                    visibleCorners++;
                }
            }
            return visibleCorners;
        }

        /// <summary>
        /// Determines if this RectTransform is fully visible from the specified camera.
        /// Works by checking if each bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
        /// </summary>
        /// <returns><c>true</c> if is fully visible from the specified camera; otherwise, <c>false</c>.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        public static bool IsFullyVisibleFrom(RectTransform rectTransform, Camera camera)
        {
            return CountCornersVisibleFrom(rectTransform, camera) == 4; // True if all 4 corners are visible
        }

        /// <summary>
        /// Determines if this RectTransform is at least partially visible from the specified camera.
        /// Works by checking if any bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
        /// </summary>
        /// <returns><c>true</c> if is at least partially visible from the specified camera; otherwise, <c>false</c>.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        public static bool IsVisibleFrom(RectTransform rectTransform, Camera camera)
        {
            return CountCornersVisibleFrom(rectTransform, camera) > 0; // True if any corners are visible
        }

        public static float GetDistanceFromScreenCorner(RectTransform rectTransform, RectTransform screenRect = null)
        {
            Vector3[] objectCorners = new Vector3[4];
            Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
            if (screenRect != null) {
                screenRect.GetWorldCorners(objectCorners);
                screenBounds = new Rect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
                for (int i = 0; i < objectCorners.Length; i++)
                {
                    screenBounds.xMin = Mathf.Min(objectCorners[i][0], screenBounds.xMin);
                    screenBounds.yMin = Mathf.Min(objectCorners[i][1], screenBounds.yMin);
                    screenBounds.xMax = Mathf.Max(objectCorners[i][0], screenBounds.xMax);
                    screenBounds.yMax = Mathf.Max(objectCorners[i][1], screenBounds.yMax);
                }
            }
            rectTransform.GetWorldCorners(objectCorners);
            var distance = 0f;
            for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
            {
                if (objectCorners[i].x < screenBounds.xMin) {
                    distance = Mathf.Max(distance, screenBounds.xMin - objectCorners[i].x);
                }
                if (objectCorners[i].y < screenBounds.yMin)
                {
                    distance = Mathf.Max(distance, screenBounds.yMin - objectCorners[i].y);
                }
                if (objectCorners[i].x > screenBounds.xMax)
                {
                    distance = Mathf.Max(distance, objectCorners[i].x - screenBounds.xMax);
                }
                if (objectCorners[i].y > screenBounds.yMax)
                {
                    distance = Mathf.Max(distance, objectCorners[i].y - screenBounds.yMax);
                }
            }
            return distance / rectTransform.lossyScale.x;
        }

        public void PostUpdate()
        {
            if (mouseView != null)
            {
                mouseView.graphicHolderClick.Active = inputManager.hoveredClickableThisFrame;
                mouseView.graphicHolderCursor.Active = !inputManager.hoveredClickableThisFrame;

            }
        }

        public void TryMoveLayer(UIUnit uIUnit, int layer)
        {
            if (creationHelper.manager.layerParents[layer] == uIUnit.transform.parent.gameObject) return;
            uIUnit.transform.parent = creationHelper.manager.layerParents[layer].transform;


        }

        public static void OverwriteDPI(float f) 
        {
            overwrittenDpi = f;
        }

        public static void ClearOverwriteDPI() 
        {
            overwrittenDpi = null;
        }

        public static void ManualUpdateStatic() 
        {
            DpiChanged = previousDpi.HasValue && previousDpi != dpi;
            previousDpi = dpi;
        }
    }
}