using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    public class PackageRequestAddAndRemove
    {
        private string[]                                             m_packagesToAdd;
        private AddAndRemoveRequest                                  m_Request;
        private Action<List<UnityEditor.PackageManager.PackageInfo>> m_RequestCallback;

        public PackageRequestAddAndRemove(List<string> addList, Action<List<UnityEditor.PackageManager.PackageInfo>> callback)
        {
            if (addList == null || addList.Count <= 0)
            {
                Debug.LogError($"addList 数据错误 请检查");
                callback?.Invoke(null);
                return;
            }

            m_packagesToAdd          =  addList.ToArray();
            m_RequestCallback        =  callback;
            m_Request                =  Client.AddAndRemove(m_packagesToAdd);
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
                    var infos       = new List<UnityEditor.PackageManager.PackageInfo>();
                    foreach (var info in packageInfo)
                    {
                        infos.Add(info);
                    }

                    m_RequestCallback?.Invoke(infos);
                }
                else
                {
                    m_RequestCallback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"添加 请求失败:{m_packagesToAdd} 请刷新后重试!\n{m_Request.Error.message}");
                m_RequestCallback?.Invoke(null);
            }

            EditorApplication.update -= UpdateRequest;
            m_RequestCallback        =  null;
        }
    }
}