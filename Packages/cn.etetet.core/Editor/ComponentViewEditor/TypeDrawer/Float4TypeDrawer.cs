using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [TypeDrawer]
    public class Float4TypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type == typeof (float4);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            Vector4 v = (float4)value;
            return new float4(EditorGUILayout.Vector4Field(memberName, v));
        }
    }
}