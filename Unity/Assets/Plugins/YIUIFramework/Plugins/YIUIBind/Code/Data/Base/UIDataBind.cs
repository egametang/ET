//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using Sirenix.OdinInspector;
using UnityEngine;
using YIUIFramework;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    /// <summary>
    /// 所有绑定数据基类
    /// </summary>
    [ExecuteInEditMode]
    [HideLabel]
    [HideReferenceObjectPicker]
    public abstract class UIDataBind : SerializedMonoBehaviour
    {
        [Required("必须选择")]
        [SerializeField]
        [HideLabel]
        [HideReferenceObjectPicker]
        [ReadOnly]
        [PropertyOrder(-999)]
        private UIBindDataTable m_DataTable;

        public UIBindDataTable DataTable => m_DataTable;

        private bool m_Binded;

        internal UIData FindData(string dataName)
        {
            if (string.IsNullOrEmpty(dataName))
            {
                Logger.LogWarning($"{name} 不能找一个空变量 请检查配置");
                return null;
            }

            var data = m_DataTable?.FindData(dataName);
            if (data == null)
            {
                if (m_DataTable == null)
                {
                    Logger.LogErrorContext(this, $"{name} 未设置变量表 所以无法找到变量 {dataName} 请检查配置");
                    return null;
                }

                Logger.LogErrorContext(this, $"{name} 没有找到这个变量 {dataName} 请检查配置");
                return null;
            }

            return data;
        }

        //会被上一级的UIDataTable初始化
        //也可以被自己初始化 根据顺序
        internal void Initialize(bool refresh = false)
        {
            if (!refresh && m_Binded) return;

            m_Binded = true;
            RefreshDataTable();
            UnBindData();
            BindData();
            OnRefreshData();
        }

        protected void OnDestroy()
        {
            UnBindData();
            m_Binded = false;
        }

        /// <summary>
        /// 绑定
        /// </summary>
        protected abstract void BindData();

        /// <summary>
        /// 解绑
        /// </summary>
        protected abstract void UnBindData();

        /// <summary>
        /// 刷新
        /// 所有绑定 解绑过后刷新
        /// 相当于初始化 所以之类非必要不要自己写Awake Start之类的
        /// 记得一定要调用base.OnRefreshData(); 以免多重基础后的错误问题
        /// </summary>
        protected abstract void OnRefreshData();

        private void RefreshDataTable()
        {
            if (m_DataTable == null)
            {
                m_DataTable = this.GetComponentInParentHard<UIBindDataTable>();
            }
        }

        #if UNITY_EDITOR

        /// <summary>
        /// 因为UIData被删除了
        /// 所以对应的需要处理相关关联绑定
        /// </summary>
        public abstract void RemoveBindData(UIData data);

        protected void OnValidate()
        {
            if (UIOperationHelper.IsPlaying() && m_Binded)
            {
                return;
            }

            m_Binded = true;
            RefreshDataTable();
            UnBindData();
            BindData();
            OnRefreshData();
        }
        #endif
    }
}