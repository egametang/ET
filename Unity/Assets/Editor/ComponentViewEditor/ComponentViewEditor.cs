using System;
using System.Collections.Generic;
using System.Reflection;
using ETModel;
using ILRuntime.Runtime.Intepreter;
using UnityEditor;
using UnityEngine;
using Component = ETModel.Component;

namespace ETEditor
{
    [CustomEditor(typeof (ComponentView))]
    public class ComponentViewEditor: Editor
    {
        public override void OnInspectorGUI()
        {
            ComponentView componentView = (ComponentView) target;
            object component = componentView.Component;
            if (component.GetType() == typeof (ILTypeInstance))
            {
                return;
            }
            ComponentViewHelper.Draw(component);
        }
    }

    public static class ComponentViewHelper
    {
        private static readonly List<ITypeDrawer> typeDrawers = new List<ITypeDrawer>();

        static ComponentViewHelper()
        {
            Assembly assembly = typeof (ComponentViewHelper).Assembly;
            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsDefined(typeof (TypeDrawerAttribute)))
                {
                    continue;
                }

                ITypeDrawer iTypeDrawer = (ITypeDrawer) Activator.CreateInstance(type);
                typeDrawers.Add(iTypeDrawer);
            }
        }
        
        public static void Draw(object obj)
        {
            try
            {
                FieldInfo[] fields = obj.GetType()
                        .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                EditorGUILayout.BeginVertical();

                foreach (FieldInfo fieldInfo in fields)
                {
                    Type type = fieldInfo.FieldType;
                    if (type.IsDefined(typeof (HideInInspector), false))
                    {
                        continue;
                    }

                    if (fieldInfo.IsDefined(typeof (HideInInspector), false))
                    {
                        continue;
                    }

                    object value = fieldInfo.GetValue(obj);

                    foreach (ITypeDrawer typeDrawer in typeDrawers)
                    {
                        if (!typeDrawer.HandlesType(type))
                        {
                            continue;
                        }

                        value = typeDrawer.DrawAndGetNewValue(type, fieldInfo.Name, value, null);
                        fieldInfo.SetValue(obj, value);
                        break;
                    }
                }

                EditorGUILayout.EndVertical();
            }
            catch (Exception e)
            {
                Log.Error($"component view error: {obj.GetType().FullName}");
            }
        }
    }
}