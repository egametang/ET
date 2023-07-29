using System.Collections.Generic;

namespace ET.Client
{
    [EntitySystemOf(typeof(ResourcesLoaderComponent))]
    [FriendOf(typeof(ResourcesLoaderComponent))]
    public static partial class ResourcesLoaderComponentSystem
    {
        [EntitySystem]
        private static void Destroy(this ResourcesLoaderComponent self)
        {
            async ETTask UnLoadAsync()
            {
                Fiber fiber = self.Fiber();
                using ListComponent<string> list = ListComponent<string>.Create();

                TimerComponent timerComponent = fiber.TimerComponent;
                list.AddRange(self.LoadedResource);
                self.LoadedResource = null;

                if (timerComponent == null)
                {
                    return;
                }

                // 延迟5秒卸载包，因为包卸载是引用计数，5秒之内假如重新有逻辑加载了这个包，那么可以避免一次卸载跟加载
                await timerComponent.WaitAsync(5000);

                CoroutineLockComponent coroutineLockComponent = fiber.CoroutineLockComponent;
                foreach (string abName in list)
                {
                    using CoroutineLock coroutineLock =
                            await coroutineLockComponent.Wait(CoroutineLockType.ResourcesLoader, abName.GetHashCode(), 0);
                    {
                        if (fiber.IsDisposed)
                        {
                            return;
                        }

                        await fiber.Root.GetComponent<ResourcesComponent>().UnloadBundleAsync(abName);
                    }
                }
            }

            UnLoadAsync().Coroutine();
        }

        public static async ETTask LoadAsync(this ResourcesLoaderComponent self, string ab)
        {
            using CoroutineLock coroutineLock = await self.Fiber().CoroutineLockComponent.Wait(CoroutineLockType.ResourcesLoader, ab.GetHashCode(), 0);

            if (self.IsDisposed)
            {
                Log.Error($"resourceload already disposed {ab}");
                return;
            }

            if (self.LoadedResource.Contains(ab))
            {
                return;
            }

            self.LoadedResource.Add(ab);
            await self.Root().GetComponent<ResourcesComponent>().LoadBundleAsync(ab);
        }
        
        [EntitySystem]
        private static void Awake(this ResourcesLoaderComponent self)
        {

        }
    }

    [ComponentOf]
    public class ResourcesLoaderComponent: Entity, IAwake, IDestroy
    {
        public HashSet<string> LoadedResource = new HashSet<string>();
    }
}