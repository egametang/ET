using System;
using UnityEditor;

namespace ET
{
    [TypeDrawer]
    public class BoolTypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type == typeof (bool);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.Toggle(memberName, (bool) value);
        }
    }
}