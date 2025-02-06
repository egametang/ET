#if ODIN_INSPECTOR
using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    public class PackageRequestRemove
    {
        private string        m_Name;
        private RemoveRequest m_Request;
        private Action<bool>  m_RequestCallback;

        public PackageRequestRemove(string name, Action<bool> callback)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError($"null包 请传入名称");
                callback?.Invoke(false);
                return;
            }

            m_Name = name;

            if (!CheckRemove())
            {
                callback?.Invoke(false);
                return;
            }

            m_RequestCallback        =  callback;
            m_Request                =  Client.Remove(name);
            EditorApplication.update += UpdateRequest;
        }

        private bool CheckRemove()
        {
            if (!PackageHubHelper.CheckRemove(m_Name, true))
            {
                return false;
            }

            var packagePath = Application.dataPath.Replace("Assets", "Packages") + "/" + m_Name;

            try
            {
                System.IO.Directory.Delete(packagePath, true);

                if (System.IO.Directory.Exists(packagePath))
                {
                    Debug.LogError("删除失败 文件还存在");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        private void UpdateRequest()
        {
            if (!m_Request.IsCompleted) return;

            if (m_Request.Status == StatusCode.Success)
            {
                m_RequestCallback?.Invoke(true);
            }
            else
            {
                Debug.LogError($"移除 请求失败:{m_Name} 请刷新后重试!\n{m_Request.Error.message}");
                m_RequestCallback?.Invoke(false);
            }

            EditorApplication.update -= UpdateRequest;
            m_RequestCallback        =  null;
        }
    }
}
#endif