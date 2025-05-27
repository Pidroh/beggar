//using UnityEngine.U2D;

using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace HeartUnity.View
{
    public static class InputManagerKeyboardNewInputSystem
    {
        public static (Key, int)[] KeyToHeart = new (Key, int)[]
        {
            (Key.UpArrow, HeartKeys.KEY_UP),
            (Key.LeftArrow, HeartKeys.KEY_LEFT),
            (Key.DownArrow, HeartKeys.KEY_DOWN),
            (Key.RightArrow, HeartKeys.KEY_RIGHT),
            (Key.Enter, HeartKeys.KEY_ENTER),
            (Key.Space, HeartKeys.KEY_SPACE),
            (Key.Escape, HeartKeys.KEY_ESCAPE),
            (Key.Tab, HeartKeys.KEY_TAB),
            (Key.LeftShift, HeartKeys.KEY_SHIFT),
            (Key.RightShift, HeartKeys.KEY_SHIFT),
        };

        public static void UpdateKeyboard(List<int> keysDown, List<int> keysPressed, List<int> keysUp, ref bool deviceKeyboard)
        {
            // Letters A-Z
            for (char c = 'A'; c <= 'Z'; c++)
            {
                var key = Key.A + (c - 'A');
                var keyControl = Keyboard.current[key];
                UpdateKeyboard(keysDown, keysPressed, keysUp, c, keyControl, ref deviceKeyboard);
            }

            // Numbers 0-9
            for (char c = '0'; c <= '9'; c++)
            {
                var key = Key.Digit0 + (c - '0');
                var keyControl = Keyboard.current[key];
                UpdateKeyboard(keysDown, keysPressed, keysUp, c, keyControl, ref deviceKeyboard);
            }
            foreach (var pair in KeyToHeart)
            {
                var keyControl = Keyboard.current[pair.Item1];
                UpdateKeyboard(keysDown, keysPressed, keysUp, pair.Item2, keyControl, ref deviceKeyboard);
            }
        }

        private static void UpdateKeyboard(List<int> keysDown, List<int> keysPressed, List<int> keysUp, int v, KeyControl aKey, ref bool deviceKeyboard)
        {
            if (aKey.isPressed)
            {
                deviceKeyboard = true;
                keysPressed.Add(v);
            }
            if (aKey.wasReleasedThisFrame)
            {
                keysUp.Add(v);
            }
            if (aKey.wasPressedThisFrame)
            {
                deviceKeyboard = true;
                keysDown.Add(v);
            }
        }
    }


}