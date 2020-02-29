using System;
using UnityEditor;

namespace ET
{
    [TypeDrawer]
    public class StringTypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type == typeof (string);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.DelayedTextField(memberName, (string) value);
        }
    }
}