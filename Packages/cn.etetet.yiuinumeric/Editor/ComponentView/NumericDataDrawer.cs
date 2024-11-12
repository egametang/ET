#if ENABLE_VIEW

using System;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [TypeDrawer]
    public class NumericDataDrawer : ITypeDrawer
    {
        public bool HandlesType(Type type)
        {
            return type == typeof(NumericData);
        }

        public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
        {
            if (value is not NumericData numericData) return value;

            EditorGUILayout.LabelField("数值:");

            if (GUILayout.Button("GM调试"))
            {
                NumericGMWindow.SwitchWindow(numericData);
            }

            using var dicPool = ObjectPool.Fetch<NumericDictionaryPool<int, long>>();

            numericData.GetNumericDic(dicPool);

            foreach ((int k, long v) in dicPool)
            {
                if (v == 0) continue;
                var kType     = (ENumericType)k;
                var keyName   = kType.GetLocalizationName();
                var valueType = kType.GetNumericValueType();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"    {keyName} [{k}]({valueType})");
                EditorGUILayout.LabelField($"{v}");
                GUILayout.EndHorizontal();
            }

            return value;
        }
    }
}

#endif
