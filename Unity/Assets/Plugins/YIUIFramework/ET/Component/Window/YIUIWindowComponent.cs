//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// UI窗口组件
    /// </summary>
    [ComponentOf(typeof (YIUIComponent))]
    public partial class YIUIWindowComponent: Entity, IAwake, IDestroy
    {
        private YIUIComponent _UiBase;

        private ETTask _LastETTask;
        
        public YIUIComponent UIBase
        {
            get
            {
                return this._UiBase ??= this.GetParent<YIUIComponent>();
            }
        }

        private Entity _OwnerUIEntity;

        public Entity OwnerUIEntity
        {
            get
            {
                return this._OwnerUIEntity ??= UIBase?.OwnerUIEntity;
            }
        }

        /// <summary>
        /// 窗口选项
        /// 实现各种配置
        /// 如非必要 所有属性判断可以直接去HasFlag分类中获取 不要直接判断这个
        /// </summary>
        public EWindowOption WindowOption = EWindowOption.None;

        private bool m_FirstOpenTween;

        internal async ETTask InternalOnWindowOpenTween(bool tween = true)
        {
            if (tween && (!WindowBanRepetitionOpenTween || !m_FirstOpenTween))
            {
                m_FirstOpenTween = true;

                if (WindowBanAwaitOpenTween)
                {
                    SealedOnWindowOpenTween().Coroutine();
                }
                else
                {
                    await SealedOnWindowOpenTween();
                }
            }
            else
            {
                OnOpenTweenEnd();
            }
        }

        private bool m_FirstCloseTween;

        internal async ETTask InternalOnWindowCloseTween(bool tween = true)
        {
            if (tween && (!WindowBanRepetitionCloseTween || !m_FirstCloseTween))
            {
                m_FirstCloseTween = true;

                if (WindowBanAwaitCloseTween)
                {
                    SealedOnWindowCloseTween().Coroutine();
                }
                else
                {
                    await SealedOnWindowCloseTween();
                }
            }
            else
            {
                OnCloseTweenEnd();
            }
        }

        //有可能没有动画 也有可能动画被跳过 反正无论如何都会有动画结束回调
        private void OnOpenTweenEnd()
        {
            YIUIEventSystem.OpenTweenEnd(this.UIBase.OwnerUIEntity);
        }

        private void OnCloseTweenEnd()
        {
            YIUIEventSystem.CloseTweenEnd(this.UIBase.OwnerUIEntity);
        }

        private async ETTask SealedOnWindowOpenTween()
        {
            if (YIUIMgrComponent.IsLowQuality || WindowBanTween)
            {
                OnOpenTweenEnd();
                return;
            }

            var foreverCode = WindowAllowOptionByTween? 0 : UIBase.m_UIMgr.BanLayerOptionForever();
            try
            {
                await OnOpenTween();
            }
            catch (Exception e)
            {
                Debug.LogError($"{UIBase.UIResName} 打开动画执行报错 {e}");
            }
            finally
            {
                UIBase.m_UIMgr.RecoverLayerOptionForever(foreverCode);
                OnOpenTweenEnd();
            }
        }

        private async ETTask SealedOnWindowCloseTween()
        {
            if (!UIBase.ActiveSelf || YIUIMgrComponent.IsLowQuality || WindowBanTween)
            {
                OnCloseTweenEnd();
                return;
            }

            var foreverCode = WindowAllowOptionByTween? 0 : UIBase.m_UIMgr.BanLayerOptionForever();
            try
            {
                await OnCloseTween();
            }
            catch (Exception e)
            {
                Debug.LogError($"{UIBase.UIResName} 关闭动画执行报错 {e}");
            }
            finally
            {
                UIBase.m_UIMgr.RecoverLayerOptionForever(foreverCode);
                OnCloseTweenEnd();
            }
        }

        private async ETTask OnOpenTween()
        {
            if (_LastETTask != null)
            {
                await this._LastETTask;
            }
            
            _LastETTask = ETTask.Create(true);
            var tweent = await YIUIEventSystem.OpenTween(this.UIBase.OwnerUIEntity);
            if (!tweent)
            {
                //panel会有默认动画
                //不要动画请在界面参数上调整 WindowBanTween
                //需要其他动画请实现动画事件
                if (this.UIBase.UIBindVo.CodeType == EUICodeType.Panel)
                    await WindowFadeAnim.In(this.UIBase);
            }
            _LastETTask.SetResult();
            _LastETTask = null;
        }

        private async ETTask OnCloseTween()
        {
            if (_LastETTask != null)
            {
                await this._LastETTask;
            }
            
            _LastETTask = ETTask.Create(true);
            var tweent = await YIUIEventSystem.CloseTween(this.UIBase.OwnerUIEntity);
            if (!tweent)
            {
                if (this.UIBase.UIBindVo.CodeType == EUICodeType.Panel)
                    await WindowFadeAnim.Out(this.UIBase);
            }
            _LastETTask.SetResult();
            _LastETTask = null;
        }
    }
}