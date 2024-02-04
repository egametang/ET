#if ENABLE_VIEW

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [TypeDrawer]
    public class EntityRefTypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            if (type.GetGenericTypeDefinition() == typeof (EntityRef<>))
            {
                return true;
            }

            return false;
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            FieldInfo fieldInfo = memberType.GetField("entity", BindingFlags.NonPublic | BindingFlags.Instance);
            Entity entity = (Entity)fieldInfo.GetValue(value);
            GameObject go = entity?.ViewGO;
            EditorGUILayout.ObjectField(memberName, go, memberType, true);
            return value;
        }
    }
}

#endif