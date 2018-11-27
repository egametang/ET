using System;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
    [TypeDrawer]
    public class ColorTypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type == typeof (Color);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.ColorField(memberName, (Color) value);
        }
    }
}