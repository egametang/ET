using System.Collections.Generic;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 3DDisplay的扩展
    /// 自带创建与对象池 基础需求使用更简单
    /// </summary>
    public partial class YIUI3DDisplayExtend
    {
        private UI3DDisplay m_UI3DDisplay;

        private Dictionary<string, GameObject> m_ObjPool = new Dictionary<string, GameObject>();

        private Dictionary<GameObject, Dictionary<string, Camera>> m_CameraPool =
                new Dictionary<GameObject, Dictionary<string, Camera>>();

        private YIUI3DDisplayExtend()
        {
        }

        public YIUI3DDisplayExtend(UI3DDisplay ui3DDisplay)
        {
            m_UI3DDisplay = ui3DDisplay;
        }

        public GameObject Show(string resName, string cameraName = "")
        {
            if (m_UI3DDisplay == null)
            {
                Debug.LogError($"没有3D显示组件");
                return null;
            }

            var obj = GetDisplayObject(resName);
            if (obj == null) return null;
            var camera = string.IsNullOrEmpty(cameraName)? m_UI3DDisplay.ShowCamera : GetCamera(obj, cameraName);
            if (camera == null) return obj;
            m_UI3DDisplay.Show(obj, camera);
            return obj;
        }

        private GameObject GetDisplayObject(string resName)
        {
            if (!m_ObjPool.ContainsKey(resName))
            {
                var newObj = CreateObject(resName);
                m_ObjPool.Add(resName, newObj);
            }

            return m_ObjPool[resName];
        }

        private GameObject CreateObject(string resName)
        {
            var obj = YIUIFactory.InstantiateGameObject("", resName);
            if (obj == null)
            {
                Debug.LogError($"实例化失败 {resName}  请检查为何没有加载成功 是否配置");
            }

            return obj;
        }

        private Camera GetCamera(GameObject obj, string cameraName)
        {
            if (!m_CameraPool.ContainsKey(obj))
            {
                m_CameraPool.Add(obj, new Dictionary<string, Camera>());
            }

            var objDic = m_CameraPool[obj];

            if (!objDic.ContainsKey(cameraName))
            {
                var camera = GetCameraByName(obj, cameraName);
                objDic.Add(cameraName, camera);
            }

            return objDic[cameraName];
        }

        private Camera GetCameraByName(GameObject obj, string cameraName)
        {
            var cameraTsf = obj.transform.FindChildByName(cameraName);
            if (cameraTsf == null)
            {
                Debug.LogError($"{obj.name} 没有找到目标摄像机 {cameraName} 请检查");
                return null;
            }

            var camera = cameraTsf.GetComponent<Camera>();
            if (camera == null)
            {
                Debug.LogError($"{obj.name} 没有找到目标摄像机组件 {cameraName} 请检查");
                return null;
            }

            return camera;
        }
    }
}