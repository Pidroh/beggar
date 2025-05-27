//using UnityEngine.U2D;

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace HeartUnity.View
{
    public class InputWrapper
    {
        internal static Vector2 mousePosition => GetCursorPosition();

        private static Vector2 GetCursorPosition()
        {
            var mouseExists = Mouse.current != null;
            var touchExists = Touchscreen.current != null;
            if (!mouseExists && !touchExists) return Vector2.zero;
            if (mouseExists && touchExists)
            {
                if (Mouse.current.lastUpdateTime > Touchscreen.current.lastUpdateTime)
                {
                    return Mouse.current.position.ReadValue();
                }
                return Touchscreen.current.position.ReadValue();
            }
            if (mouseExists) return Mouse.current.position.ReadValue();
            return Touchscreen.current.position.ReadValue();
        }

        public static bool GetKey(KeyCode p)
        {
            if (Keyboard.current == null) return false;
            KeyControl key = GetKeyControl(p);
            return key != null && key.isPressed;
        }
        public static bool GetKeyDown(KeyCode p)
        {
            if (Keyboard.current == null) return false;
            KeyControl key = GetKeyControl(p);
            return key != null && key.wasPressedThisFrame;
        }

        private static KeyControl GetKeyControl(KeyCode p)
        {
            return p switch
            {
                KeyCode.A => Keyboard.current.aKey,
                KeyCode.B => Keyboard.current.bKey,
                KeyCode.C => Keyboard.current.cKey,
                KeyCode.D => Keyboard.current.dKey,
                KeyCode.E => Keyboard.current.eKey,
                KeyCode.F => Keyboard.current.fKey,
                KeyCode.G => Keyboard.current.gKey,
                KeyCode.H => Keyboard.current.hKey,
                KeyCode.I => Keyboard.current.iKey,
                KeyCode.J => Keyboard.current.jKey,
                KeyCode.K => Keyboard.current.kKey,
                KeyCode.L => Keyboard.current.lKey,
                KeyCode.M => Keyboard.current.mKey,
                KeyCode.N => Keyboard.current.nKey,
                KeyCode.O => Keyboard.current.oKey,
                KeyCode.P => Keyboard.current.pKey,
                KeyCode.Q => Keyboard.current.qKey,
                KeyCode.R => Keyboard.current.rKey,
                KeyCode.S => Keyboard.current.sKey,
                KeyCode.T => Keyboard.current.tKey,
                KeyCode.U => Keyboard.current.uKey,
                KeyCode.V => Keyboard.current.vKey,
                KeyCode.W => Keyboard.current.wKey,
                KeyCode.X => Keyboard.current.xKey,
                KeyCode.Y => Keyboard.current.yKey,
                KeyCode.Z => Keyboard.current.zKey,

                KeyCode.Alpha0 => Keyboard.current.digit0Key,
                KeyCode.Alpha1 => Keyboard.current.digit1Key,
                KeyCode.Alpha2 => Keyboard.current.digit2Key,
                KeyCode.Alpha3 => Keyboard.current.digit3Key,
                KeyCode.Alpha4 => Keyboard.current.digit4Key,
                KeyCode.Alpha5 => Keyboard.current.digit5Key,
                KeyCode.Alpha6 => Keyboard.current.digit6Key,
                KeyCode.Alpha7 => Keyboard.current.digit7Key,
                KeyCode.Alpha8 => Keyboard.current.digit8Key,
                KeyCode.Alpha9 => Keyboard.current.digit9Key,

                KeyCode.Return => Keyboard.current.enterKey,
                KeyCode.KeypadEnter => Keyboard.current.numpadEnterKey,

                KeyCode.Space => Keyboard.current.spaceKey,

                KeyCode.Backspace => Keyboard.current.backspaceKey,
                KeyCode.Tab => Keyboard.current.tabKey,

                KeyCode.Escape => Keyboard.current.escapeKey,

                KeyCode.UpArrow => Keyboard.current.upArrowKey,
                KeyCode.DownArrow => Keyboard.current.downArrowKey,
                KeyCode.LeftArrow => Keyboard.current.leftArrowKey,
                KeyCode.RightArrow => Keyboard.current.rightArrowKey,

                _ => null
            };
        }

        internal static bool GetMouseButton(int v)
        {
            if (ShouldAndCanUseMouse()) 
            {
                if (v == 0) return Mouse.current.leftButton.isPressed;
                return Mouse.current.rightButton.isPressed;
            }
            if (Touchscreen.current == null) return false;
            return Touchscreen.current.primaryTouch.IsPressed();
        }

        private static bool ShouldAndCanUseMouse()
        {
            if (Touchscreen.current == null) { return Mouse.current != null; }
            if (Mouse.current == null || Mouse.current.lastUpdateTime < Touchscreen.current.lastUpdateTime) return false;
            return true;
        }

        private static Pointer GetPointer()
        {
            if (Touchscreen.current == null) { return Mouse.current; }
            if (Mouse.current == null || Mouse.current.lastUpdateTime < Touchscreen.current.lastUpdateTime) return Touchscreen.current;
            return Mouse.current;
        }
    }
}