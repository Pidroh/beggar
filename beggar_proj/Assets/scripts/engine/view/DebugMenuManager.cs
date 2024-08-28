//using UnityEngine.U2D;

using System;
using TMPro;
using UnityEngine;

namespace HeartUnity.View
{
    public class DebugMenuManager
    {
        private DebugMenu debugMenu;

        public void InitDebugMenu()
        {
            if (debugMenu == null){
                var debugMenu = Resources.Load<DebugMenu>("DebugMenu");
                this.debugMenu = GameObject.Instantiate(debugMenu);

            }
                
        }
        public void ManualUpdate()
        {
            if (Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.O) && Input.GetKeyDown(KeyCode.J))
            {
                InitDebugMenu();
                debugMenu.Show(true);
            }
            if (debugMenu != null && debugMenu.IsShowing)
            {
                if (Input.GetKey(KeyCode.Escape))
                {
                    debugMenu.Show(false);
                }
            }
        }

        public static DebugMenuManager Instance;

        public static bool CheckCommand(string v)
        {
            if (!CheckValid()) return false;
            return Instance.debugMenu.currentDebugMessage.Trim() == v;
            //return Instance.debugMenu.currentDebugMessage.IndexOf(v.Trim()) == 0;
        }

        public static bool CheckCommand(string v, out int number)
        {
            number = -1;
            if (!CheckValid()) return false;
            if (Instance.debugMenu.currentDebugMessage.Contains(v) && Instance.debugMenu.currentDebugMessage.Length > v.Length) {
                number = int.Parse(Instance.debugMenu.currentDebugMessage.Replace(v, "").Trim());
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