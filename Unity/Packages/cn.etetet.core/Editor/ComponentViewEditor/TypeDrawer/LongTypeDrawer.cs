using System;
using UnityEditor;

namespace ET
{
    [TypeDrawer]
    public class LongTypeDrawer: ITypeDrawer
    {
        [TypeDrawer]
        public bool HandlesType(Type type)
        {
            return type == typeof (long);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.LongField(memberName, (long) value);
        }
    }
}