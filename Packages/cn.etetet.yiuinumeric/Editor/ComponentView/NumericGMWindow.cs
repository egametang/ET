using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public partial class NumericGMWindow : EditorWindow
    {
        private class NumericEditorData
        {
            public int    NumericType;
            public string NumericTypeString;
            public long   Value;
            public string Name;
            public int    FinalType;
            public int    ValueType;
            public int    DefinitionType;
            public string ShowValue;
        }

        private          object                  m_Data;
        private readonly Dictionary<int, long>   m_DataDic     = new();
        private readonly List<NumericEditorData> m_ShowData    = new();
        private          List<NumericEditorData> m_SearchData  = new();
        private          int                     m_CurrentPage = 1;
        private readonly int                     m_PerPage     = 20;
        private          int                     m_AddType     = 0;
        private          long                    m_AddValue    = 0;
        private          string                  m_Search      = "";

        public static void SwitchWindow(object data)
        {
            if (HasOpenInstances<NumericGMWindow>())
            {
                CloseWindow();
            }
            else
            {
                OpenWindow(data);
            }
        }

        public static void OpenWindow(object data)
        {
            var window = GetWindow<NumericGMWindow>(false, "数值GM调试");
            if (window != null)
            {
                window.m_Data = data;
                window.Show();
                window.Focus();
            }
        }

        public static void CloseWindow()
        {
            GetWindow<NumericGMWindow>()?.Close();
        }

        private void UpdateSyncData()
        {
            if (m_Data is not NumericData numericData) return;
            numericData.GetNumericDic(m_DataDic);
            m_ShowData.Clear();

            foreach ((int k, long v) in m_DataDic)
            {
                var kType = (ENumericType)k;

                m_ShowData.Add(new()
                {
                    NumericType       = k,
                    NumericTypeString = k.ToString(),
                    Value             = v,
                    Name              = kType.GetLocalizationName(),
                    ValueType         = (int)kType.GetNumericValueType(),
                    DefinitionType    = (int)kType.GetNumericDefinitionType(),
                    ShowValue         = numericData.GetLocalizationValue(kType),
                    FinalType         = (int)kType.GetNumericFinalEnum(),
                });
            }

            m_ShowData.Sort(SortData);
            m_SearchData = m_ShowData;
        }

        private int SortData(NumericEditorData x, NumericEditorData y)
        {
            var final = x.FinalType.CompareTo(y.FinalType);
            if (final == 0)
            {
                return x.DefinitionType.CompareTo(y.DefinitionType);
            }

            return final;
        }

        private void SearchUpdate(string newSearch)
        {
            if (string.IsNullOrEmpty(newSearch))
            {
                m_SearchData = m_ShowData;
                return;
            }

            m_SearchData = new();
            foreach (var data in m_ShowData)
            {
                if (Regex.IsMatch(data.NumericTypeString, newSearch) || Regex.IsMatch(data.Name, newSearch))
                {
                    m_SearchData.Add(data);
                }
            }
        }

        private async ETTask PushGMChange(ETTask task)
        {
            await task;
            UpdateSyncData();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                CloseWindow();
                return;
            }

            if (m_Data is not NumericData numericData) return;

            if (GUILayout.Button("刷新数据", GUILayout.Height(50)))
            {
                UpdateSyncData();
            }

            if (m_SearchData.Count <= 0)
            {
                UpdateSyncData();
            }

            //手动添加数值
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"动态添加数值ID");
                EditorGUILayout.LabelField($"值 (存储值)");
                EditorGUILayout.LabelField($"", GUILayout.Width(100));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                m_AddType  = EditorGUILayout.IntField("", m_AddType);
                m_AddValue = EditorGUILayout.LongField("", m_AddValue);
                if (GUILayout.Button("修改", GUILayout.Width(100)))
                {
                    var ownerEntity = numericData.OwnerEntity;
                    if (ownerEntity == null)
                    {
                        Debug.LogError($"目标数值数据 没有拥有者 Entity");
                        return;
                    }

                    PushGMChange(EventSystem.Instance.PublishAsync(ownerEntity.Scene(), new NumericGMChange
                    {
                        OwnerEntity = ownerEntity,
                        NumericType = m_AddType,
                        New         = m_AddValue
                    })).NoContext();
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            //搜索
            {
                GUILayout.BeginHorizontal();
                m_Search = EditorGUILayout.TextField("", m_Search);
                if (GUILayout.Button("搜索", GUILayout.Width(100)))
                {
                    SearchUpdate(m_Search);
                    GUILayout.EndHorizontal();
                    return;
                }
                else
                {
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);
            }

            var startIndex = (m_CurrentPage - 1) * m_PerPage;
            var endIndex   = Mathf.Min(startIndex + m_PerPage, m_SearchData.Count);

            //分页
            {
                GUILayout.BeginHorizontal();
                if (m_SearchData.Count == m_ShowData.Count)
                {
                    GUILayout.Label($"[总数:{m_SearchData.Count}]", GUILayout.Width(200));
                }
                else
                {
                    GUILayout.Label($"[筛选:{m_SearchData.Count}] [总数:{m_ShowData.Count}]", GUILayout.Width(200));
                }

                GUILayout.Label($"[页面 {m_CurrentPage} of {(m_SearchData.Count + m_PerPage - 1) / m_PerPage}]");

                if (GUILayout.Button("上一页", GUILayout.Width(50)))
                {
                    m_CurrentPage = Mathf.Max(1, m_CurrentPage - 1);
                }

                if (GUILayout.Button("下一页", GUILayout.Width(50)))
                {
                    m_CurrentPage = Mathf.Min((m_SearchData.Count + m_PerPage - 1) / m_PerPage, m_CurrentPage + 1);
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            //显示数据
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"名称", GUILayout.Width(200));
            EditorGUILayout.LabelField($"ID", GUILayout.Width(100));
            EditorGUILayout.LabelField($"类型", GUILayout.Width(100));
            EditorGUILayout.LabelField($"显示值", GUILayout.Width(150));
            EditorGUILayout.LabelField($"存储值");
            GUILayout.EndHorizontal();

            for (int i = startIndex; i < endIndex; i++)
            {
                var editorData     = m_SearchData[i];
                var k              = editorData.NumericType;
                var v              = editorData.Value;
                var keyName        = editorData.Name;
                var valueType      = editorData.ValueType;
                var definitionType = editorData.DefinitionType;
                var showValue      = editorData.ShowValue;

                GUILayout.BeginHorizontal();

                var isResult = definitionType == 0;
                var notGrow  = k.IsNotGrowNumeric();
                GUI.color = isResult ? notGrow ? Color.cyan : Color.green : Color.white;
                EditorGUILayout.LabelField($"[{keyName}]", GUILayout.Width(200));
                GUI.color = Color.white;
                EditorGUILayout.LabelField($"[{k}]", GUILayout.Width(100));
                EditorGUILayout.LabelField($"[{(ENumericValueType)valueType}]", GUILayout.Width(100));
                EditorGUILayout.LabelField($"[{showValue}]", GUILayout.Width(150));

                if (isResult && !notGrow)
                {
                    EditorGUILayout.LabelField($"{v}");
                    GUILayout.EndHorizontal();
                }
                else
                {
                    var newValue = EditorGUILayout.LongField($"", v);
                    GUILayout.EndHorizontal();
                    m_DataDic.TryGetValue(k, out var oldValue);
                    var change = newValue != oldValue;
                    if (newValue != v)
                    {
                        m_SearchData[i].Value = newValue;
                    }

                    if (change)
                    {
                        if (GUILayout.Button("修改", GUILayout.Height(30)))
                        {
                            var ownerEntity = numericData.OwnerEntity;
                            if (ownerEntity == null)
                            {
                                Debug.LogError($"目标数值数据 没有拥有者 Entity");
                                return;
                            }

                            PushGMChange(EventSystem.Instance.PublishAsync(ownerEntity.Scene(), new NumericGMChange
                            {
                                OwnerEntity = ownerEntity,
                                NumericType = k,
                                Old         = oldValue,
                                New         = newValue
                            })).NoContext();
                        }

                        GUILayout.Space(10);
                    }
                }
            }
        }
    }
}