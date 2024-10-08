//using UnityEngine.U2D;
using System;

namespace HeartUnity.View
{
    public static class InputDefaultLabels
    {
        public static string GetDefaultInputLabel(int key, GamepadType gamepadType)
        {
            if (key < char.MaxValue)
            {
                return ((char)key).ToString();
            }
            else
            {
                switch (gamepadType)
                {
                    case GamepadType.PLAYSTATION:
                        if (key == HeartKeys.JOY_BUTTON_SOUTH) return "X";
                        if (key == HeartKeys.JOY_BUTTON_EAST) return "O";
                        break;
                    case GamepadType.XBOX:
                        if (key == HeartKeys.JOY_BUTTON_SOUTH) return "A";
                        if (key == HeartKeys.JOY_BUTTON_EAST) return "B";
                        if (key == HeartKeys.JOY_BUTTON_WEST) return "X";
                        if (key == HeartKeys.JOY_BUTTON_NORTH) return "Y";
                        break;
                    case GamepadType.STEAM_DECK:
                        if (key == HeartKeys.JOY_BUTTON_SOUTH) return "A";
                        if (key == HeartKeys.JOY_BUTTON_EAST) return "B";
                        if (key == HeartKeys.JOY_BUTTON_WEST) return "X";
                        if (key == HeartKeys.JOY_BUTTON_NORTH) return "Y";
                        break;
                    case GamepadType.SWITCH:
                        if (key == HeartKeys.JOY_BUTTON_SOUTH) return "B";
                        if (key == HeartKeys.JOY_BUTTON_EAST) return "A";
                        if (key == HeartKeys.JOY_BUTTON_WEST) return "Y";
                        if (key == HeartKeys.JOY_BUTTON_NORTH) return "X";
                        break;
                    default:
                        break;
                }
                switch (key)
                {
                    case HeartKeys.JOY_BUTTON_SOUTH:
                        
                    default:
                        break;
                }

            }
            return null;
        }
    }


}