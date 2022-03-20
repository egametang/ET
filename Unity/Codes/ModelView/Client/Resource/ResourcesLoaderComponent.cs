using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class ResourcesLoaderComponentDestroySystem: DestroySystem<ResourcesLoaderComponent>
    {
        public override void Destroy(ResourcesLoaderComponent self)
        {
            async ETTask UnLoadAsync()
            {
                using (ListComponent<string> list = ListComponent<string>.Create())
                {
                    list.AddRange(self.LoadedResource);
                    self.LoadedResource = null;

                    if (TimerComponent.Instance == null)
                    {
                        return;
                    }
                    
                    // 延迟5秒卸载包，因为包卸载是引用计数，5秒之内假如重新有逻辑加载了这个包，那么可以避免一次卸载跟加载
                    await TimerComponent.Instance.WaitAsync(5000);

                    foreach (string abName in list)
                    {
                        CoroutineLock coroutineLock = null;
                        try
                        {
                            coroutineLock =
                                    await CoroutineLockComponent.Instance.Wait(CoroutineLockType.ResourcesLoader, abName.GetHashCode(), 0);
                            {
                                if (ResourcesComponent.Instance == null)
                                {
                                    return;
                                }

                                await ResourcesComponent.Instance.UnloadBundleAsync(abName);
                            }
                        }
                        finally
                        {
                            coroutineLock?.Dispose();
                        }
                    }
                }
            }

            UnLoadAsync().Coroutine();
        }
    }
    
    public class ResourcesLoaderComponent: Entity, IAwake, IDestroy
    {
        public HashSet<string> LoadedResource = new HashSet<string>();

        public async ETTask LoadAsync(string ab)
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.ResourcesLoader, ab.GetHashCode(), 0);
                if (this.IsDisposed)
                {
                    Log.Error($"resourceload already disposed {ab}");
                    return;
                }

                if (this.LoadedResource.Contains(ab))
                {
                    return;
                }

                LoadedResource.Add(ab);
                await ResourcesComponent.Instance.LoadBundleAsync(ab);
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }
    }
}