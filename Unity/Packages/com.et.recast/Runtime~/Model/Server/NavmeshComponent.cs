using System;
using System.Collections.Generic;

namespace ET
{
    public class NavmeshComponent: Singleton<NavmeshComponent>, ISingletonAwake
    {
        public struct RecastFileLoader
        {
            public string Name { get; set; }
        }

        private readonly Dictionary<string, byte[]> navmeshs = new();
        
        public void Awake()
        {
        }
        
        public byte[] Get(string name)
        {
            lock (this)
            {
                if (this.navmeshs.TryGetValue(name, out byte[] bytes))
                {
                    return bytes;
                }

                byte[] buffer =
                        EventSystem.Instance.Invoke<NavmeshComponent.RecastFileLoader, byte[]>(
                            new NavmeshComponent.RecastFileLoader() { Name = name });
                if (buffer.Length == 0)
                {
                    throw new Exception($"no nav data: {name}");
                }

                this.navmeshs[name] = buffer;
                return buffer;
            }
        }
    }
}