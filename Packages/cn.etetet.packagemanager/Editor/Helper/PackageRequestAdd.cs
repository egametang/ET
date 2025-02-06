using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    public class PackageRequestAdd
    {
        private string                                         m_Name;
        private AddRequest                                     m_Request;
        private Action<UnityEditor.PackageManager.PackageInfo> m_RequestCallback;

        public PackageRequestAdd(string name, Action<UnityEditor.PackageManager.PackageInfo> callback)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError($"null包 请传入名称");
                callback?.Invoke(null);
                return;
            }

            m_Name                   =  name;
            m_RequestCallback        =  callback;
            m_Request                =  Client.Add(name);
            EditorApplication.update += UpdateRequest;
        }

        private void UpdateRequest()
        {
            if (!m_Request.IsCompleted) return;

            if (m_Request.Status == StatusCode.Success)
            {
                if (m_Request.Result != null)
                {
                    var packageInfo = m_Request.Result;
                    m_RequestCallback?.Invoke(packageInfo);
                }
                else
                {
                    m_RequestCallback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"添加 请求失败:{m_Name} 请刷新后重试!\n{m_Request.Error.message}");
                m_RequestCallback?.Invoke(null);
            }

            EditorApplication.update -= UpdateRequest;
            m_RequestCallback        =  null;
        }
    }
}
