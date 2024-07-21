using System;
using System.IO;
using UnityEngine;

namespace HeartUnity
{
    public class EnumMaker : MonoBehaviour {
        public TextAsset[] enumSources;
        public string subFolder = "/scripts/autogen/";

        [ContextMenu("Generate enums")]
        public void Generate() {
            foreach (var item in enumSources)
            {
                var content = item.text;
                var name = item.name;
                var extensionClass = $"public static class {name}Extensions {{{Environment.NewLine} public static int ToInt(this {name} value){Environment.NewLine}  {{  return value switch {{ ";
                var classText = $"public enum {name} {{{Environment.NewLine}";
                var enums = content.Split('\n');
                for (int i = 0; i < enums.Length; i++)
                {
                    string enumValue = enums[i];
                    classText += $"  {enumValue},{Environment.NewLine}";
                    extensionClass += $"{Environment.NewLine} {name}.{enumValue} => {i},";
                }
                classText += "\n_max";
                classText += $"{Environment.NewLine}}}";
                extensionClass += $"{Environment.NewLine}{name}._max => {enums.Length},{Environment.NewLine}{Environment.NewLine}_ => -1{Environment.NewLine}}};}}}}";

                File.WriteAllText(Application.dataPath + subFolder + name + ".cs",$"namespace autogen {{{classText}{Environment.NewLine}{Environment.NewLine}{extensionClass}}}");
            }
        }
    }

}