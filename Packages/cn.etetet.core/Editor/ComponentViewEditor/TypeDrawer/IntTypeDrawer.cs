using System;
using UnityEditor;

namespace ET
{
    [TypeDrawer]
    public class IntTypeDrawer: ITypeDrawer
    {
        [TypeDrawer]
        public bool HandlesType(Type type)
        {
            return type == typeof (int);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.IntField(memberName, (int) value);
        }
    }
}