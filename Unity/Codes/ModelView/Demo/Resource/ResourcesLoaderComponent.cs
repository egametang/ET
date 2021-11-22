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
                    list.List.AddRange(self.LoadedResource);
                    self.LoadedResource = null;

                    // 延迟5秒卸载包，因为包卸载是引用技术，5秒之内假如重新有逻辑加载了这个包，那么可以避免一次卸载跟加载
                    await TimerComponent.Instance.WaitAsync(5000);

                    foreach (string abName in list.List)
                    {
                        using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.ResourcesLoader, abName.GetHashCode(), 0))
                        {
                            if (ResourcesComponent.Instance == null)
                            {
                                return;
                            }

                            await ResourcesComponent.Instance.UnloadBundleAsync(abName);
                        }
                    }
                }
            }
            
            
            UnLoadAsync().Coroutine();
        }
    }

    public class ResourcesLoaderComponent: Entity
    {
        public HashSet<string> LoadedResource = new HashSet<string>();

        public async ETTask LoadAsync(string ab)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.ResourcesLoader, ab.GetHashCode(), 0))
            {
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
        }
    }
}