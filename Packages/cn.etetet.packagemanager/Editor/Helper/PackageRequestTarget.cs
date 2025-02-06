using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    public class PackageRequestTarget
    {
        private string                                         m_Name;
        private SearchRequest                                  m_TargetRequest;
        private Action<UnityEditor.PackageManager.PackageInfo> m_RequestTargetCallback;

        public PackageRequestTarget(string name, Action<UnityEditor.PackageManager.PackageInfo> callback)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError($"不可查询 null包 请传入名称");
                callback?.Invoke(null);
                return;
            }

            m_Name                   =  name;
            m_RequestTargetCallback  =  callback;
            m_TargetRequest          =  Client.Search(name);
            EditorApplication.update += CheckUpdateTargetProgress;
        }

        private void CheckUpdateTargetProgress()
        {
            if (!m_TargetRequest.IsCompleted) return;

            if (m_TargetRequest.Status == StatusCode.Success)
            {
                if (m_TargetRequest.Result is { Length: >= 1 })
                {
                    var packageInfo = m_TargetRequest.Result[0];
                    m_RequestTargetCallback?.Invoke(packageInfo);
                }
                else
                {
                    Debug.LogError($"请求失败:{m_Name} Result >= 1");
                    m_RequestTargetCallback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"请求失败:{m_Name} 如果确定此包不可用可禁用就不会请求了!! \n{m_TargetRequest.Error.message}");
                m_RequestTargetCallback?.Invoke(null);
            }

            EditorApplication.update -= CheckUpdateTargetProgress;
            m_RequestTargetCallback  =  null;
        }
    }
}