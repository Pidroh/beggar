using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeartUnity.View.NeoLayout
{
    public class NeoLayout
    {
        public RectTransform Parent { get; }
        public ScrollManager DefaultScrollManager { get; set; }

        public List<Window> Windows = new();

        public NeoLayout(RectTransform parent)
        {
            Parent = parent;
        }

        public Window CreateWindow()
        {
            var window = new Window();
            window.RectTransform = NeoLayout.CreateChild(Parent, new CreateChildParameters() { stretchHeight = true, name = "Window" });
            window.RectTransform.SetWidthMilimeters(60, minSize: 360);
            Windows.Add(window);
            return window;
        }

        public Window CreateOverlayWindow(Window parentWindow)
        {
            var window = new Window();
            // The parent needs to be the NeoLayout main parent instead of the parent window, to guarantee it's above all children
            window.RectTransform = NeoLayout.CreateChild(Parent, new CreateChildParameters() { stretchHeight = true, name = "Window Overlay" });
            parentWindow.AddOverlayWindow(window);
            // not managed directly
            // Windows.Add(window);
            return window;
        }

        public void ManualUpdate()
        {
            foreach (var w in Windows)
            {
                w.ManualUpdate();
            }
        }

        internal static void ApplyRectChildConfig(Vector2 parentSize, RectTransform rectT, RectChildConfig rectChildConfig)
        {
            Vector2 calculatedSize = rectT.GetSize();
            for (int i = 0; i < 2; i++)
            {

                if (rectChildConfig.SizeRatio[i] > 0f)
                {
                    calculatedSize[i] = rectChildConfig.SizeRatio[i] * parentSize[i];
                }
                if (rectChildConfig.SizeMilimeter[i] > 0f)
                {
                    calculatedSize[i] = rectChildConfig.SizeMilimeter[i] * RectTransformExtensions.MilimeterToPixel;
                }
                calculatedSize[i] = Mathf.Max(calculatedSize[i], rectChildConfig.SizeMinimum[i]);
            }
            rectT.SetSize(calculatedSize);
        }

        #region CreateChild
        public static RectTransform CreateChild(RectTransform parentRectTransform, CreateChildParameters parameters)
        {
            // Create a new GameObject
            GameObject childObject = new GameObject("ChildRect");

            // Set the parent of the new GameObject to the parentRectTransform
            childObject.transform.SetParent(parentRectTransform, false);

            // Add a RectTransform component to the new GameObject
            RectTransform childRectTransform = childObject.AddComponent<RectTransform>();
            bool stretchHeight = parameters.stretchHeight;

            // Set anchors to stretch the child vertically and horizontally within the parent
            var vecMin = new Vector2();
            var vecMax = new Vector2();
            for (int j = 0; j < 2; j++)
            {
                var stretch = j == 0 ? parameters.stretchWidth : parameters.stretchHeight;
                vecMin[j] = stretch ? 0 : 0.5f;
                vecMax[j] = stretch ? 1f : 0.5f;
            }

            childRectTransform.anchorMin = vecMin;
            childRectTransform.anchorMax = vecMax;
            childRectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Set the fixed width of the child
            //float fixedWidth = 320f;
            childRectTransform.sizeDelta = new Vector2(0f, 0f); // Set height to 0, it will be automatically adjusted by the anchors
            if (parameters.name != null)
                childRectTransform.gameObject.name = parameters.name;
            return childRectTransform;
        }
        #endregion
    }

    public class Window
    {
        public RectTransform RectTransform;
        public List<FlexiLayout> LowerMenus = new();
        public List<FlexiLayout> HigherMenus = new();
        public List<Window> OverlayWindows = new();

        public bool Active
        {
            get { return RectTransform.gameObject.activeInHierarchy; }
            set
            {
                RectTransform.gameObject.SetActive(value);
            }
        }

        public FlexiLayout CreateLowerMenu(FlexiLayout.FlexiLayoutConfig config)
        {
            var child = NeoLayout.CreateChild(RectTransform, new CreateChildParameters() { stretchWidth = true });
            child.SetHeightMilimeters(10);
            FlexiLayout lowerMenu = new FlexiLayout(RectTransform: child, config);
            LowerMenus.Add(lowerMenu);
            return lowerMenu;
        }

        public FlexiLayout CreateHigherMenu(FlexiLayout.FlexiLayoutConfig config)
        {
            var child = NeoLayout.CreateChild(RectTransform, new CreateChildParameters() { stretchWidth = true });
            child.SetHeightMilimeters(10);
            FlexiLayout higherMenu = new FlexiLayout(RectTransform: child, config);
            HigherMenus.Add(higherMenu);
            return higherMenu;
        }

        public FlexiLayout CreateLowerMenu(ScrollManager scrollManagerPrefab, FlexiLayout.FlexiLayoutConfig config)
        {
            var scrollManager = GameObject.Instantiate(scrollManagerPrefab, RectTransform);
            config.Scroll = scrollManager;
            FlexiLayout lowerMenu = new FlexiLayout(RectTransform: scrollManager.scrollView.GetComponent<RectTransform>(), config);
            LowerMenus.Add(lowerMenu);
            return lowerMenu;
        }

        public void ManualUpdate()
        {
            var width = RectTransform.GetWidth();
            var offset = 0f;
            foreach (var lowerMenu in LowerMenus)
            {
                if (!lowerMenu.Visible) continue;
                NeoLayout.ApplyRectChildConfig(RectTransform.GetSize(), lowerMenu.RectTransform, lowerMenu.Config.RectChildConfig.Value);
                lowerMenu.RectTransform.SetBottomYToParent(offset);
                var height = lowerMenu.RectTransform.GetSize()[1];
                offset += height;
                lowerMenu.ManualUpdate();
            }
            offset = 0f;
            foreach (var higherMenu in HigherMenus)
            {
                if (!higherMenu.Visible) continue;
                NeoLayout.ApplyRectChildConfig(RectTransform.GetSize(), higherMenu.RectTransform, higherMenu.Config.RectChildConfig.Value);
                higherMenu.RectTransform.SetTopYToParent(offset);
                var height = higherMenu.RectTransform.GetSize()[1];
                offset += height;
                higherMenu.ManualUpdate();
            }
            foreach (var ow in OverlayWindows)
            {
                ow.ManualUpdate();
                ow.RectTransform.transform.position = RectTransform.position;
                ow.RectTransform.SetSize(RectTransform.GetSize());
            }
        }

        internal void AddOverlayWindow(Window window)
        {
            OverlayWindows.Add(window);
        }
    }

    public class FlexiLayout
    {
        public List<LayoutChild> children = new();
        public RectTransform RectTransform { get; }
        public FlexiLayoutConfig Config { get; }
        public ScrollManager ScrollManager => Config.Scroll;
        public RectTransform ContentRectTransform => ScrollManager != null ? ScrollManager.Content : RectTransform;

        public bool Visible
        {
            get
            {
                return RectTransform.gameObject.activeSelf;
            }
            set
            {
                RectTransform.gameObject.SetActive(value);
            }
        }

        public UIUnit UiUnit { get; private set; }

        public FlexiLayout(RectTransform RectTransform, FlexiLayoutConfig config)
        {
            // default value if none
            if (!config.RectChildConfig.HasValue)
            {
                config.RectChildConfig = new RectChildConfig()
                {
                    SizeRatio = new Vector2(1.0f, -1),
                    SizeMilimeter = new Vector2(-1, 10)
                };
            }
            this.RectTransform = RectTransform;
            Config = config;
        }


        public LayoutChild Add(MonoBehaviour child, LayoutChild.LayoutChildConfig config)
        {
            var childTransform = child.GetComponent<RectTransform>();
            LayoutChild lc = new LayoutChild(childTransform, config);
            children.Add(lc);
            if (Config.Horizontal)
                childTransform.SetBottomYToParent(0);
            childTransform.parent = ContentRectTransform;
            return lc;
        }

        public void ManualUpdate()
        {
            var distance = 3.2f * RectTransformExtensions.MilimeterToPixel;
            var vectorIndex = Config.Horizontal ? 0 : 1;
            var oppositeIndex = vectorIndex == 0 ? 1 : 0;
            var parentSize = RectTransform.GetSize();

            int initialLineIndex = 0;
            bool breakLine = Config.LayoutMode == LayoutMode.LINE_BREAK_ON_OVERFLOW;
            var orderFromTop = Config.OppositeIndexFromTop;
            var posOppositeOffset = 0f;
            // 100 to avoid infinite loop
            for (int j = 0; j < 100; j++)
            {
                // subtracts distance because if only one object there is no distance to take into account
                var totalSizeInLine = 0f - distance;
                var maxOppositeSize = 0f;
                var maxLineIndex = children.Count - 1;
                // fix sizes and try to determine the max index in the current line (causing linebreak if necessary)
                for (int childIndex = initialLineIndex; childIndex < children.Count; childIndex++)
                {
                    LayoutChild c = children[childIndex];
                    var rectT = c.RectTransform;
                    RectChildConfig rectChildConfig = c.Config.RectChildConfig;
                    NeoLayout.ApplyRectChildConfig(parentSize, rectT, rectChildConfig);
                    var newSize = totalSizeInLine + c.RectTransform.GetSize()[vectorIndex];
                    // force line break here if necessary
                    if (breakLine && newSize > parentSize[vectorIndex])
                    {
                        break;
                    }
                    // if the code reaches this place, then the current LayoutChild is part of the line
                    maxLineIndex = childIndex;
                    totalSizeInLine = newSize;
                    maxOppositeSize = Mathf.Max(c.RectTransform.GetSize()[oppositeIndex], maxOppositeSize);
                    totalSizeInLine += distance;
                }
                // enforce the positions of the current line
                var posMin = -totalSizeInLine / 2;
                for (int i = initialLineIndex; i < maxLineIndex + 1; i++)
                {
                    LayoutChild c = children[i];
                    switch (vectorIndex)
                    {
                        case 0:
                            c.RectTransform.SetLeftLocalX(posMin);
                            if (orderFromTop)
                            {
                                c.RectTransform.SetTopYToParent(posOppositeOffset);
                            }
                            else
                            {
                                c.RectTransform.SetBottomYToParent(posOppositeOffset);
                            }

                            break;
                        case 1:
                            c.RectTransform.SetBottomLocalY(posMin);
                            if (orderFromTop)
                            {
                                c.RectTransform.SetRightXToParent(posOppositeOffset);
                            }
                            else
                            {
                                c.RectTransform.SetLeftXToParent(posOppositeOffset);
                            }
                            break;
                        default:
                            break;
                    }

                    posMin += c.RectTransform.GetSize()[vectorIndex] + distance;
                }
                // update the offset for the next line
                posOppositeOffset += maxOppositeSize + distance;
                // make the next line start right after the current one
                initialLineIndex = maxLineIndex + 1;
                // end if it's the last line
                if (initialLineIndex >= children.Count) break;
            }
        }

        public T Instantiate<T>(T prefab, out LayoutChild layoutChild, LayoutChild.LayoutChildConfig config) where T : MonoBehaviour
        {
            T obj = GameObject.Instantiate(prefab, ContentRectTransform);
            layoutChild = Add(obj, config);
            return obj;
        }

        public void EnforceParent(RectTransform parent)
        {
            NeoLayout.ApplyRectChildConfig(parent.GetSize(), RectTransform, Config.RectChildConfig.Value);
        }

        public void EnableClicking()
        {
            var button = RectTransform.gameObject.AddComponent<Button>();
            var image = RectTransform.gameObject.AddComponent<Image>();
            button.image = image;
            UiUnit = RectTransform.gameObject.AddComponent<UIUnit>();
            image.color = new Color(0,0,0,0);
        }

        #region internal classes
        public struct FlexiLayoutConfig
        {
            public bool Horizontal;
            public LayoutMode LayoutMode;
            public bool OppositeIndexFromTop;

            public ScrollManager Scroll { get; internal set; }
            public RectChildConfig? RectChildConfig;
        }

        public class LayoutChild
        {
            public RectTransform RectTransform;

            public LayoutChild(RectTransform rectTransform, LayoutChildConfig config)
            {
                RectTransform = rectTransform;
                Config = config;
            }

            public LayoutChildConfig Config { get; }

            public struct LayoutChildConfig
            {
                public RectChildConfig RectChildConfig;
            }
        }

        public enum LayoutMode
        {
            NONE, SCALE_TO_FIT_UNIMPLEMENTED, LINE_BREAK_ON_OVERFLOW
        }
        #endregion

    }

    #region config and initialization structs
    public struct RectChildConfig
    {
        public Vector2 SizeRatio { get; set; }
        public Vector2 SizeMilimeter { get; set; }
        public Vector2 SizeMinimum { get; set; }
    }

    public struct CreateChildParameters
    {
        public bool stretchWidth;
        public bool stretchHeight;
        public string name;
    }
    #endregion
}