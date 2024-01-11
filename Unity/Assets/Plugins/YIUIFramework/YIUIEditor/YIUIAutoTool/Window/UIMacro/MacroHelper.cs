#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    /// <summary>
    /// 宏助手
    /// </summary>
    public static class MacroHelper
    {
        public static List<string> GetSymbols(BuildTargetGroup targetGroup)
        {
            var ori = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            return new List<string>(ori.Split(';'));
        }

        public static void SetSymbols(List<string>     defineSymbols,
                                      BuildTargetGroup targetGroup)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineSymbols.ToArray()));
        }

        /// <summary>
        /// 一次改变最终结果
        /// 防止多次调用set
        /// </summary>
        public static void ChangeSymbols(List<string> allRemove, List<string> allSelect, BuildTargetGroup targetGroup)
        {
            var defineSymbols = GetSymbols(targetGroup);

            for (int i = defineSymbols.Count - 1; i >= 0; i--)
            {
                var data = defineSymbols[i];
                foreach (var remove in allRemove)
                {
                    if (data == remove)
                    {
                        defineSymbols.Remove(data);
                        break;
                    }
                }
            }

            foreach (var data in defineSymbols)
            {
                foreach (var add in allSelect)
                {
                    if (data == add)
                    {
                        allSelect.Remove(add);
                        break;
                    }
                }
            }

            defineSymbols.AddRange(allSelect);

            SetSymbols(defineSymbols, targetGroup);
        }

        /// <summary>
        /// 添加宏
        /// </summary>
        public static void AddMacro(string macro, BuildTargetGroup targetGroup)
        {
            if (string.IsNullOrEmpty(macro))
            {
                return;
            }

            var defineSymbols = GetSymbols(targetGroup);

            foreach (var data in defineSymbols)
            {
                if (data == macro)
                {
                    Debug.LogError($"此宏已存在  {macro}");
                    return;
                }
            }

            defineSymbols.Add(macro);

            SetSymbols(defineSymbols, targetGroup);
        }

        /// <summary>
        /// 添加宏
        /// </summary>
        public static void AddMacro(List<string> macro, BuildTargetGroup targetGroup)
        {
            if (macro == null || macro.Count <= 0)
            {
                return;
            }

            var defineSymbols = GetSymbols(targetGroup);

            foreach (var data in defineSymbols)
            {
                foreach (var add in macro)
                {
                    if (data == add)
                    {
                        macro.Remove(add);
                        break;
                    }
                }
            }

            defineSymbols.AddRange(macro);

            SetSymbols(defineSymbols, targetGroup);
        }

        /// <summary>
        /// 移除宏
        /// </summary>
        public static void RemoveMacro(string macro, BuildTargetGroup targetGroup)
        {
            if (string.IsNullOrEmpty(macro))
            {
                return;
            }

            var defineSymbols = GetSymbols(targetGroup);

            foreach (var data in defineSymbols)
            {
                if (data == macro)
                {
                    defineSymbols.Remove(data);
                    SetSymbols(defineSymbols, targetGroup);
                    return;
                }
            }
        }

        /// <summary>
        /// 移除宏
        /// </summary>
        public static void RemoveMacro(List<string> macro, BuildTargetGroup targetGroup)
        {
            if (macro == null || macro.Count <= 0)
            {
                return;
            }

            var defineSymbols = GetSymbols(targetGroup);

            for (int i = defineSymbols.Count - 1; i >= 0; i--)
            {
                var data = defineSymbols[i];
                foreach (var remove in macro)
                {
                    if (data == remove)
                    {
                        defineSymbols.Remove(data);
                        break;
                    }
                }
            }

            SetSymbols(defineSymbols, targetGroup);
        }

        /// <summary>
        /// 泛型 根据枚举获取所有字符串
        /// </summary>
        public static List<string> GetEnumAll<T>() where T : struct
        {
            var all = new List<string>();

            foreach (var valueObj in Enum.GetValues(typeof(T)))
            {
                var value = (long)valueObj;
                if (value <= 0)
                {
                    continue;
                }

                all.Add(valueObj.ToString());
            }

            return all;
        }

        /// <summary>
        /// 获取选择相同的
        /// </summary>
        public static List<string> GetEnumSelect<T>(T selectEnum) where T : struct
        {
            var selectValue = Convert.ToInt64(selectEnum);
            ;

            var all = new List<string>();

            foreach (var valueObj in Enum.GetValues(typeof(T)))
            {
                var value = (long)valueObj;
                if (value <= 0)
                {
                    continue;
                }

                if ((value & selectValue) != 0)
                {
                    all.Add(valueObj.ToString());
                }
            }

            return all;
        }

        /// <summary>
        /// 根据现有宏 初始化枚举值
        /// </summary>
        public static long InitEnumValue<T>(BuildTargetGroup targetGroup) where T : struct
        {
            var defineSymbols = GetSymbols(targetGroup);

            long initValue = 0;

            foreach (var symbol in defineSymbols)
            {
                foreach (var valueObj in Enum.GetValues(typeof(T)))
                {
                    var value = (long)valueObj;
                    if (value <= 0)
                    {
                        continue;
                    }

                    if (symbol == valueObj.ToString())
                    {
                        initValue += value;
                    }
                }
            }

            return initValue;
        }
    }
}
#endif