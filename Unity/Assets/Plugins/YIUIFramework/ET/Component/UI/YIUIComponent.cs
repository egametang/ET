//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using Sirenix.OdinInspector;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// UI主体
    /// </summary>
    [ChildOf(typeof (YIUIComponent))]
    public partial class YIUIComponent: Entity, IAwake<YIUIBindVo, GameObject>, IDestroy
    {
        /// <summary>
        /// UI的资源包名
        /// </summary>
        public string UIPkgName => m_UIBindVo.PkgName;

        /// <summary>
        /// UI的资源名称
        /// </summary>
        public string UIResName { get; private set; }

        public UIBindCDETable       CDETable       { get; private set; }
        public UIBindComponentTable ComponentTable { get; private set; }
        public UIBindDataTable      DataTable      { get; private set; }
        public UIBindEventTable     EventTable     { get; private set; }

        /// <summary>
        /// 当前UI的预设对象
        /// </summary>
        [LabelText("UI对象")]
        public GameObject OwnerGameObject { get; private set; }

        /// <summary>
        /// 当前UI的Tsf
        /// </summary>
        [HideInInspector]
        public RectTransform OwnerRectTransform { get; private set; }

        /// <summary>
        /// 初始化状态
        /// </summary>
        private bool m_UIBaseInit;

        public bool UIBaseInit => m_UIBaseInit;

        //用这个不用. 不用一长串的获取
        public YIUIMgrComponent m_UIMgr;

        //被实例化后的UIEntity对象
        //我的XXComponent 不是当前的这个UIComponent
        public Entity OwnerUIEntity { get; private set; }

        /// <summary>
        /// UI名称 用于开关UI = componentName
        /// </summary>
        public string UIName => this.m_UIBindVo.ComponentType.Name;

        /// <summary>
        /// 绑定信息
        /// </summary>
        private YIUIBindVo m_UIBindVo;

        public YIUIBindVo UIBindVo => m_UIBindVo;

        /// <summary>
        /// 当前显示状态  显示/隐藏
        /// 不要使用这个设置显影
        /// 应该使用控制器 或调用方法 SetActive();
        /// </summary>
        public bool ActiveSelf
        {
            get
            {
                if (OwnerGameObject == null) return false;
                return OwnerGameObject.activeSelf;
            }
        }

        //设置当前拥有的这个实际UI 之后初始化
        internal void InitOwnerUIEntity(Entity uiEntity)
        {
            this.OwnerUIEntity = uiEntity;
            this.UIInitialize();
        }

        /// <summary>
        /// 初始化UIBase 由PanelMgr创建对象后调用
        /// 外部禁止
        /// </summary>
        internal void InitUIBase(YIUIBindVo uiBindVo, GameObject ownerGameObject)
        {
            if (ownerGameObject == null)
            {
                Debug.LogError($"null对象无法初始化");
                return;
            }

            OwnerGameObject    = ownerGameObject;
            OwnerRectTransform = OwnerGameObject.GetComponent<RectTransform>();
            CDETable           = OwnerGameObject.GetComponent<UIBindCDETable>();
            if (CDETable == null)
            {
                Debug.LogError($"{OwnerGameObject.name} 没有UIBindCDETable组件 这是必须的");
                return;
            }

            ComponentTable           = CDETable.ComponentTable;
            DataTable                = CDETable.DataTable;
            EventTable               = CDETable.EventTable;
            m_UIBindVo               = uiBindVo;
            UIResName                = uiBindVo.ResName;
            m_UIBaseInit             = true;
            m_UIMgr                  = YIUIMgrComponent.Inst;
            CDETable.UIBaseStart     = UIBaseStart;
            CDETable.UIBaseOnDestroy = UIBaseOnDestroy;
            AddUIDataComponent();
        }

        //根据UI类型添加其他组件
        private void AddUIDataComponent()
        {
            switch (this.m_UIBindVo.CodeType)
            {
                case EUICodeType.Panel:
                    AddComponent<YIUIWindowComponent>();
                    AddComponent<YIUIPanelComponent>();
                    break;
                case EUICodeType.View:
                    AddComponent<YIUIWindowComponent>();
                    AddComponent<YIUIViewComponent>();
                    break;
                case EUICodeType.Common:
                    break;
                default:
                    Debug.LogError($"没有这个类型 {this.m_UIBindVo.CodeType}");
                    break;
            }
        }

        private void UIDataComponentInitialize()
        {
            try
            {
                switch (this.m_UIBindVo.CodeType)
                {
                    case EUICodeType.Panel:
                        YIUIEventSystem.Initialize(this.GetComponent<YIUIWindowComponent>());
                        YIUIEventSystem.Initialize(this.GetComponent<YIUIPanelComponent>());
                        break;
                    case EUICodeType.View:
                        YIUIEventSystem.Initialize(this.GetComponent<YIUIWindowComponent>());
                        YIUIEventSystem.Initialize(this.GetComponent<YIUIViewComponent>());
                        break;
                    case EUICodeType.Common:
                        break;
                    default:
                        Debug.LogError($"没有这个类型 {this.m_UIBindVo.CodeType}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        #region 公共方法

        /// <summary>
        /// 设置显隐
        /// </summary>
        public void SetActive(bool value)
        {
            if (OwnerGameObject == null) return;
            OwnerGameObject.SetActive(value);
        }

        //其他的关于 RectTransform 相关的 不建议包一层
        //就直接 OwnerRectTransform. 使用Unity API 就可以了 没必要包一成
        //这么多方法 都有可能用到你都包一层嘛

        #endregion

        #region 生命周期

        private void UIBaseStart()
        {
            CDETable.UIBaseOnEnable  = UIBaseOnEnable;
            CDETable.UIBaseOnDisable = UIBaseOnDisable;
        }

        private void UIInitialize()
        {
            UIDataComponentInitialize();
            try
            {
                YIUIEventSystem.Bind(this.OwnerUIEntity);
                YIUIEventSystem.Initialize(this.OwnerUIEntity);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void UIBaseOnEnable()
        {
            try
            {
                YIUIEventSystem.Enable(this.OwnerUIEntity);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        private void UIBaseOnDisable()
        {
            try
            {
                YIUIEventSystem.Disable(this.OwnerUIEntity);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        //UI对象被移除时
        private void UIBaseOnDestroy()
        {
            if (!this.IsDisposed)
                this.Parent.RemoveChild(this.Id);
            YIUIFactory.Destroy(this);
        }

        #endregion
    }
}