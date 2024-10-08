//using UnityEngine.U2D;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.Pool;

namespace HeartUnity.View
{

    public class InputManager
    {
        public List<int> keysDown = new();
        public List<int> keysPressed = new();
        public List<int> keysUp = new();
        public List<int> config_keysThatDontSwapBetweenKeyboardAndMouse = new();
        private List<int> buttonsDown = new();
        private List<int> buttonsDownRepeatLongPress = new();
        private List<int> buttonsPressed = new();
        public Dictionary<int, float> buttonPressedTime = new();
        public bool hoveredClickableThisFrame = false;

        public void HoveredClickableThisFrame()
        {
            hoveredClickableThisFrame = true;
        }

        private List<int> buttonsDownConsumed = new();
        Dictionary<KeyCode, int> keyCodeToIntCode = new Dictionary<KeyCode, int> {
        { KeyCode.KeypadEnter, HeartKeys.KEY_ENTER }, { KeyCode.Return, HeartKeys.KEY_ENTER }, { KeyCode.Insert, HeartKeys.KEY_ENTER }, { KeyCode.Space, HeartKeys.KEY_SPACE }, { KeyCode.Tab, HeartKeys.KEY_TAB }, { KeyCode.LeftArrow, HeartKeys.KEY_LEFT }, { KeyCode.UpArrow, HeartKeys.KEY_UP }, { KeyCode.DownArrow, HeartKeys.KEY_DOWN }, { KeyCode.RightArrow, HeartKeys.KEY_RIGHT }, { KeyCode.Escape, HeartKeys.KEY_ESCAPE },
        { KeyCode.Mouse0, HeartKeys.MOUSE_BUTTON_LEFT }, { KeyCode.Mouse1, HeartKeys.MOUSE_BUTTON_RIGHT },
        {KeyCode.Alpha0, ((int)'0')},
        {KeyCode.Alpha1, ((int)'1')},
        {KeyCode.Alpha2, ((int)'2')},
        {KeyCode.Alpha3, ((int)'3')},
        {KeyCode.Alpha4, ((int)'4')},
        {KeyCode.Alpha5, ((int)'5')},
        {KeyCode.Alpha6, ((int)'6')},
        {KeyCode.Alpha7, ((int)'7')},
        {KeyCode.Alpha8, ((int)'8')},
        {KeyCode.Alpha9, ((int)'9')},
        {KeyCode.Keypad0, ((int)'0')},
        {KeyCode.Keypad1, ((int)'1')},
        {KeyCode.Keypad2, ((int)'2')},
        {KeyCode.Keypad3, ((int)'3')},
        {KeyCode.Keypad4, ((int)'4')},
        {KeyCode.Keypad5, ((int)'5')},
        {KeyCode.Keypad6, ((int)'6')},
        {KeyCode.Keypad7, ((int)'7')},
        {KeyCode.Keypad8, ((int)'8')},
        {KeyCode.Keypad9, ((int)'9')}};

        public ButtonBinding IdentifyKeyboardOrGamepadBindingForLatestDevice(List<ButtonBinding> bb, out InputDevice id)
        {
            InputDevice keyboardOrMouseOnlyDevice = latestInputDeviceKeyboardOrGamepad.HasValue ? latestInputDeviceKeyboardOrGamepad.Value : InputDevice.KEYBOARD;
            foreach (var b in bb)
            {

                if (b.key >= HeartKeys.JOY_BUTTON_SOUTH && b.key <= HeartKeys.JOY_BUTTON_SOUTH + 20)
                {
                    if (keyboardOrMouseOnlyDevice == InputDevice.CONTROLLER)
                    {
                        id = InputDevice.CONTROLLER;
                        return b;
                    }
                }
                else
                {
                    // checking if binding is for keyboard buttons
                    if (b.button < HeartKeys.JOY_BUTTON_SOUTH)
                    {
                        if (keyboardOrMouseOnlyDevice != InputDevice.CONTROLLER)
                        {
                            id = InputDevice.KEYBOARD;
                            return b;
                        }
                    }
                }
            }
            id = InputDevice.KEYBOARD;
            return null;
        }

        public struct AxisToKey
        {
            public string axis;
            public int negativeKey;
            public int positiveKey;
            public int previousDirection;
        }

        public KeyCode joystickStartKeyCode = KeyCode.JoystickButton0;
        public KeyCode joystickEndKeyCode = KeyCode.JoystickButton19;

        int[] joystickKeys = { HeartKeys.JOY_BUTTON_WEST, HeartKeys.JOY_BUTTON_SOUTH, HeartKeys.JOY_BUTTON_EAST, HeartKeys.JOY_BUTTON_NORTH, HeartKeys.JOY_BUTTON_L, HeartKeys.JOY_BUTTON_R, HeartKeys.JOY_BUTTON_L2, HeartKeys.JOY_BUTTON_R2, HeartKeys.JOY_BUTTON_SELECT, HeartKeys.JOY_BUTTON_START };


        public Dictionary<HeartKeys, ContinuousStateKey> continuousStateKeys = new();

        private bool _inputEnabled;
        private bool _inputDisableRequest;

        public bool InputEnabled => _inputEnabled;

        public GamepadType LatestGamepad => GetGamepadType();

        private GamepadType GetGamepadType()
        {
#if UNITY_SWITCH
            return GamepadType.SWITCH;
#endif
            if (Gamepad.current == null)
            {
                return GamepadType.XBOX;
            }
            if (Gamepad.current is DualShockGamepad)
            {
                return GamepadType.PLAYSTATION;
            }
            return GamepadType.XBOX;
        }

        public enum InputDevice
        {
            KEYBOARD, MOUSE, CONTROLLER
        }

        public InputDevice? _latestInputDevice;

        public InputDevice? LatestInputDevice
        {
            get
            {
                return _latestInputDevice;
            }
            set
            {
                _latestInputDevice = value;
                if (value == InputDevice.KEYBOARD || value == InputDevice.CONTROLLER)
                    latestInputDeviceKeyboardOrGamepad = value;
                
            }
        }
        public InputDevice? latestInputDeviceKeyboardOrGamepad = null;
        public static InputManagerCrossSceneData? CrossSceneData = null;
        public static Vector3 CanvasMousePosition;
        static private readonly float LONG_PRESS_REPEAT = 0.2f;
        static private readonly float LONG_PRESS_DELAY = 0.22f;

        public void ManualUpdate()
        {
            _inputEnabled = true;
            hoveredClickableThisFrame = false;
            if (_inputDisableRequest) _inputEnabled = false;
            keysDown.Clear();
            keysPressed.Clear();
            keysUp.Clear();

            if (InputEnabled)
            {
                if (Gamepad.current != null)
                {
                    GamepadToHeartKey(Gamepad.current.dpad.down, HeartKeys.JOY_BUTTON_D_DOWN);
                    GamepadToHeartKey(Gamepad.current.dpad.left, HeartKeys.JOY_BUTTON_D_LEFT);
                    GamepadToHeartKey(Gamepad.current.dpad.right, HeartKeys.JOY_BUTTON_D_RIGHT);
                    GamepadToHeartKey(Gamepad.current.dpad.up, HeartKeys.JOY_BUTTON_D_UP);
                    GamepadToHeartKey(Gamepad.current.leftStick.down, HeartKeys.JOY_BUTTON_D_DOWN);
                    GamepadToHeartKey(Gamepad.current.leftStick.left, HeartKeys.JOY_BUTTON_D_LEFT);
                    GamepadToHeartKey(Gamepad.current.leftStick.right, HeartKeys.JOY_BUTTON_D_RIGHT);
                    GamepadToHeartKey(Gamepad.current.leftStick.up, HeartKeys.JOY_BUTTON_D_UP);
                    GamepadToHeartKey(Gamepad.current.buttonSouth, HeartKeys.JOY_BUTTON_SOUTH);
                    GamepadToHeartKey(Gamepad.current.buttonEast, HeartKeys.JOY_BUTTON_EAST);
                    GamepadToHeartKey(Gamepad.current.buttonWest, HeartKeys.JOY_BUTTON_WEST);
                    GamepadToHeartKey(Gamepad.current.buttonNorth, HeartKeys.JOY_BUTTON_NORTH);
                    GamepadToHeartKey(Gamepad.current.leftTrigger, HeartKeys.JOY_BUTTON_L);
                    GamepadToHeartKey(Gamepad.current.rightTrigger, HeartKeys.JOY_BUTTON_R);
                    GamepadToHeartKey(Gamepad.current.leftShoulder, HeartKeys.JOY_BUTTON_L2);
                    GamepadToHeartKey(Gamepad.current.rightShoulder, HeartKeys.JOY_BUTTON_R2);
                    GamepadToHeartKey(Gamepad.current.startButton, HeartKeys.JOY_BUTTON_START);
                    GamepadToHeartKey(Gamepad.current.selectButton, HeartKeys.JOY_BUTTON_SELECT);
                }

                var keyCodes = EnumHelper<KeyCode>.GetAllValues();
                foreach (var kc in keyCodes)
                {
                    if (Input.GetKeyDown(kc))
                    {
                        keysDown.Add(TranslateKeyCode(kc, keyCodeToIntCode));

                    }
                    if (Input.GetKeyUp(kc))
                    {
                        keysUp.Add(TranslateKeyCode(kc, keyCodeToIntCode));
                    }
                    if (Input.GetKey(kc))
                    {
                        int translatedKey = TranslateKeyCode(kc, keyCodeToIntCode);
                        keysPressed.Add(translatedKey);
                        // only changes pressing latestInputDevice is not a target of no swapping configuration
                        if (!config_keysThatDontSwapBetweenKeyboardAndMouse.Contains(translatedKey) ||
                            (LatestInputDevice != InputDevice.KEYBOARD && LatestInputDevice != InputDevice.MOUSE))
                        {
                            LatestInputDevice = IdentifyDevice(kc);
                        }

                    }
                }
                int TranslateKeyCode(KeyCode kc, Dictionary<KeyCode, int> overwriteKeys)
                {
                    if (overwriteKeys.TryGetValue(kc, out int value))
                    {
                        return value;
                    }
                    /*
                    if (kc >= joystickStartKeyCode && kc <= joystickEndKeyCode)
                    {
                        foreach (var item in Input.GetJoystickNames())
                        {
                            Debug.Log(item);
                        }

                        var indexJoystick = kc - joystickStartKeyCode;
                        if (joystickKeys.Length > indexJoystick)
                        {
                            return joystickKeys[indexJoystick];
                        }
                    }
                    */
                    return (int)kc;
                }

            }

            if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.2f)
            {
                LatestInputDevice = InputDevice.MOUSE;
            }



        }

        private void GamepadToHeartKey(ButtonControl bc, int key)
        {
            if (bc.wasPressedThisFrame)
            {
                keysDown.Add(key);
                LatestInputDevice = InputDevice.CONTROLLER;
                
            }
            if (bc.IsPressed())
            {
                keysPressed.Add(key);
                LatestInputDevice = InputDevice.CONTROLLER;
            }
        }

        public static List<ButtonBinding> CreateDefaultButtonBindings()
        {
            List<ButtonBinding> buttonBindings = new();
            buttonBindings.Add(new ButtonBinding(HeartKeys.KEY_UP, DefaultButtons.UP));
            buttonBindings.Add(new ButtonBinding(HeartKeys.KEY_RIGHT, DefaultButtons.RIGHT));
            buttonBindings.Add(new ButtonBinding(HeartKeys.KEY_LEFT, DefaultButtons.LEFT));
            buttonBindings.Add(new ButtonBinding(HeartKeys.KEY_DOWN, DefaultButtons.DOWN));
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_D_UP, DefaultButtons.UP));
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_D_RIGHT, DefaultButtons.RIGHT));
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_D_LEFT, DefaultButtons.LEFT));
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_D_DOWN, DefaultButtons.DOWN));
            buttonBindings.Add(new ButtonBinding('w', DefaultButtons.UP));
            buttonBindings.Add(new ButtonBinding('d', DefaultButtons.RIGHT));
            buttonBindings.Add(new ButtonBinding('a', DefaultButtons.LEFT));
            buttonBindings.Add(new ButtonBinding('s', DefaultButtons.DOWN));
            buttonBindings.Add(new ButtonBinding(HeartKeys.KEY_ENTER, DefaultButtons.CONFIRM));
            buttonBindings.Add(new ButtonBinding(HeartKeys.KEY_SPACE, DefaultButtons.CONFIRM));
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_START, DefaultButtons.START));
#if PLATFORM_SWITCH
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_EAST, DefaultButtons.CONFIRM));
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_SOUTH, DefaultButtons.CANCEL));
#else
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_SOUTH, DefaultButtons.CONFIRM));
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_EAST, DefaultButtons.CANCEL));
#endif

            buttonBindings.Add(new ButtonBinding(HeartKeys.KEY_ESCAPE, DefaultButtons.CANCEL));
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_L2, DefaultButtons.LEFT_TRIGGER_2));
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_L, DefaultButtons.LEFT_TRIGGER));
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_R2, DefaultButtons.RIGHT_TRIGGER_2));
            buttonBindings.Add(new ButtonBinding(HeartKeys.JOY_BUTTON_R, DefaultButtons.RIGHT_TRIGGER));
            return buttonBindings;
        }

        public bool ConsumeButtonDown(DefaultButtons button)
        {
            if (IsButtonDown(button))
            {
                buttonsDownConsumed.Add((int)button);
                return true;
            }
            return false;
        }

        public void UpdateWithButtonBindings(List<ButtonBinding> buttonBindings)
        {
            buttonsDown.Clear();
            buttonsDownRepeatLongPress.Clear();
            buttonsDownConsumed.Clear();
            buttonsPressed.Clear();
            if (!InputEnabled) return;
            foreach (var bb in buttonBindings)
            {
                if (keysDown.Contains(bb.key))
                {
                    buttonsDown.Add(bb.button);
                }
                if (keysPressed.Contains(bb.key))
                {
                    buttonsPressed.Add(bb.button);
                }
            }
            foreach (var b in buttonsPressed)
            {
                if (!buttonPressedTime.ContainsKey(b))
                {
                    buttonPressedTime[b] = 0;
                }
            }
            using (ListPool<int>.Get(out var list))
            {
                list.AddRange(buttonPressedTime.Keys);
                foreach (var button in list)
                {
                    if (buttonsPressed.Contains(button))
                    {
                        var before = Mathf.CeilToInt((buttonPressedTime[button] - LONG_PRESS_DELAY) / LONG_PRESS_REPEAT);
                        buttonPressedTime[button] += Time.deltaTime;
                        var after = Mathf.CeilToInt((buttonPressedTime[button] - LONG_PRESS_DELAY) / LONG_PRESS_REPEAT);
                        if (before != after && after > 0)
                        {
                            buttonsDownRepeatLongPress.Add(button);
                        }
                    }
                    else
                    {
                        buttonPressedTime[button] = 0;
                    }
                }
            }
            //buttonPressedTime.Keys;



        }
        public bool IsButtonDown(DefaultButtons button)
        {
            return IsButtonDown((int)button);
        }

        public bool IsButtonPressed(DefaultButtons button)
        {
            return IsButtonPressed((int)button);
        }

        public bool IsButtonDownOrRepeat(DefaultButtons button)
        {
            return IsButtonDownOrRepeat((int)button);
        }


        public bool IsButtonDown(Enum button)
        {
            int buttonInt = Convert.ToInt32(button);
            return IsButtonDown(buttonInt);
        }

        private bool IsButtonDown(int buttonInt)
        {
            if (!InputEnabled) return false;
            if (buttonsDownConsumed.Contains(buttonInt)) return false;
            return buttonsDown.Contains(buttonInt);
        }
        private bool IsButtonPressed(int buttonInt)
        {
            if (!InputEnabled) return false;
            return buttonsPressed.Contains(buttonInt);
        }
        private bool IsButtonDownOrRepeat(int buttonInt)
        {
            if (!InputEnabled) return false;
            if (buttonsDownConsumed.Contains(buttonInt)) return false;
            return buttonsDown.Contains(buttonInt) || buttonsDownRepeatLongPress.Contains(buttonInt);
        }

        public void DisableInputForThisFrameAndNext()
        {
            _inputEnabled = false;
            _inputDisableRequest = true;
        }


        private InputDevice IdentifyDevice(KeyCode kc)
        {
            switch (kc)
            {
                case KeyCode.Mouse0:

                case KeyCode.Mouse1:

                case KeyCode.Mouse2:

                case KeyCode.Mouse3:

                case KeyCode.Mouse4:

                case KeyCode.Mouse5:

                case KeyCode.Mouse6:
                    return InputDevice.MOUSE;
                case KeyCode.JoystickButton0:

                case KeyCode.JoystickButton1:

                case KeyCode.JoystickButton2:

                case KeyCode.JoystickButton3:

                case KeyCode.JoystickButton4:

                case KeyCode.JoystickButton5:

                case KeyCode.JoystickButton6:

                case KeyCode.JoystickButton7:

                case KeyCode.JoystickButton8:

                case KeyCode.JoystickButton9:

                case KeyCode.JoystickButton10:

                case KeyCode.JoystickButton11:

                case KeyCode.JoystickButton12:

                case KeyCode.JoystickButton13:

                case KeyCode.JoystickButton14:

                case KeyCode.JoystickButton15:

                case KeyCode.JoystickButton16:

                case KeyCode.JoystickButton17:

                case KeyCode.JoystickButton18:

                case KeyCode.JoystickButton19:

                case KeyCode.Joystick1Button0:

                case KeyCode.Joystick1Button1:

                case KeyCode.Joystick1Button2:

                case KeyCode.Joystick1Button3:

                case KeyCode.Joystick1Button4:

                case KeyCode.Joystick1Button5:

                case KeyCode.Joystick1Button6:

                case KeyCode.Joystick1Button7:

                case KeyCode.Joystick1Button8:

                case KeyCode.Joystick1Button9:

                case KeyCode.Joystick1Button10:

                case KeyCode.Joystick1Button11:

                case KeyCode.Joystick1Button12:

                case KeyCode.Joystick1Button13:

                case KeyCode.Joystick1Button14:

                case KeyCode.Joystick1Button15:

                case KeyCode.Joystick1Button16:

                case KeyCode.Joystick1Button17:

                case KeyCode.Joystick1Button18:

                case KeyCode.Joystick1Button19:

                case KeyCode.Joystick2Button0:

                case KeyCode.Joystick2Button1:

                case KeyCode.Joystick2Button2:

                case KeyCode.Joystick2Button3:

                case KeyCode.Joystick2Button4:

                case KeyCode.Joystick2Button5:

                case KeyCode.Joystick2Button6:

                case KeyCode.Joystick2Button7:

                case KeyCode.Joystick2Button8:

                case KeyCode.Joystick2Button9:

                case KeyCode.Joystick2Button10:

                case KeyCode.Joystick2Button11:

                case KeyCode.Joystick2Button12:

                case KeyCode.Joystick2Button13:

                case KeyCode.Joystick2Button14:

                case KeyCode.Joystick2Button15:

                case KeyCode.Joystick2Button16:

                case KeyCode.Joystick2Button17:

                case KeyCode.Joystick2Button18:

                case KeyCode.Joystick2Button19:

                case KeyCode.Joystick3Button0:

                case KeyCode.Joystick3Button1:

                case KeyCode.Joystick3Button2:

                case KeyCode.Joystick3Button3:

                case KeyCode.Joystick3Button4:

                case KeyCode.Joystick3Button5:

                case KeyCode.Joystick3Button6:

                case KeyCode.Joystick3Button7:

                case KeyCode.Joystick3Button8:

                case KeyCode.Joystick3Button9:

                case KeyCode.Joystick3Button10:

                case KeyCode.Joystick3Button11:

                case KeyCode.Joystick3Button12:

                case KeyCode.Joystick3Button13:

                case KeyCode.Joystick3Button14:

                case KeyCode.Joystick3Button15:

                case KeyCode.Joystick3Button16:

                case KeyCode.Joystick3Button17:

                case KeyCode.Joystick3Button18:

                case KeyCode.Joystick3Button19:

                case KeyCode.Joystick4Button0:

                case KeyCode.Joystick4Button1:

                case KeyCode.Joystick4Button2:

                case KeyCode.Joystick4Button3:

                case KeyCode.Joystick4Button4:

                case KeyCode.Joystick4Button5:

                case KeyCode.Joystick4Button6:

                case KeyCode.Joystick4Button7:

                case KeyCode.Joystick4Button8:

                case KeyCode.Joystick4Button9:

                case KeyCode.Joystick4Button10:

                case KeyCode.Joystick4Button11:

                case KeyCode.Joystick4Button12:

                case KeyCode.Joystick4Button13:

                case KeyCode.Joystick4Button14:

                case KeyCode.Joystick4Button15:

                case KeyCode.Joystick4Button16:

                case KeyCode.Joystick4Button17:

                case KeyCode.Joystick4Button18:

                case KeyCode.Joystick4Button19:

                case KeyCode.Joystick5Button0:

                case KeyCode.Joystick5Button1:

                case KeyCode.Joystick5Button2:

                case KeyCode.Joystick5Button3:

                case KeyCode.Joystick5Button4:

                case KeyCode.Joystick5Button5:

                case KeyCode.Joystick5Button6:

                case KeyCode.Joystick5Button7:

                case KeyCode.Joystick5Button8:

                case KeyCode.Joystick5Button9:

                case KeyCode.Joystick5Button10:

                case KeyCode.Joystick5Button11:

                case KeyCode.Joystick5Button12:

                case KeyCode.Joystick5Button13:

                case KeyCode.Joystick5Button14:

                case KeyCode.Joystick5Button15:

                case KeyCode.Joystick5Button16:

                case KeyCode.Joystick5Button17:

                case KeyCode.Joystick5Button18:

                case KeyCode.Joystick5Button19:

                case KeyCode.Joystick6Button0:

                case KeyCode.Joystick6Button1:

                case KeyCode.Joystick6Button2:

                case KeyCode.Joystick6Button3:

                case KeyCode.Joystick6Button4:

                case KeyCode.Joystick6Button5:

                case KeyCode.Joystick6Button6:

                case KeyCode.Joystick6Button7:

                case KeyCode.Joystick6Button8:

                case KeyCode.Joystick6Button9:

                case KeyCode.Joystick6Button10:

                case KeyCode.Joystick6Button11:

                case KeyCode.Joystick6Button12:

                case KeyCode.Joystick6Button13:

                case KeyCode.Joystick6Button14:

                case KeyCode.Joystick6Button15:

                case KeyCode.Joystick6Button16:

                case KeyCode.Joystick6Button17:

                case KeyCode.Joystick6Button18:

                case KeyCode.Joystick6Button19:

                case KeyCode.Joystick7Button0:

                case KeyCode.Joystick7Button1:

                case KeyCode.Joystick7Button2:

                case KeyCode.Joystick7Button3:

                case KeyCode.Joystick7Button4:

                case KeyCode.Joystick7Button5:

                case KeyCode.Joystick7Button6:

                case KeyCode.Joystick7Button7:

                case KeyCode.Joystick7Button8:

                case KeyCode.Joystick7Button9:

                case KeyCode.Joystick7Button10:

                case KeyCode.Joystick7Button11:

                case KeyCode.Joystick7Button12:

                case KeyCode.Joystick7Button13:

                case KeyCode.Joystick7Button14:

                case KeyCode.Joystick7Button15:

                case KeyCode.Joystick7Button16:

                case KeyCode.Joystick7Button17:

                case KeyCode.Joystick7Button18:

                case KeyCode.Joystick7Button19:

                case KeyCode.Joystick8Button0:

                case KeyCode.Joystick8Button1:

                case KeyCode.Joystick8Button2:

                case KeyCode.Joystick8Button3:

                case KeyCode.Joystick8Button4:

                case KeyCode.Joystick8Button5:

                case KeyCode.Joystick8Button6:

                case KeyCode.Joystick8Button7:

                case KeyCode.Joystick8Button8:

                case KeyCode.Joystick8Button9:

                case KeyCode.Joystick8Button10:

                case KeyCode.Joystick8Button11:

                case KeyCode.Joystick8Button12:

                case KeyCode.Joystick8Button13:

                case KeyCode.Joystick8Button14:

                case KeyCode.Joystick8Button15:

                case KeyCode.Joystick8Button16:

                case KeyCode.Joystick8Button17:

                case KeyCode.Joystick8Button18:

                case KeyCode.Joystick8Button19:
                    return InputDevice.CONTROLLER;
                default:
                    return InputDevice.KEYBOARD;
            }
        }

        public void RecordSceneLatestDevice()
        {
            CrossSceneData = new InputManagerCrossSceneData(LatestInputDevice, latestInputDeviceKeyboardOrGamepad);
        }

        public void ApplyPreviousSceneLatestDevice()
        {
            if (!CrossSceneData.HasValue) return;
            LatestInputDevice = CrossSceneData.Value.LatestInputDevice;
            latestInputDeviceKeyboardOrGamepad = CrossSceneData.Value.latestInputDeviceKeyboardOrMouse;
            CrossSceneData = null;
        }

    }

    public struct InputManagerCrossSceneData 
    {
        public InputManager.InputDevice? latestInputDeviceKeyboardOrMouse;
        public InputManager.InputDevice? LatestInputDevice;

        public InputManagerCrossSceneData(InputManager.InputDevice? latestInputDevice, InputManager.InputDevice? latestInputDeviceKeyboardOrMouse)
        {
            this.latestInputDeviceKeyboardOrMouse = latestInputDeviceKeyboardOrMouse;
            LatestInputDevice = latestInputDevice;
        }
    }

    public class HeartKeys
    {
        public const int KEY_UP = 100000;
        public const int KEY_LEFT = 100001;
        public const int KEY_DOWN = 100002;
        public const int KEY_RIGHT = 100003;
        public const int KEY_ENTER = 100004;
        public const int KEY_SPACE = 100005;
        public const int KEY_ESCAPE = 100006;
        public const int KEY_TAB = 100007;
        public const int KEY_SHIFT = 100008;
        public const int JOY_BUTTON_SOUTH = 100051; // X
        public const int JOY_BUTTON_EAST = 100052; // O
        public const int JOY_BUTTON_WEST = 100053; // ■
        public const int JOY_BUTTON_NORTH = 100054; // ▲
        public const int JOY_BUTTON_L = 100055;
        public const int JOY_BUTTON_R = 100056;
        public const int JOY_BUTTON_R2 = 100057;
        public const int JOY_BUTTON_L2 = 100058;
        public const int JOY_BUTTON_D_UP = 100059;
        public const int JOY_BUTTON_D_LEFT = 100060;
        public const int JOY_BUTTON_D_DOWN = 100061;
        public const int JOY_BUTTON_D_RIGHT = 100062;
        public const int JOY_BUTTON_START = 100063;
        public const int JOY_BUTTON_SELECT = 100064;
        public const int MOUSE_BUTTON_LEFT = 101001;
        public const int MOUSE_BUTTON_RIGHT = 101002;

    }

    // maybe will use this for stick
    public class ContinuousStateKey
    {
        public bool pressedLastFrame;
    }

    public enum GamepadType
    {
        INVALID,
        PLAYSTATION,
        XBOX,
        SWITCH,
        STEAM_DECK
    }


    public enum DefaultButtons
    {
        INVALID = 10000,
        LEFT = 10001,
        RIGHT = 10002,
        UP = 10003,
        DOWN = 10004,
        CONFIRM = 10005,
        CANCEL = 10006,
        SUBACTION1 = 10007,
        SUBACTION2 = 10008,
        LEFT_TRIGGER = 10009,
        RIGHT_TRIGGER = 10010,
        LEFT_TRIGGER_2 = 10011,
        RIGHT_TRIGGER_2 = 10012,
        START = 10013,
    }


}