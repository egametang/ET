using System.Collections.Generic;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 部类 界面拆分数据
    /// </summary>
    public partial class YIUIPanelComponent
    {
        private UIPanelSplitData m_PanelSplitData;

        private Dictionary<string, Entity> m_ExistView = new();

        private Dictionary<string, RectTransform> m_ViewParent = new();

        #region 下面公开的方法之所以放到component就是 如果你不确定你不应该使用

        /// <summary>
        /// 当前已打开的UI View 不包含弹窗
        /// </summary>
        internal Entity u_CurrentOpenView;

        public Entity CurrentOpenView => u_CurrentOpenView;

        /// <summary>
        /// 外界可判断最后一次打开的view名字
        /// </summary>
        public string CurrentOpenViewName => u_CurrentOpenView?.GetParent<YIUIComponent>().UIResName ?? "";

        /// <summary>
        /// 由于view是可以自己关闭自己的 所以当前的UI有可能会自己关闭自己 并不是用的通用打开其他被关闭
        /// 所以这里可以判断到他是否被关闭了
        /// </summary>
        public bool CurrentOpenViewActiveSelf => u_CurrentOpenView?.GetParent<YIUIComponent>().ActiveSelf ?? false;

        #endregion

        internal void InitPanelViewData()
        {
            m_ExistView.Clear();
            m_ViewParent.Clear();
            m_PanelSplitData = this.UIBase.CDETable.PanelSplitData;
            CreateCommonView();
            AddViewParent(m_PanelSplitData.AllCommonView);
            AddViewParent(m_PanelSplitData.AllCreateView);
            AddViewParent(m_PanelSplitData.AllPopupView);
        }

        private void AddViewParent(List<RectTransform> listParent)
        {
            foreach (var parent in listParent)
            {
                var viewName = parent.name.Replace(UIStaticHelper.UIParentName, "");
                m_ViewParent.Add(viewName, parent);
            }
        }

        private void CreateCommonView()
        {
            foreach (var commonParentView in m_PanelSplitData.AllCommonView)
            {
                var viewName = commonParentView.name.Replace(UIStaticHelper.UIParentName, "");

                //通用view的名称是不允许修改的 如果修改了 那么就创建一个新的
                var viewTsf = commonParentView.FindChildByName(viewName);
                if (viewTsf == null)
                {
                    Debug.LogError($"{viewName} 当前通用View 不存在于父级下 所以无法自动创建 将会动态创建");
                    continue;
                }

                var data = YIUIBindHelper.GetBindVoByResName(viewName);
                if (data == null) return;
                var vo = data.Value;
                if (vo.CodeType != EUICodeType.View)
                {
                    Log.Error($"打开错误必须是一个view类型");
                    return;
                }

                //查看本地是否已经创建
                var view = this.UIBase.CDETable.FindUIOwner<Entity>(viewName);

                //如果没有则通用重新创建
                view ??= YIUIFactory.CreateByObjVo(vo, viewTsf.gameObject, this.UIBase.OwnerUIEntity);

                if (view != null)
                {
                    m_ExistView.Add(viewName, view);
                }
            }
        }

        private RectTransform GetViewParent(string viewName)
        {
            m_ViewParent.TryGetValue(viewName, out var value);
            return value;
        }

        internal async ETTask<Entity> GetView(string resName)
        {
            if (this.UIBase.OwnerUIEntity == null)
            {
                Log.Error($"没有找到ET UI组件");
                return null;
            }

            var data = YIUIBindHelper.GetBindVoByResName(resName);
            if (data == null) return null;
            var vo = data.Value;
            if (vo.CodeType != EUICodeType.View)
            {
                Log.Error($"打开错误必须是一个view类型");
                return null;
            }

            var viewParent = GetViewParent(resName);
            if (viewParent == null)
            {
                Debug.LogError($"不存在这个View  请检查 {resName}");
                return null;
            }

            if (this.m_ExistView.TryGetValue(resName, out var baseView))
            {
                return baseView;
            }

            if (ViewIsOpening(resName))
            {
                Debug.LogError($"请检查 {resName} 正在异步打开中 请勿重复调用 请检查代码是否一瞬间频繁调用");
                return null;
            }

            AddOpening(resName);
            var view = await YIUIFactory.InstantiateAsync(vo, this.UIBase.OwnerUIEntity, viewParent);
            RemovOpening(resName);
            m_ExistView.Add(resName, view);
            return view;
        }

        internal async ETTask<Entity> GetView<T>() where T : Entity
        {
            if (this.UIBase.OwnerUIEntity == null)
            {
                Log.Error($"没有找到ET UI组件");
                return null;
            }

            var data = YIUIBindHelper.GetBindVoByType<T>();
            if (data == null) return null;
            var vo = data.Value;
            if (vo.CodeType != EUICodeType.View)
            {
                Log.Error($"打开错误必须是一个view类型");
                return null;
            }

            var viewName   = vo.ResName;
            var viewParent = GetViewParent(viewName);
            if (viewParent == null)
            {
                Debug.LogError($"不存在这个View  请检查 {viewName}");
                return null;
            }

            if (this.m_ExistView.TryGetValue(viewName, out var baseView))
            {
                return baseView;
            }

            if (ViewIsOpening(viewName))
            {
                Debug.LogError($"请检查 {viewName} 正在异步打开中 请勿重复调用 请检查代码是否一瞬间频繁调用");
                return null;
            }

            AddOpening(viewName);
            var view = await YIUIFactory.InstantiateAsync(vo, this.UIBase.OwnerUIEntity, viewParent);
            RemovOpening(viewName);
            m_ExistView.Add(viewName, view);
            return view;
        }

        internal (bool, Entity) ExistView<T>() where T : Entity
        {
            if (this.UIBase.OwnerUIEntity == null)
            {
                Log.Error($"没有找到ET UI组件");
                return (false, null);
            }

            var data = YIUIBindHelper.GetBindVoByType<T>();
            if (data == null) return (false, null);
            var vo = data.Value;
            if (vo.CodeType != EUICodeType.View)
            {
                Log.Error($"打开错误必须是一个view类型");
                return (false, null);
            }

            var viewName   = vo.ResName;
            var viewParent = GetViewParent(viewName);
            if (viewParent == null)
            {
                Debug.LogError($"不存在这个View  请检查 {viewName}");
                return (false, null);
            }

            if (this.m_ExistView.TryGetValue(viewName, out var baseView))
            {
                return (true, baseView);
            }

            return (false, null);
        }

        internal (bool, Entity) ExistView(string resName)
        {
            if (this.UIBase.OwnerUIEntity == null)
            {
                Log.Error($"没有找到ET UI组件");
                return (false, null);
            }

            var data = YIUIBindHelper.GetBindVoByResName(resName);
            if (data == null) return (false, null);
            var vo = data.Value;
            if (vo.CodeType != EUICodeType.View)
            {
                Log.Error($"打开错误必须是一个view类型");
                return (false, null);
            }

            var viewParent = GetViewParent(resName);
            if (viewParent == null)
            {
                Debug.LogError($"不存在这个View  请检查 {resName}");
                return (false, null);
            }

            if (this.m_ExistView.TryGetValue(resName, out var baseView))
            {
                return (true, baseView);
            }

            return (false, null);
        }

        /// <summary>
        /// 打开之前
        /// </summary>
        internal async ETTask OpenViewBefore(Entity view)
        {
            if (!view.GetParent<YIUIComponent>().GetComponent<YIUIWindowComponent>().WindowFitstOpen)
            {
                await CloseLastView(view);
            }
        }

        /// <summary>
        /// 打开之后
        /// </summary>
        internal async ETTask OpenViewAfter(Entity view, bool success)
        {
            if (success)
            {
                if (view.GetParent<YIUIComponent>().GetComponent<YIUIWindowComponent>().WindowFitstOpen)
                {
                    await CloseLastView(view);
                }
            }
            else
            {
                view.GetParent<YIUIComponent>().GetComponent<YIUIViewComponent>().Close(false);
            }
        }

        /// <summary>
        /// 关闭上一个
        /// </summary>
        /// <param name="view">当前</param>
        private async ETTask CloseLastView(Entity view)
        {
            //其他需要被忽略 Panel下的view 如果是窗口类型 那么他只能同时存在一个  弹窗层可以存在多个
            if (view.GetParent<YIUIComponent>().GetComponent<YIUIViewComponent>().ViewWindowType != EViewWindowType.View)
            {
                return;
            }

            //View只有切换没有关闭
            var skipTween = view.GetParent<YIUIComponent>().GetComponent<YIUIWindowComponent>().WindowSkipOtherCloseTween;

            if (u_CurrentOpenView != null && u_CurrentOpenView != view && CurrentOpenViewActiveSelf)
            {
                var uibase = u_CurrentOpenView.GetParent<YIUIComponent>();

                //View 没有自动回退功能  比如AView 关闭 自动吧上一个BView 给打开 没有这种需求 也不能有这个需求
                //只能有 打开一个新View 上一个View的自动处理 99% 都是吧上一个隐藏即可
                //外部就只需要关心 打开 A B C 即可
                //因为这是View  不是 Panel
                switch (uibase.GetComponent<YIUIViewComponent>().StackOption)
                {
                    case EViewStackOption.None:
                        break;
                    case EViewStackOption.Visible:
                        uibase.SetActive(false);
                        break;
                    case EViewStackOption.VisibleTween:
                        await u_CurrentOpenView.GetParent<YIUIComponent>().GetComponent<YIUIViewComponent>().CloseAsync(!skipTween);
                        break;
                    default:
                        Debug.LogError($"新增类型未实现 {uibase.GetComponent<YIUIViewComponent>().StackOption}");
                        uibase.SetActive(false);
                        break;
                }
            }

            u_CurrentOpenView = view;
        }
    }
}