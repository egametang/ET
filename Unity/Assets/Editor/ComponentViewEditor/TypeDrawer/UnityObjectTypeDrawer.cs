using System;
using UnityEditor;

namespace ET
{
    [TypeDrawer]
    public class UnityObjectTypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type == typeof (UnityEngine.Object) ||
                    type.IsSubclassOf(typeof (UnityEngine.Object));
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.ObjectField(memberName, (UnityEngine.Object) value, memberType, true);
        }
    }
}