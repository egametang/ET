using System;
using System.Collections.Generic;
using UnityEditor;

namespace ET
{
    [TypeDrawer]
    public class DictionaryIntLongTypeDrawer: ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type == typeof (Dictionary<int, long>);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            Dictionary<int, long> dictionary = value as Dictionary<int, long>;

            EditorGUILayout.LabelField($"{memberName}:");
            foreach ((int k, long v) in dictionary)
            {
                if (v == 0)
                {
                    continue;
                }
                EditorGUILayout.LongField($"    {k} :", v);
            }
            return value;
        }
    }
}