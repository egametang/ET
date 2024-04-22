using System;
using UnityEditor;

namespace ET
{
    [TypeDrawer]
    public class DateTimeTypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type == typeof (DateTime);
        }

        // Note: This is a very basic implementation. The ToString() method conversion will cut off milliseconds.
        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            var dateString = value.ToString();
            var newDateString = EditorGUILayout.TextField(memberName, dateString);

            return newDateString != dateString
                    ? DateTime.Parse(newDateString)
                    : value;
        }
    }
}