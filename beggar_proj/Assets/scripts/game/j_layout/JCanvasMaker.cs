﻿using HeartUnity.View;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using static JLayout.JLayoutRuntimeData;

namespace JLayout
{
    public class JCanvasMaker
    {
        

        public static JLayCanvas CreateCanvas(int N, CanvasMaker.CreateCanvasRequest canvasReq, Canvas reusableCanvas, JLayoutRuntimeData runtime)
        {
            JLayCanvas dc = new JLayCanvas();
            runtime.jLayCanvas = dc;
            // Create Canvas GameObject
            if (reusableCanvas == null)
            {
                dc.canvasGO = new GameObject("Canvas");
                // Create EventSystem GameObject
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<InputSystemUIInputModule>();
                // Set the EventSystem as a sibling of the Canvas
                eventSystemGO.transform.SetParent(dc.canvasGO.transform, false);

                Canvas canvas = dc.canvasGO.AddComponent<Canvas>();
                dc.canvasGO.AddComponent<GraphicRaycaster>();
                dc.Canvas = canvas;
                dc.canvasGO.AddComponent<CanvasScaler>();
            }
            else
            {
                var cnvs = GameObject.Instantiate(reusableCanvas);
                dc.canvasGO = cnvs.gameObject;
                dc.Canvas = cnvs;
            }

            {
                var canvas = dc.Canvas;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                var canvasGO = dc.canvasGO;
                var scaler = canvasGO.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.referenceResolution = new Vector2(1920, 1080);


                // Create Root GameObject
                GameObject rootGO = new GameObject("Root");
                rootGO.transform.SetParent(canvasGO.transform, false);

                RectTransform rootRT = rootGO.AddComponent<RectTransform>();
                rootRT.FillParent();

                // Create N children
                for (int i = 0; i < N; i++)
                {
                    dc.children.Add(new JLayCanvasChild(CreateCanvasScrollChild(rootGO, i, canvasReq.ScrollStyle, runtime)));
                }
                dc.childrenForLayouting.AddRange(dc.children);
                dc.RootRT = rootRT;
                dc.OverlayRoot = canvasGO.GetComponent<RectTransform>().CreateFullSizeChild("overlay_root");
                {
                    var oi = dc.OverlayRoot.CreateFullSizeChild("overlay_image");
                    dc.overlayImage = oi.gameObject.AddComponent<Image>();
                    dc.overlayImage.color = new Color(0f, 0f, 0f, 0.99f);
                    dc.HideOverlay();
                }
                {
                    //var canvasChild = CreateSimpleCanvasChild(dc.OverlayRoot.gameObject);
                    JLayoutRuntimeUnit layoutRuntimeUnit = CreateCanvasScrollChild(dc.OverlayRoot.gameObject, 0, canvasReq.ScrollStyle, runtime);
                    layoutRuntimeUnit.ContentTransform.pivot = new Vector2(layoutRuntimeUnit.ContentTransform.pivot.x, 0.5f);
                    dc.Overlays.Add(new JLayCanvasChild(layoutRuntimeUnit));
                }
                // shows in the opposite order so that the bottoms ones are shown last
                // thus, prioritized
                for (int i = N - 1; i >= 0; i--)
                //for (int i = 0; i < N; i++)
                {
                    dc.ShowChild(dc.children[i]);
                }
            }

            return dc;
        }

        internal static void AddFixedMenu(JLayCanvas jCanvas, Direction dir, JLayoutRuntimeUnit layoutRU)
        {
            jCanvas.FixedMenus[dir].Add(new JLayCanvasChild(layoutRU));
            layoutRU.RectTransform.SetParent(jCanvas.RootRT);
        }

        private static JLayoutRuntimeUnit CreateLayout(JLayoutRuntimeData runtime)
        {
            JLayoutRuntimeUnit lp;
            {
                // Create Child GameObject
                GameObject childGO2 = new GameObject();
                childGO2.name = "Layout";
                RectTransform childRT2 = childGO2.AddComponent<RectTransform>();
                lp = new JLayoutRuntimeUnit(childRT2, runtime.CurrentColorSchemeId);
                childRT2.SetAnchorsByIndex(1, 1f);
            }

            return lp;
        }

        internal static JLayoutRuntimeUnit CreateLayout(LayoutData layoutD, JLayoutRuntimeData runtime)
        {
            JLayoutRuntimeUnit ru = CreateLayout(runtime);
            bool clickable = layoutD.Clickable;
            bool hasColor = layoutD.commons.ColorSet != null;
            var createImage = hasColor || clickable;
            if (createImage) {
                var img = ru.RectTransform.gameObject.AddComponent<Image>();
                var color = Color.clear;
                if (clickable)
                {
                    var button = ru.RectTransform.gameObject.AddComponent<Button>();
                    button.targetGraphic = img;
                    var uu = ru.RectTransform.gameObject.AddComponent<UIUnit>();
                    ru.SelfUIUnit = uu;
                }
                if (hasColor)
                {
                    color = layoutD.commons.ColorSet.ColorDatas[ColorSetType.NORMAL].data.Colors[runtime.CurrentColorSchemeId];
                }
                img.color = color;
            }

            
            ru.LayoutData = layoutD;
            
            
            foreach (var childData in layoutD.Children)
            {
                switch (childData)
                {
                    case { ButtonRef: not null }:
                        {
                            (JLayoutRuntimeUnit buttonLayout, UIUnit uiUnit) = CreateButton(childData.ButtonRef.data, runtime);
                            // will have to fuse commons data eventually
                            var child = ru.AddLayoutAsChild(buttonLayout, childData);
                            child.UiUnit = uiUnit;

                            ru.BindButton(buttonLayout, child);
                        }
                        break;
                    case { TextRef: not null }:
                        {
                            JLayoutChild textChild = CreateText(childData, childData.TextRef.data, runtime);
                            ru.AddChild(textChild);
                            ru.BindText(textChild);
                        }
                        break;
                    case { ImageKey: not null }:
                        {
                            
                            JLayoutChild imageChild = CreateImage(childData, runtime);
                            ru.AddChild(imageChild);
                            ru.BindImage(imageChild);
                        }
                        break;
                    case { LayoutRef: not null }:
                        {
                            var childLayout = CreateLayout(childData.LayoutRef.data, runtime);
                            var c = ru.AddLayoutAsChild(childLayout, childData);
                            ru.BindLayout(c);
                        }
                        break;
                    default:
                        break;
                }
            }
            ru.RectTransform.gameObject.name = "layout_" + layoutD.Id;
            return ru;
        }

        public static JLayoutChild CreateImage(LayoutChildData childData, JLayoutRuntimeData runtime) 
        {
            var sprite = runtime.ImageSprites[childData.ImageKey];
            var color = childData.Commons.ColorSet?.ColorDatas[ColorSetType.NORMAL].data.Colors[runtime.CurrentColorSchemeId] ?? Color.white;
            var unit = CanvasMaker.CreateSimpleImage(color);
            unit.Image.sprite = sprite;
            unit.Image.type = Image.Type.Sliced;
            return new JLayoutChild
            {
                Commons = childData.Commons,
                LayoutChildData = childData,
                UiUnit = unit,
                ColorSchemeId = runtime.CurrentColorSchemeId
            };
        }

        private static JLayoutChild CreateText(LayoutChildData childData, TextData data, JLayoutRuntimeData runtime)
        {
            var textColor = data.GetNormalColor(runtime.CurrentColorSchemeId);
            var fontSize = data.Size;
            var uiUnit = CanvasMaker.CreateTextUnit(textColor, runtime.DefaultFont, fontSize);
            uiUnit.text.horizontalAlignment = childData.Commons.TextHorizontalMode switch
            {
                TextHorizontal.RIGHT => TMPro.HorizontalAlignmentOptions.Right,
                TextHorizontal.LEFT => TMPro.HorizontalAlignmentOptions.Left,
                TextHorizontal.CENTER => TMPro.HorizontalAlignmentOptions.Center,
                _ => TMPro.HorizontalAlignmentOptions.Left
            };
            var commons = childData.Commons;
            return new JLayoutChild
            {
                Commons = commons,
                LayoutChildData = childData,
                TextData = data,
                UiUnit = uiUnit,
                ColorSchemeId = runtime.CurrentColorSchemeId
            };
        }

        private static (JLayoutRuntimeUnit,UIUnit) CreateButton(ButtonData buttonD, JLayoutRuntimeData runtime)
        {
            var layout = CreateLayout(buttonD.LayoutData, runtime);
            GameObject buttonObject = layout.RectTransform.gameObject;
            buttonObject.name += " button";
            // Add Button component
            var button = buttonObject.AddComponent<Button>();

            var uiUnit = buttonObject.AddComponent<UIUnit>();
            uiUnit.Init();
            // Add CanvasRenderer component
            buttonObject.AddComponent<CanvasRenderer>();

            

            // Add Image component for button background
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0, 0, 0, 0); // Set button background color
            button.targetGraphic = buttonImage;
            return (layout, uiUnit);
        }
        
        static JLayoutRuntimeUnit CreateCanvasScrollChild(GameObject parent, int index, CanvasMaker.ScrollStyle scrollStyle, JLayoutRuntimeData runtime)
        {
            JLayoutRuntimeUnit lp = null;

            lp = CreateLayout(runtime);
            var ld = new LayoutData();
            ld.commons = new();
            ld.commons.AxisModes = new AxisMode[2];
            ld.commons.AxisModes[0] = AxisMode.SELF_SIZE;
            ld.commons.AxisModes[1] = AxisMode.CONTAIN_CHILDREN;
            lp.LayoutData = ld;

            var childRT = lp.RectTransform;
            var childGO = lp.RectTransform.gameObject;
            childRT.anchorMin = new Vector2(0, 0);
            childRT.anchorMax = new Vector2(0, 1);
            childRT.sizeDelta = new Vector2(320, 0);
            childGO.transform.SetParent(parent.transform, false);


            childRT.anchoredPosition = new Vector2(320 * index, 0);

            // Add ScrollView
            ScrollRect scrollRect = childGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
#if !UNITY_EDITOR
        scrollRect.scrollSensitivity *= 0.1f;
#endif
#if UNITY_EDITOR
            scrollRect.scrollSensitivity *= 2f;
#endif

            // Add Viewport
            GameObject viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(childGO.transform, false);

            RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;
            viewportRT.SetPivotByIndex(0, 0);

            CanvasRenderer viewportCR = viewportGO.AddComponent<CanvasRenderer>();
            Image viewportImage = viewportGO.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 1f);
            viewportGO.AddComponent<Mask>().showMaskGraphic = false;

            scrollRect.viewport = viewportRT;

            // Add Content
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);

            RectTransform contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = new Vector2(0, 0);
            contentRT.sizeDelta = new Vector2(0, 0);

            const int scrollBarWidth = 10;

            // contentRT.SetOffsetMaxByIndex(0, -scrollBarWidth);
            contentRT.SetOffsetMaxByIndex(0, -4);


            lp.ContentTransformOverride = contentRT;

            scrollRect.content = contentRT;

            // Add Vertical ScrollBar
            GameObject scrollbarGO = new GameObject("ScrollbarVertical");
            scrollbarGO.transform.SetParent(childGO.transform, false);

            RectTransform scrollbarRT = scrollbarGO.AddComponent<RectTransform>();
            scrollbarRT.anchorMin = new Vector2(1, 0);
            scrollbarRT.anchorMax = new Vector2(1, 1);
            scrollbarRT.pivot = new Vector2(1, 0.5f);

            scrollbarRT.sizeDelta = new Vector2(scrollBarWidth, 0);

            Scrollbar scrollbar = scrollbarGO.AddComponent<Scrollbar>();
            Image scrollbarImage = scrollbarGO.AddComponent<Image>();
            scrollbarImage.color = scrollStyle.ScrollBarBG;

            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarSpacing = -3;

            // Add Scrollbar Handle
            GameObject handleGO = new GameObject("Handle");
            handleGO.transform.SetParent(scrollbarGO.transform, false);

            RectTransform handleRT = handleGO.AddComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(0, 0);

            Image handleImage = handleGO.AddComponent<Image>();
            handleImage.color = scrollStyle.ScrollHandleColor;
            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRT;
            lp.DefaultPositionModes = new PositionMode[2] { PositionMode.LEFT_ZERO, PositionMode.SIBLING_DISTANCE };

            return lp;
        }

        internal static JLayoutRuntimeUnit CreateLayout(string layoutId, JLayoutRuntimeData runtime)
        {
            return CreateLayout(runtime.LayoutMaster.LayoutDatas.GetData(layoutId), runtime);
        }
    }
}
