using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [TypeDrawer]
    public class QuaternionTypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type == typeof (quaternion);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            Vector4 v = ((quaternion)value).value;
            return new quaternion(EditorGUILayout.Vector4Field(memberName, v));
        }
    }
}