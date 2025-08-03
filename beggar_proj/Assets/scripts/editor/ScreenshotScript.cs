#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeggarEditor
{
    [Serializable]
    public class ScreenshotCommand
    {
        public enum CommandType
        {
            LOAD_SAVE,
            START_GAME,
            EXPAND,
            SCREENSHOT,
            WAIT,
            ACTIVATE_TAB,
            UNKNOWN
        }

        public CommandType Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public float WaitTime { get; set; }

        public static ScreenshotCommand Parse(string line)
        {
            line = line.Trim();
            if (!line.StartsWith(">")) return null;
            
            line = line.Substring(1).Trim();
            var parts = line.Split(' ');
            if (parts.Length == 0) return null;

            var command = new ScreenshotCommand();
            var commandName = parts[0].ToLower();

            switch (commandName)
            {
                case "load_save":
                    command.Type = CommandType.LOAD_SAVE;
                    break;
                case "start_game":
                    command.Type = CommandType.START_GAME;
                    break;
                case "expand":
                    command.Type = CommandType.EXPAND;
                    break;
                case "screenshot":
                    command.Type = CommandType.SCREENSHOT;
                    break;
                case "wait":
                    command.Type = CommandType.WAIT;
                    break;
                case "activate_tab":
                    command.Type = CommandType.ACTIVATE_TAB;
                    break;
                default:
                    command.Type = CommandType.UNKNOWN;
                    break;
            }

            // Parse parameters
            for (int i = 1; i < parts.Length; i++)
            {
                var part = parts[i];
                if (part.Contains(":"))
                {
                    var kvp = part.Split(':');
                    if (kvp.Length == 2)
                    {
                        command.Parameters[kvp[0]] = kvp[1];
                    }
                }
                else if (command.Type == CommandType.WAIT && part.EndsWith("s"))
                {
                    // Parse wait time
                    if (float.TryParse(part.Substring(0, part.Length - 1), out float waitTime))
                    {
                        command.WaitTime = waitTime;
                    }
                }
            }

            return command;
        }
    }

    public class ScreenshotScript
    {
        public List<ScreenshotCommand> Commands { get; set; } = new List<ScreenshotCommand>();

        public static ScreenshotScript Parse(string scriptText)
        {
            var script = new ScreenshotScript();
            var lines = scriptText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var command = ScreenshotCommand.Parse(line);
                if (command != null && command.Type != ScreenshotCommand.CommandType.UNKNOWN)
                {
                    script.Commands.Add(command);
                }
            }

            return script;
        }
    }
}
#endif