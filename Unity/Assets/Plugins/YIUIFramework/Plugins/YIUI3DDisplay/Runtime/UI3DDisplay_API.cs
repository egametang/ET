using System;
using UnityEngine;

namespace YIUIFramework
{
    public sealed partial class UI3DDisplay
    {
        //当前显示的层级  默认>>YIUI3DLayer 最好不要改
        public string ShowLayerName => m_ShowLayerName;

        //当前显示的摄像机
        public Camera ShowCamera => m_ShowCamera;

        //添加回收 回调 如果自己有所有对象的引用管理 可以自行管理 也可以使用回调管理
        //如果没有回调 上一个物体不做处理 不会被看到因为他层级会被修改到默认
        public void AddRecycleLastAction(Action<GameObject> action)
        {
            m_RecycleLastAction += action;
        }

        //移除回收 回调
        public void RemoveRecycleLastAction(Action<GameObject> action)
        {
            m_RecycleLastAction -= action;
        }

        /// <summary>
        /// 显示3D
        /// </summary>
        /// <param name="showObject">需要被显示的对象</param>
        /// <param name="lookCamera">摄像机参数</param>
        internal void Show(GameObject showObject, Camera lookCamera)
        {
            if (showObject == null)
            {
                Debug.LogError($"必须设置显示对象");
                return;
            }

            if (lookCamera == null)
            {
                Debug.LogError($"必须设置参考摄像机");
                return;
            }

            SetTemporaryRenderTexture();

            UpdateShowObject(showObject);

            UpdateLookCamera(lookCamera);
        }

        //清除显示的对象
        public void ClearShow()
        {
            if (m_ShowCameraCtrl != null)
            {
                m_ShowCameraCtrl.ShowObject = null;
            }

            DisableMeshRectShadow();
            RecycleLastShow(m_ShowObject);
            m_ShowObject = null;
        }

        //重置旋转
        public void ResetRotation()
        {
            m_DragRotation = 0.0f;
            if (m_ShowObject == null) return;

            var showTsf      = m_ShowObject.transform;
            var showRotation = Quaternion.Euler(m_ShowRotation);
            var showUp       = showRotation * Vector3.up;
            showRotation     *= Quaternion.AngleAxis(m_DragRotation, showUp);
            showTsf.rotation =  showRotation;
        }

        //设置旋转
        public void SetRotation(Vector3 rotation)
        {
            m_ShowRotation = rotation;
            if (m_ShowObject == null) return;

            var showTsf      = m_ShowObject.transform;
            var showRotation = Quaternion.Euler(m_ShowRotation);
            var showUp       = showRotation * Vector3.up;
            showRotation     *= Quaternion.AngleAxis(m_DragRotation, showUp);
            showTsf.rotation =  showRotation;
        }

        //设置位置偏移
        public void SetOffset(Vector3 offset)
        {
            m_ShowOffset = offset;
            if (m_ShowObject == null) return;

            var showTsf = m_ShowObject.transform;
            showTsf.localPosition = m_ModelGlobalOffset + m_ShowOffset;
            m_ShowPosition        = showTsf.localPosition;
        }

        //设置大小
        public void SetScale(Vector3 scale)
        {
            m_ShowScale = scale;
            if (m_ShowObject == null) return;

            var showTsf = m_ShowObject.transform;
            showTsf.localScale = m_ShowScale;
        }

        //改变 当前的分辨率 一般情况下 都不会改变
        public void ChangeResolution(Vector2 newResolution)
        {
            if (!(Math.Abs(newResolution.x - m_ResolutionX) > 0.01f) &&
                !(Math.Abs(newResolution.y - m_ResolutionY) > 0.01f)) return;

            m_ResolutionX = (int)Math.Round(newResolution.x);
            m_ResolutionY = (int)Math.Round(newResolution.y);
            SetTemporaryRenderTexture();
        }
    }
}