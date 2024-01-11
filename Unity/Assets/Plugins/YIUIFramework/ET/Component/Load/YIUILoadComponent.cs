using System;
using System.Collections.Generic;
using YIUIFramework;
using YooAsset;

namespace ET.Client
{
    /// <summary>
    /// UI通用加载组件
    /// </summary>
    [ComponentOf(typeof (YIUIMgrComponent))]
    public partial class YIUILoadComponent: Entity, IAwake, IAwake<string>, IDestroy
    {
        private Dictionary<int, AssetOperationHandle> m_AllHandle = new();

        private ResourcePackage package;

        internal void Destroy()
        {
            ReleaseAllAction();
        }

        internal void Awake(string packageName = "DefaultPackage")
        {
            package = YooAssets.GetPackage(packageName);

            //关联UI工具中自动生成绑定代码 Tools >> YIUI自动化工具 >> 发布 >> UI自动生成绑定替代反射代码
            //在ET中这个自动生成的代码在ModelView中所以在此框架中无法初始化赋值
            //将由HotfixView AddComponent<YIUIMgrComponent> 之前调用一次
            //会在 InitAllBind 方法中被调用
            //YIUIBindHelper.InternalGameGetUIBindVoFunc = YIUICodeGenerated.YIUIBindProvider.Get;
            
            //YIUI会用到的各种加载 需要自行实现 Demo中使用的是YooAsset 根据自己项目的资源管理器实现下面的方法
            YIUILoadDI.LoadAssetFunc           = LoadAsset;               //同步加载
            YIUILoadDI.LoadAssetAsyncFunc      = LoadAssetAsync;          //异步加载
            YIUILoadDI.ReleaseAction           = ReleaseAction;           //释放
            YIUILoadDI.VerifyAssetValidityFunc = VerifyAssetValidityFunc; //检查
            YIUILoadDI.ReleaseAllAction        = ReleaseAllAction;        //释放所有
        }

        /// <summary>
        /// 释放方法
        /// </summary>
        /// <param name="hashCode">加载时所给到的唯一ID</param>
        private void ReleaseAction(int hashCode)
        {
            if (!this.m_AllHandle.TryGetValue(hashCode, out var value))
            {
                return;
            }

            value.Release();
            this.m_AllHandle.Remove(hashCode);
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="arg1">包名</param>
        /// <param name="arg2">资源名</param>
        /// <param name="arg3">类型</param>
        /// <returns>返回值(obj资源对象,唯一ID)</returns>
        private async ETTask<(UnityEngine.Object, int)> LoadAssetAsync(string arg1, string arg2, Type arg3)
        {
            var handle = package.LoadAssetAsync(arg2, arg3);
            await handle.Task;
            return LoadAssetHandle(handle);
        }

        /// <summary>
        /// 同步加载
        /// </summary>
        /// <param name="arg1">包名</param>
        /// <param name="arg2">资源名</param>
        /// <param name="arg3">类型</param>
        /// <returns>返回值(obj资源对象,唯一ID)</returns>
        private (UnityEngine.Object, int) LoadAsset(string arg1, string arg2, Type arg3)
        {
            var handle = package.LoadAssetSync(arg2, arg3);
            return LoadAssetHandle(handle);
        }

        //Demo中对YooAsset加载后的一个简单返回封装
        //只有成功加载才返回 否则直接释放
        private (UnityEngine.Object, int) LoadAssetHandle(AssetOperationHandle handle)
        {
            if (handle.AssetObject != null)
            {
                var hashCode = handle.GetHashCode();
                m_AllHandle.Add(hashCode, handle);
                return (handle.AssetObject, hashCode);
            }
            else
            {
                handle.Release();
                return (null, 0);
            }
        }

        //释放所有
        private void ReleaseAllAction()
        {
            foreach (var handle in m_AllHandle.Values)
            {
                handle.Release();
            }

            m_AllHandle.Clear();
        }

        //检查合法
        private bool VerifyAssetValidityFunc(string arg1, string arg2)
        {
            return package.CheckLocationValid(arg2);
        }
    }
}