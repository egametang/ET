using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YIUIFramework;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    /// <summary>
    /// 改变绑定数据 配合 event使用
    /// 由event触发 然后吧对应的值修改
    /// </summary>
    [Serializable]
    [DetailedInfoBox("改变数据",
        @"根据提前关联的数据 触发时会将关联的数据修改成提前预设的数据
内置一个改变响应事件 可以注册这个事件做回调处理
Event 那边已经做好一个自动关联 可以直接关联到事件回调
因为有接口 IPointerClickHandler 所以任何可以被射线检测到的都可以点击
不一定需要Selectable组件 他不是必须的")]
    [LabelText("改变数据")]
    [AddComponentMenu("YIUIBind/Data/★改变数据 【Change】 UIDataBindSelectBase")]
    public class UIDataBindChange : UIDataBindSelectBase, IPointerClickHandler
    {
        [OdinSerialize]
        [ShowInInspector]
        [LabelText("所有需要改变的数据")]
        [ListDrawerSettings(IsReadOnly = true)]
        #if UNITY_EDITOR
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        #endif
        private List<UIDataChangeRef> m_Datas = new List<UIDataChangeRef>();

        [OdinSerialize]
        [ShowInInspector]
        [LabelText("响应点击")]
        #if UNITY_EDITOR
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        #endif
        private bool m_InvokeClick = true;

        [SerializeField]
        [LabelText("拖拽时不响应点击")]
        [ShowIf("m_InvokeClick")]
        #if UNITY_EDITOR
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        #endif
        private bool m_SkipWhenDrag;

        [SerializeField]
        [ReadOnly]
        [LabelText("可选组件")]
        [ShowIf("m_InvokeClick")]
        private Selectable m_Selectable;

        private event Action OnChangeDataValueAction;

        public void AddChangeAction(Action changeAction)
        {
            OnChangeDataValueAction -= changeAction;
            OnChangeDataValueAction += changeAction;
        }

        public void RemoveChangeAction(Action changeAction)
        {
            OnChangeDataValueAction -= changeAction;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!m_InvokeClick) return;

            if (m_Selectable != null && !m_Selectable.interactable)
            {
                return;
            }

            if (m_SkipWhenDrag && eventData.dragging)
            {
                return;
            }

            ChangeDataValue();
        }

        protected override int Mask()
        {
            return -1;
        }

        [GUIColor(0, 1, 1)]
        [Button("响应点击", 30)]
        [ShowIf("m_InvokeClick")]
        [PropertyOrder(-100)]
        public void ChangeDataValue()
        {
            if (m_Datas == null || m_Datas.Count <= 0)
            {
                return;
            }

            foreach (var changeData in m_Datas)
            {
                changeData.Data.SetValueFrom(changeData.ChangeData);
                changeData.Refresh(changeData.Data);
            }

            try
            {
                OnChangeDataValueAction?.Invoke();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        protected override void OnValueChanged()
        {
        }

        protected override void OnRefreshData()
        {
            base.OnRefreshData();

            //循环比较
            //没有的就删除
            //缺少的就添加
            //且同步修改后的名字
            m_Selectable ??= GetComponent<Selectable>();

            foreach (var target in DataSelectDic.Values)
            {
                var exist = false;

                //我的
                foreach (var self in m_Datas)
                {
                    if (target.Data.DataGuid == self.Data.DataGuid)
                    {
                        //存在则刷新
                        self.Refresh(target.Data);
                        exist = true;
                        break;
                    }
                }

                if (exist) continue;

                //说明不存在
                m_Datas.Add(new UIDataChangeRef(target.Data));
            }

            //需要移除 所以从后往前
            for (var i = m_Datas.Count - 1; i >= 0; i--)
            {
                var self  = m_Datas[i];
                var exist = false;
                foreach (var target in DataSelectDic.Values)
                {
                    if (target.Data.DataGuid == self.Data.DataGuid)
                    {
                        exist = true;
                        break;
                    }
                }

                if (exist) continue;

                //说明不一样
                m_Datas.RemoveAt(i);
            }
        }

        #if UNITY_EDITOR

        [SerializeField]
        [HideInInspector]
        private UIEventBindChangeDataValue m_UIEventBindChangeDataValue;

        [ShowIf("ShowAddEventBtn")]
        [Button("添加事件", 30)]
        #if UNITY_EDITOR
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        #endif
        private void AddEventChange()
        {
            m_UIEventBindChangeDataValue = gameObject.GetOrAddComponent<UIEventBindChangeDataValue>();
        }

        private bool ShowAddEventBtn()
        {
            if (!m_InvokeClick) return false;
            return m_UIEventBindChangeDataValue == null;
        }

        #endif
    }
}