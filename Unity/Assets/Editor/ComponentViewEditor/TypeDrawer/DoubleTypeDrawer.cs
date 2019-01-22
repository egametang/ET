using System;
using UnityEditor;

namespace ETEditor
{
    [TypeDrawer]
    public class DoubleTypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type == typeof (double);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.DoubleField(memberName, (double) value);
        }
    }
}