using System;
using ET;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    public partial class YIUIViewComponent
    {
        /// <summary>
        /// 使用基础Open 打开类
        /// </summary>
        /// <returns></returns>
        internal async ETTask<bool> UseBaseOpen()
        {
            if (!this.UIWindow.WindowCanUseBaseOpen)
            {
                Debug.LogError($"当前传入的参数不支持 并未实现这个打开方式 且不允许使用基础Open打开 请检查");
                return false;
            }

            var success = false;

            try
            {
                if (this.OwnerUIEntity is IYIUIOpen _)
                {
                    success = await YIUIEventSystem.Open(this.OwnerUIEntity);
                }
                else
                {
                    success = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"err={e.Message}{e.StackTrace}");
            }

            return success;
        }

        public async ETTask<bool> Open()
        {
            this.UIBase.SetActive(true);

            var success = false;

            if (!this.UIWindow.WindowHaveIOpenAllowOpen && this.OwnerUIEntity is IYIUIOpenParam)
            {
                Debug.LogError($"当前Panel 有其他IOpen 接口 需要参数传入 不允许直接调用Open");
                return false;
            }

            try
            {
                if (this.OwnerUIEntity is IYIUIOpen _)
                {
                    success = await YIUIEventSystem.Open(this.OwnerUIEntity);
                }
                else
                {
                    success = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"ResName{this.UIBase.UIResName}, err={e.Message}{e.StackTrace}");
            }

            if (success)
            {
                await this.UIWindow.InternalOnWindowOpenTween();
            }

            return success;
        }

        public async ETTask<bool> Open(ParamVo param)
        {
            if (this.UIWindow.WindowBanParamOpen)
            {
                Debug.LogError($"当前禁止使用ParamOpen 请检查");
                return false;
            }

            UIBase.SetActive(true);

            var success = false;

            if (this.OwnerUIEntity is IYIUIOpen<ParamVo> _)
            {
                try
                {
                    success = await YIUIEventSystem.Open(this.OwnerUIEntity, param);
                }
                catch (Exception e)
                {
                    Debug.LogError($"ResName{UIBase.UIResName}, err={e.Message}{e.StackTrace}");
                }
            }
            else
            {
                success = await UseBaseOpen();
            }

            if (success)
            {
                await this.UIWindow.InternalOnWindowOpenTween();
            }

            return success;
        }

        public async ETTask<bool> Open<P1>(P1 p1)
        {
            UIBase.SetActive(true);

            var success = false;

            if (this.OwnerUIEntity is IYIUIOpen<P1> _)
            {
                try
                {
                    success = await YIUIEventSystem.Open(this.OwnerUIEntity, p1);
                }
                catch (Exception e)
                {
                    Debug.LogError($"ResName{this.UIBase.UIResName}, err={e.Message}{e.StackTrace}");
                }
            }
            else
            {
                success = await UseBaseOpen();
            }

            if (success)
            {
                await this.UIWindow.InternalOnWindowOpenTween();
            }

            return success;
        }

        public async ETTask<bool> Open<P1, P2>(P1 p1, P2 p2)
        {
            UIBase.SetActive(true);

            var success = false;

            if (this.OwnerUIEntity is IYIUIOpen<P1, P2> _)
            {
                try
                {
                    success = await YIUIEventSystem.Open(this.OwnerUIEntity, p1, p2);
                }
                catch (Exception e)
                {
                    Debug.LogError($"ResName{this.UIBase.UIResName}, err={e.Message}{e.StackTrace}");
                }
            }
            else
            {
                success = await UseBaseOpen();
            }

            if (success)
            {
                await this.UIWindow.InternalOnWindowOpenTween();
            }

            return success;
        }

        public async ETTask<bool> Open<P1, P2, P3>(P1 p1, P2 p2, P3 p3)
        {
            UIBase.SetActive(true);

            var success = false;

            if (this.OwnerUIEntity is IYIUIOpen<P1, P2, P3> _)
            {
                try
                {
                    success = await YIUIEventSystem.Open(this.OwnerUIEntity, p1, p2, p3);
                }
                catch (Exception e)
                {
                    Debug.LogError($"ResName{this.UIBase.UIResName}, err={e.Message}{e.StackTrace}");
                }
            }
            else
            {
                success = await UseBaseOpen();
            }

            if (success)
            {
                await this.UIWindow.InternalOnWindowOpenTween();
            }

            return success;
        }

        public async ETTask<bool> Open<P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4)
        {
            UIBase.SetActive(true);

            var success = false;

            if (this.OwnerUIEntity is IYIUIOpen<P1, P2, P3, P4> _)
            {
                try
                {
                    success = await YIUIEventSystem.Open(this.OwnerUIEntity, p1, p2, p3, p4);
                }
                catch (Exception e)
                {
                    Debug.LogError($"ResName{this.UIBase.UIResName}, err={e.Message}{e.StackTrace}");
                }
            }
            else
            {
                success = await UseBaseOpen();
            }

            if (success)
            {
                await this.UIWindow.InternalOnWindowOpenTween();
            }

            return success;
        }

        public async ETTask<bool> Open<P1, P2, P3, P4, P5>(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            UIBase.SetActive(true);

            var success = false;

            if (this.OwnerUIEntity is IYIUIOpen<P1, P2, P3, P4, P5> _)
            {
                try
                {
                    success = await YIUIEventSystem.Open(this.OwnerUIEntity, p1, p2, p3, p4, p5);
                }
                catch (Exception e)
                {
                    Debug.LogError($"ResName{this.UIBase.UIResName}, err={e.Message}{e.StackTrace}");
                }
            }
            else
            {
                success = await UseBaseOpen();
            }

            if (success)
            {
                await this.UIWindow.InternalOnWindowOpenTween();
            }

            return success;
        }
    }
}