//using UnityEngine.U2D;

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HeartUnity.View
{
    public class DebugMenuManager
    {
        private DebugMenu debugMenu;
        public List<DebugCommandGamepad> gamepadCommands = new();
        public enum DebugCommandGamepad 
        {
            NORTH, SOUTH, WEST
        }

        public void InitDebugMenu()
        {
            if (debugMenu == null){
                var debugMenu = Resources.Load<DebugMenu>("DebugMenu");
                this.debugMenu = GameObject.Instantiate(debugMenu);

            }
                
        }
        public void ManualUpdate()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (debugMenu != null && debugMenu.IsShowing && Gamepad.current != null)
            {
                var lengthPrevious = gamepadCommands.Count;
                if (Gamepad.current.buttonSouth.wasPressedThisFrame) 
                {
                    gamepadCommands.Add(DebugCommandGamepad.SOUTH);
                    debugMenu.mainDebugField.text += "s";
                }
                if (Gamepad.current.buttonWest.wasPressedThisFrame)
                {
                    gamepadCommands.Add(DebugCommandGamepad.WEST);
                    debugMenu.mainDebugField.text += "w";
                }
                if (Gamepad.current.buttonNorth.wasPressedThisFrame)
                {
                    gamepadCommands.Add(DebugCommandGamepad.NORTH);
                    debugMenu.mainDebugField.text += "n";
                }
                if (lengthPrevious != gamepadCommands.Count && gamepadCommands.Count >= 3) 
                {
                    debugMenu.currentDebugMessage = "";
                    for (int i = 0; i < 3; i++)
                    {
                        switch (gamepadCommands[i])
                        {
                            case DebugCommandGamepad.NORTH:
                                debugMenu.currentDebugMessage += "n";
                                break;
                            case DebugCommandGamepad.SOUTH:
                                debugMenu.currentDebugMessage += "s";
                                break;
                            case DebugCommandGamepad.WEST:
                                debugMenu.currentDebugMessage += "w";
                                break;
                            default:
                                break;
                        }
                    }
                    gamepadCommands.Clear();
                    debugMenu.mainDebugField.text = "";


                }
            }
            
            if (Gamepad.current != null && Gamepad.current.leftTrigger.IsPressed() && Gamepad.current.rightTrigger.IsPressed() && Gamepad.current.buttonSouth.wasPressedThisFrame) 
            {
                InitDebugMenu();
                debugMenu.Show(true);
                gamepadCommands.Clear();
            }
            
#endif
            
            if (InputWrapper.GetKey(KeyCode.P) && InputWrapper.GetKey(KeyCode.O) && InputWrapper.GetKeyDown(KeyCode.J))
            {
                InitDebugMenu();
                debugMenu.Show(true);
                gamepadCommands.Clear();
            }
            if (debugMenu != null && debugMenu.IsShowing)
            {
                var leavingGamepad = Gamepad.current == null ? false : Gamepad.current.buttonEast.wasPressedThisFrame;
                if (InputWrapper.GetKey(KeyCode.Escape) || leavingGamepad)
                {
                    debugMenu.Show(false);
                    gamepadCommands.Clear();
                }
            }
        }

        public static DebugMenuManager Instance;

        public static bool CheckCommand(string v)
        {
            if (!CheckValid()) return false;
            bool result = Instance.debugMenu.currentDebugMessage.Trim() == v;
            if (result) Instance.debugMenu.currentDebugMessage = null;
            return result;
            //return Instance.debugMenu.currentDebugMessage.IndexOf(v.Trim()) == 0;
        }

        public static bool CheckCommand(string v, out int number)
        {
            number = -1;
            if (!CheckValid()) return false;
            if (Instance.debugMenu.currentDebugMessage.Contains(v) && Instance.debugMenu.currentDebugMessage.Length > v.Length) {
                number = int.Parse(Instance.debugMenu.currentDebugMessage.Replace(v, "").Trim());
                Instance.debugMenu.currentDebugMessage = null;
                return true;
            }
            return false;
        }

        public static bool CheckCommand(string command, out string label, out int number)
        {
            label = string.Empty;
            number = -1;
            if (!CheckValid()) return false;
            if (Instance.debugMenu.currentDebugMessage.Contains(command))
            {
                string[] parts = Instance.debugMenu.currentDebugMessage.Split(',');
                if (parts.Length == 3)
                {
                    label = parts[1].Trim();
                    number = int.Parse(parts[2].Trim());
                    Instance.debugMenu.currentDebugMessage = null;
                    return true;
                }
            }
            return false;
        }



        private static bool CheckValid()
        {
            if (Instance == null) return false;
            if (Instance.debugMenu == null) return false;
            if (Instance.debugMenu.currentDebugMessage == null) return false;
            return true;
        }
    }
}