#if ENABLE_VIEW
using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace ET
{
    [CustomEditor(typeof (ComponentView))]
    public class ComponentViewEditor: Editor
    {
        public override void OnInspectorGUI()
        {
            ComponentView componentView = (ComponentView) target;
            Entity component = componentView.Component;
            ComponentViewHelper.Draw(component);
        }
    }

    public static class ComponentViewHelper
    {
        private static readonly List<ITypeDrawer> typeDrawers = new List<ITypeDrawer>();

        static ComponentViewHelper()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
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
        }
        
        public static void Draw(Entity entity)
        {
            try
            {
                FieldInfo[] fields = entity.GetType()
                        .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                EditorGUILayout.BeginVertical();
                
                EditorGUILayout.LongField("InstanceId: ", entity.InstanceId);
                
                EditorGUILayout.LongField("Id: ", entity.Id);

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

                    object value = fieldInfo.GetValue(entity);

                    foreach (ITypeDrawer typeDrawer in typeDrawers)
                    {
                        if (!typeDrawer.HandlesType(type))
                        {
                            continue;
                        }

                        string fieldName = fieldInfo.Name;
                        if (fieldName.Length > 17 && fieldName.Contains("k__BackingField"))
                        {
                            fieldName = fieldName.Substring(1, fieldName.Length - 17);
                        }

                        try
                        {
                            value = typeDrawer.DrawAndGetNewValue(type, fieldName, value, null);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                        
                        fieldInfo.SetValue(entity, value);
                        break;
                    }
                }

                EditorGUILayout.EndVertical();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"component view error: {entity.GetType().FullName} {e}");
            }
        }
    }
}
#endif