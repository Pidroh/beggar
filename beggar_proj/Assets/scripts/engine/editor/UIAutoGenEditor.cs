using UnityEngine;
using UnityEditor;
using System.IO;
using HeartUnity.View;
using TMPro;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace autogen.ui
{
    public static class UIAutoGenEditor
    {
        [MenuItem("Tools/UI AutoGen/Generate UI Code")]
        static void GenerateUICode()
        {
            GameObject selectedObject = Selection.activeGameObject;

            if (selectedObject == null)
            {
                Debug.LogError("Please select a GameObject in the scene.");
                return;
            }

            string scriptPath = "Assets/Scripts/autogen-ui/";
            string scriptName = selectedObject.name + "UI.cs";
            string fullPath = scriptPath + scriptName;

            // Check if the folder exists, create it if not
            if (!Directory.Exists(scriptPath))
            {
                Directory.CreateDirectory(scriptPath);
            }

            string scriptContent = GenerateScriptContent(selectedObject);

            File.WriteAllText(fullPath, scriptContent);

            // Regenerate CentralUI
            RegenerateCentralUI();

            AssetDatabase.Refresh();
            Debug.Log("UI code generated at: " + fullPath);


        }

        static string GenerateScriptContent(GameObject selectedObject)
        {
            string className = selectedObject.name + "UI";
            string scriptContent = $"using UnityEngine;\nusing TMPro;\nusing UnityEngine.UI;\nusing HeartUnity.View;\n";
            scriptContent += $"namespace autogen.ui\n";
            scriptContent += "{\n";
            scriptContent += $"\tpublic class {className} : MonoBehaviour\n";
            scriptContent += "\t{\n";

            GenerateFieldCode(selectedObject.transform, ref scriptContent);

            scriptContent += "\t}\n";
            scriptContent += "}\n";

            return scriptContent;
        }

        static void GenerateFieldCode(Transform parent, ref string scriptContent)
        {
            foreach (Transform child in parent)
            {
                child.name = child.name.Replace(" ", "_");
                child.name = child.name.Replace("(", "_");
                child.name = child.name.Replace(")", "_");

                Component[] components = child.GetComponents<Component>();
                Type highestPriorityType = GetHighestPriorityType(components);

                if (highestPriorityType != null)
                {
                    string fieldType = GetFieldType(highestPriorityType);
                    scriptContent += $"\t\tpublic {fieldType} {child.name};\n";
                }
                else
                {
                    scriptContent += $"\t\tpublic Transform {child.name};\n";
                }
            }
        }

        static Type GetHighestPriorityType(Component[] components)
        {
            Type highestPriorityType = null;
            int highestPriority = int.MinValue;

            foreach (var component in components)
            {
                int priority = GetFieldTypePriority(component.GetType());
                if (priority > highestPriority)
                {
                    highestPriority = priority;
                    highestPriorityType = component.GetType();
                }
            }

            return highestPriorityType;
        }

        static int GetFieldTypePriority(Type fieldType)
        {
            if (fieldType == typeof(IconButton))
            {
                return 6;
            }
            if (fieldType == typeof(AnimationUnitList))
            {
                return 7;
            }
            if (fieldType == typeof(SpriteAnimation))
            {
                return 8;
            }
            if (fieldType == typeof(ScrollManager))
            {
                return 9;
            }
            if (fieldType == typeof(UIUnit))
            {
                return 5;
            }
            else if (fieldType == typeof(TextMeshProUGUI))
            {
                return 4;
            }
            else if (fieldType == typeof(UnityEngine.UI.Button))
            {
                return 3;
            }
            else if (fieldType == typeof(UnityEngine.UI.Image))
            {
                return 2;
            }
            else
            {
                return 1; // Transform has the lowest priority
            }
        }

        static string GetFieldType(Type fieldType)
        {
            if (fieldType == typeof(ScrollManager))
            {
                return "ScrollManager";
            }
            if (fieldType == typeof(SpriteAnimation))
            {
                return "SpriteAnimation";
            }
            if (fieldType == typeof(AnimationUnitList))
            {
                return "AnimationUnitList";
            }
            if (fieldType == typeof(IconButton))
            {
                return "IconButton";
            }
            if (fieldType == typeof(UIUnit))
            {
                return "UIUnit";
            }
            else if (fieldType == typeof(TextMeshProUGUI))
            {
                return "TextMeshProUGUI";
            }
            else if (fieldType == typeof(UnityEngine.UI.Button))
            {
                return "Button";
            }
            else if (fieldType == typeof(UnityEngine.UI.Image))
            {
                return "Image";
            }
            else
            {
                return "Transform";
            }
        }

        [MenuItem("Tools/UI AutoGen/Auto Assign UI")]
        static void AutoAssignUI()
        {
            GameObject selectedObject = Selection.activeGameObject;

            if (selectedObject == null)
            {
                Debug.LogError("Please select a GameObject in the scene.");
                return;
            }

            string className = selectedObject.name + "UI";
            string scriptPath = "Assets/Scripts/autogen-ui/";
            string scriptName = className + ".cs";
            string fullPath = scriptPath + scriptName;

            // Check if the script exists
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"No generated UI class found for {selectedObject.name}.");
                return;
            }

            MonoScript scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(fullPath);

            if (scriptAsset != null)
            {
                Type uiType = scriptAsset.GetClass();

                if (uiType != null)
                {
                    MonoBehaviour uiInstance = selectedObject.GetComponent(uiType) as MonoBehaviour;

                    if (uiInstance == null)
                    {
                        uiInstance = selectedObject.AddComponent(uiType) as MonoBehaviour;
                        AssignFields(selectedObject, uiInstance);
                        Debug.Log($"UI assigned to {selectedObject.name}.");
                    }
                    else
                    {
                        AssignFields(selectedObject, uiInstance);
                        Debug.Log($"{selectedObject.name} already has a UI assigned.");
                    }
                }
            }

            AutoAssignAllUI();

        }

        static void AssignFields(GameObject selectedObject, MonoBehaviour uiInstance)
        {
            FieldInfo[] fields = uiInstance.GetType().GetFields();

            foreach (FieldInfo field in fields)
            {
                Transform childTransform = selectedObject.transform.Find(field.Name);

                if (childTransform != null)
                {
                    Component component = childTransform.GetComponent(field.FieldType);

                    if (component != null)
                    {
                        field.SetValue(uiInstance, component);
                    }
                }
            }
        }

        static void RegenerateCentralUI()
        {
            string centralUIScriptPath = "Assets/Scripts/autogen-ui/";
            string centralUIScriptName = "UIAutoElements.cs";
            string centralUIFullPath = centralUIScriptPath + centralUIScriptName;

            // Check if the folder exists, create it if not
            if (!Directory.Exists(centralUIScriptPath))
            {
                Directory.CreateDirectory(centralUIScriptPath);
            }

            string centralUIScriptContent = GenerateCentralUIScriptContent();

            File.WriteAllText(centralUIFullPath, centralUIScriptContent);

            AssetDatabase.Refresh();
            Debug.Log("Central UI code regenerated at: " + centralUIFullPath);
        }

        static string GenerateCentralUIScriptContent()
        {
            string scriptContent = $"using UnityEngine;\n\n";
            scriptContent += $"namespace autogen.ui\n";
            scriptContent += "{\n";
            scriptContent += $"\tpublic class UIAutoElements : MonoBehaviour\n";
            scriptContent += "\t{\n";

            // Add fields for each generated UI class
            foreach (var uiClassName in GetGeneratedUIClassNames())
            {
                scriptContent += $"\t\tpublic {uiClassName} {uiClassName};\n";
            }

            scriptContent += "\t}\n";
            scriptContent += "}\n";

            return scriptContent;
        }

        static string[] GetGeneratedUIClassNames()
        {
            string scriptPath = "Assets/Scripts/autogen-ui/";
            var scriptFiles = Directory.GetFiles(scriptPath, "*UI.cs");

            var classNames = new List<string>();

            foreach (var scriptFile in scriptFiles)
            {
                var className = Path.GetFileNameWithoutExtension(scriptFile);
                classNames.Add(className);
            }

            return classNames.ToArray();
        }
        [MenuItem("Tools/UI AutoGen/Auto Assign All UI")]
        static void AutoAssignAllUI()
        {
            GameObject selectedObject = Selection.activeGameObject;

            if (selectedObject == null)
            {
                Debug.LogError("Please select a GameObject in the scene.");
                return;
            }

            string className = selectedObject.name + "UI";
            string scriptPath = "Assets/Scripts/autogen-ui/";
            string scriptName = className + ".cs";
            string fullPath = scriptPath + scriptName;

            // Check if the script exists
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"No generated UI class found for {selectedObject.name}.");
                return;
            }

            MonoScript scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(fullPath);

            if (scriptAsset != null)
            {
                Type uiType = scriptAsset.GetClass();

                if (uiType != null)
                {
                    // Find all objects in the scene
                    GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>();

                    foreach (GameObject obj in sceneObjects)
                    {
                        AssignToNullFields(obj, selectedObject, uiType);
                    }

                    Debug.Log($"Auto assigned {selectedObject.name} to all objects with null UI fields.");
                }
            }
            EditorUtility.SetDirty(selectedObject);


        }

        static void AssignToNullFields(GameObject obj, GameObject selectedObject, Type uiType)
        {
            MonoBehaviour[] monoBehaviours = obj.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour monoBehaviour in monoBehaviours)
            {
                FieldInfo[] fields = monoBehaviour.GetType().GetFields();

                foreach (FieldInfo field in fields)
                {
                    // Check if the field is of the same type as the autogenerated UI code
                    if (field.FieldType == uiType)
                    {
                        // Get the current value of the field
                        object fieldValue = field.GetValue(monoBehaviour);

                        // If the field is null, assign the selected object to it
                        if (fieldValue == null)
                        {
                            field.SetValue(monoBehaviour, selectedObject.GetComponent(uiType));
                        }
                    }
                }
            }
        }
    }
}
