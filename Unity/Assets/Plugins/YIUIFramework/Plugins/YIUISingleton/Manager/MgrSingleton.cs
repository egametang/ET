using ET;
using UnityEngine;

namespace YIUIFramework
{
    //与Singleton 相比 mgr的单例可以受mgrcenter管理 可实现IManagerUpdate等操作
    public abstract class MgrSingleton<T> : Singleton<T>, IManagerAsyncInit where T : MgrSingleton<T>, new()
    {
        private bool m_Enabled;

        public bool Enabled => m_Enabled;

        private bool m_InitedSucceed;

        public bool InitedSucceed => m_InitedSucceed;

        public async ETTask<bool> ManagerAsyncInit()
        {
            if (m_InitedSucceed)
            {
                Debug.LogError($"{typeof(T).Name}已成功初始化过 请勿重复初始化");
                return true;
            }

            var result = await MgrAsyncInit();
            if (!result)
            {
                Debug.LogError($"{typeof(T).Name} 初始化失败");
            }
            else
            {
                //成功初始化才记录
                m_InitedSucceed = true;
            }

            return result;
        }

        public void SetEnabled(bool value)
        {
            m_Enabled = value;
        }

        protected sealed override void OnInitSingleton()
        {
            //密封初始化方法 必须使用异步
        }

        protected virtual async ETTask<bool> MgrAsyncInit()
        {
            await ETTask.CompletedTask;
            return true;
        }
    }
}