//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System.Collections.Generic;
using ET;

namespace YIUIFramework
{
    //管理所有继承IManager的管理器
    public partial class MgrCenter : MonoSingleton<MgrCenter>
    {
        private MgrCore g_MgrCore = new MgrCore();

        //失败的初始化队列 //可根据需求对失败的进行二次处理
        private Queue<IManager> m_FailInited = new Queue<IManager>();

        //异步注册需要管理的 管理器 如果有异步初始化方法则会调用
        //返回初始化结果 如果失败是无法加入到管理中的
        //失败的可以重复调用直到成功
        //只有注册没有对应的移除  需要移除直接Dispose释放即可
        public async ETTask<bool> Register(IManager manager)
        {
            var result = await g_MgrCore.Add(manager);
            if (!result)
            {
                m_FailInited.Enqueue(manager);
            }
            return result;
        }

        //获取失败的管理器
        //队列出队 可能没有
        public IManager GetFailInitedMgr()
        {
            return m_FailInited.Dequeue();
        }

        //获取当前失败数量
        public int GetFailInitedCount()
        {
            return m_FailInited.Count;
        }

        //获取当前剩余的所有
        public List<IManager> GetFailInitedMgrList()
        {
            var list  = new List<IManager>();
            var count = GetFailInitedCount();
            for (int i = 0; i < count; i++)
            {
                list.Add(GetFailInitedMgr());
            }

            return list;
        }

        private void Update()
        {
            g_MgrCore?.Update();
        }

        private void LateUpdate()
        {
            g_MgrCore?.LateUpdate();
        }

        private void FixedUpdate()
        {
            g_MgrCore?.FixedUpdate();
        }

        protected override void OnDispose()
        {
            g_MgrCore?.Dispose();
            g_MgrCore = null;
        }
    }
}