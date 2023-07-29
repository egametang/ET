using System;
using UnityEditor;

namespace ET
{
    [TypeDrawer]
    public class EnumTypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type.IsEnum;
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            if (memberType == typeof (SceneType))
            {
                string sceneType = EditorGUILayout.DelayedTextField(memberName, value.ToString());
                return EnumHelper.FromString<SceneType>(sceneType);
            }
            
            if (memberType.IsDefined(typeof (FlagsAttribute), false))
            {
                return EditorGUILayout.EnumFlagsField(memberName, (Enum) value);
            }

            return EditorGUILayout.EnumPopup(memberName, (Enum) value);
        }
    }
}