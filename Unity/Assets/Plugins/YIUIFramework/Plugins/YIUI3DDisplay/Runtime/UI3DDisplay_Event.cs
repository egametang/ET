using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System;
using Sirenix.Serialization;
using UnityEngine.Serialization;

namespace YIUIFramework
{
    public sealed partial class UI3DDisplay
    {
        public bool CanDrag
        {
            get => m_CanDrag;
            set => m_CanDrag = value;
        }

        [MinValue(0)]
        [LabelText("点击时允许偏移值")]
        [OdinSerialize]
        [ShowInInspector]
        private Vector2 m_OnClickOffset = new Vector2(50, 50);

        private Vector2                        m_OnClickDownPos = Vector2.zero;
        private RaycastHit                     m_ClickRaycastHit;
        private RaycastHit                     m_DragRaycastHit;
        private Action<GameObject, GameObject> m_OnClickEvent; //参数1 被点击的对象 参数2 他的最终父级是谁(显示对象)

        //添加一个回调
        public void AddClickEvent(Action<GameObject, GameObject> action)
        {
            m_OnClickEvent += action;
        }

        //移除一个回调
        public void RemoveClickEvent(Action<GameObject, GameObject> action)
        {
            m_OnClickEvent -= action;
        }

        //从屏幕坐标发送射线检测
        public bool Raycast(Vector2 screenPoint, out RaycastHit hitInfo)
        {
            var rect = transform as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, UICamera,
                    out var localScreenPoint) == false)
            {
                hitInfo = default;
                return false;
            }

            localScreenPoint -= rect.rect.min;
            var ray = m_ShowCamera.ScreenPointToRay(localScreenPoint);
            return Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, 1 << m_ShowLayer);
        }

        //可拖拽目标
        private GameObject m_DragTarge;

        //拖拽
        public void OnDrag(PointerEventData eventData)
        {
            if (!m_CanDrag) return;

            if (!m_DragTarge || !(m_DragSpeed > 0.0f)) return;

            var delta    = eventData.delta.x;
            var deltaRot = -m_DragSpeed * delta * Time.deltaTime;
            var dragTsf  = m_DragTarge.transform;

            if (m_MultipleTargetMode)
            {
                dragTsf.Rotate(Vector3.up * deltaRot, Space.World);
            }
            else
            {
                m_DragRotation += deltaRot;
                var showRotation = Quaternion.Euler(m_ShowRotation);
                var showUp       = showRotation * Vector3.up;
                showRotation     *= Quaternion.AngleAxis(m_DragRotation, showUp);
                dragTsf.rotation =  showRotation;
            }
        }

        //按下
        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_MultipleTargetMode)
            {
                if (!Raycast(eventData.position, out m_DragRaycastHit))
                    return;

                m_DragTarge = GetMultipleTargetByClick(m_DragRaycastHit.collider.gameObject);
            }

            if (m_OnClickEvent != null)
                m_OnClickDownPos = eventData.position;
        }

        //抬起
        public void OnPointerUp(PointerEventData eventData)
        {
            if (m_OnClickEvent == null)
                return;

            if (!ClickSucceed(eventData.position))
                return;

            if (!Raycast(eventData.position, out m_ClickRaycastHit))
                return;

            var clickObj       = m_ClickRaycastHit.collider.gameObject;
            var clickObjParent = m_MultipleTargetMode? GetMultipleTargetByClick(clickObj) : m_ShowObject;

            try
            {
                m_OnClickEvent?.Invoke(clickObj, clickObjParent);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        //范围检测 因为手机上会有偏移
        private bool ClickSucceed(Vector2 upPos)
        {
            var offset = upPos - m_OnClickDownPos;
            return Math.Abs(offset.x) <= m_OnClickOffset.x && Math.Abs(offset.y) <= m_OnClickOffset.y;
        }
    }
}