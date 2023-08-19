using System.Collections.Generic;

namespace ET.Client
{
    [EntitySystemOf(typeof(ResourcesLoaderComponent))]
    [FriendOf(typeof(ResourcesLoaderComponent))]
    public static partial class ResourcesLoaderComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ResourcesLoaderComponent self)
        {
        }
        
        [EntitySystem]
        private static void Destroy(this ResourcesLoaderComponent self)
        {
            Fiber fiber = self.Fiber();
            ReleaseAssetsAsync().Coroutine();
            return;

            async ETTask ReleaseAssetsAsync()
            {
                using ListComponent<string> list = ListComponent<string>.Create();

                TimerComponent timerComponent = fiber.TimerComponent;
                list.AddRange(self.resources);
                self.resources = null;

                if (timerComponent == null)
                {
                    return;
                }

                // 延迟5秒卸载包，因为包卸载是引用计数，5秒之内假如重新有逻辑加载了这个包，那么可以避免一次卸载跟加载
                await timerComponent.WaitAsync(5000);

                foreach (string assetsName in list)
                {
                    using CoroutineLock coroutineLock = await fiber.CoroutineLockComponent.Wait(CoroutineLockType.ResourcesLoader, assetsName.GetHashCode());
                    ResourcesComponent.Instance.ReleaseAssets(assetsName);
                }
            }
        }

        public static async ETTask<UnityEngine.Object> LoadAssetsAsync(this ResourcesLoaderComponent self, string assetsName)
        {
            UnityEngine.Object o;
            if (ResourcesComponent.Instance.TryGetAssets(assetsName, out o))
            {
                return o;
            }
            using CoroutineLock coroutineLock = await self.Fiber().CoroutineLockComponent.Wait(CoroutineLockType.ResourcesLoader, assetsName.GetHashCode());
            if (ResourcesComponent.Instance.TryGetAssets(assetsName, out o))
            {
                return o;
            }
            o = await ResourcesComponent.Instance.LoadAssetAsync(assetsName);
            self.resources.Add(assetsName);
            return o;
        }
    }

    [ComponentOf]
    public class ResourcesLoaderComponent: Entity, IAwake, IDestroy
    {
        public HashSet<string> resources = new();
    }
}