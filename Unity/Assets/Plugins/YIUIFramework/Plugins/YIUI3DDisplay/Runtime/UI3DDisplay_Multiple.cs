using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace YIUIFramework
{
    /// <summary>
    /// 多个
    /// </summary>
    public sealed partial class UI3DDisplay
    {
        [SerializeField]
        [LabelText("多目标模式")]
        private bool m_MultipleTargetMode = false;

        //可动态设置改变多目标 但是尽量不要所以关闭了
        public bool MultipleTargetMode
        {
            get => m_MultipleTargetMode;
            private set
            {
                m_MultipleTargetMode = value;
                if (value)
                {
                    InitRotationData();
                }
                else
                {
                    ClearMultipleData();
                }
            }
        }

        private List<GameObject> m_AllMultipleTarget;

        private Dictionary<GameObject, GameObject> m_MultipleCache;

        private bool m_InitMultipleData = false;

        //初始化
        private void InitRotationData()
        {
            if (m_InitMultipleData) return;

            m_AllMultipleTarget = new List<GameObject>();
            m_MultipleCache     = new Dictionary<GameObject, GameObject>();
            m_InitMultipleData  = true;
        }

        //清除
        private void ClearMultipleData()
        {
            m_AllMultipleTarget = null;
            m_MultipleCache     = null;
            m_InitMultipleData  = false;
            m_DragTarge         = m_ShowObject;
        }

        //添加目标
        public void AddMultipleTarget(GameObject obj, Camera lookCamera = null, Transform parent = null)
        {
            if (m_ShowObject == null)
            {
                Debug.LogError($"多目标 使用前 需要一个父级对象");
                return;
            }

            if (!m_InitMultipleData)
            {
                Debug.LogError("多目标模式 未初始化");
                return;
            }

            SetPosAndRotAndParent(obj.transform, lookCamera, parent);

            SetAllAnimatorCullingMode(obj.transform);

            SetupShowLayerTarget(obj.transform);

            m_AllMultipleTarget.Add(obj);
        }

        //移除目标
        public void RemoveMultipleTarget(GameObject obj)
        {
            if (!m_InitMultipleData)
            {
                Debug.LogError("多目标模式 未初始化");
                return;
            }

            m_AllMultipleTarget.Remove(obj);
        }

        //获取点击目标的父级对象
        private GameObject GetMultipleTargetByClick(GameObject child)
        {
            var obj = GetMultipleCache(child);
            if (obj != null)
                return obj;

            if (IsMultipleTarget(child))
                return child;

            var parent = child.transform.parent;
            if (parent)
                return GetMultipleTargetByClick(parent.gameObject);

            Debug.LogError($"没有找到这个对象 {child.name}");
            return null;
        }

        private GameObject GetMultipleCache(GameObject obj)
        {
            m_MultipleCache.TryGetValue(obj, out var rotationObj);
            return rotationObj;
        }

        private bool IsMultipleTarget(GameObject obj)
        {
            foreach (var item in m_AllMultipleTarget)
            {
                if (item == obj)
                {
                    m_MultipleCache.Add(obj, item);
                    return true;
                }
            }

            return false;
        }

        //多目标下 设置位置旋转父级
        private static void SetPosAndRotAndParent(Transform target, Camera lookCamera = null, Transform parent = null)
        {
            if (parent)
                target.SetParent(parent, false);

            target.localPosition = Vector3.zero;

            target.localScale = Vector3.one;

            target.localRotation = lookCamera? lookCamera.transform.localRotation : Quaternion.identity;
        }
    }
}