using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ResourcesComponent: Singleton<ResourcesComponent>, ISingletonAwake
    {
        private readonly Dictionary<string, UnityEngine.Object> resources = new();
        
        public void Awake()
        {
        }

        protected override void Destroy()
        {
            foreach (var kv in this.resources)
            {
                Addressables.Release(kv.Value);
            }
        }

        public bool TryGetAssets(string assetsName, out UnityEngine.Object o)
        {
            return this.resources.TryGetValue(assetsName, out o);
        }

        public UnityEngine.Object GetAssets(string assetsName)
        {
            this.resources.TryGetValue(assetsName, out UnityEngine.Object o);
            return o;
        }
        
        public async ETTask<UnityEngine.Object> LoadAssetAsync(string assetsName)
        {
            UnityEngine.Object o = await Addressables.LoadAssetAsync<UnityEngine.Object>(assetsName).Task;
            this.resources.Add(assetsName, o);
            return o;
        }
        
        public void ReleaseAssets(string assetsName)
        {
            if (!this.resources.TryGetValue(assetsName, out UnityEngine.Object o))
            {
                return;
            }
            Addressables.Release(o);
        }
    }
}