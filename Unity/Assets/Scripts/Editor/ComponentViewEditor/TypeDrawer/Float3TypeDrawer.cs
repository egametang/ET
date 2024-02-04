using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [TypeDrawer]
    public class Float3TypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type == typeof (float3);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            Vector3 v = (float3)value;
            return new float3(EditorGUILayout.Vector3Field(memberName, v));
        }
    }
}