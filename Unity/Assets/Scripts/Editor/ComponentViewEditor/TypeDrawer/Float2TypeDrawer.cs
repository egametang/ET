using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [TypeDrawer]
    public class Float2TypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type == typeof (float2);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            Vector2 v = (float2)value;
            return new float2(EditorGUILayout.Vector2Field(memberName, v));
        }
    }
}