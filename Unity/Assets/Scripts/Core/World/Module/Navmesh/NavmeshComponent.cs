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

        private readonly Dictionary<string, long> navmeshs = new();
        
        public void Awake()
        {
        }
        
        public long Get(string name)
        {
            lock (this)
            {
                long ptr;
                if (this.navmeshs.TryGetValue(name, out ptr))
                {
                    return ptr;
                }

                byte[] buffer = EventSystem.Instance.Invoke<RecastFileLoader, byte[]>(new RecastFileLoader() {Name = name});
                if (buffer.Length == 0)
                {
                    throw new Exception($"no nav data: {name}");
                }

                ptr = Recast.RecastLoadLong(name.GetHashCode(), buffer, buffer.Length);
                this.navmeshs[name] = ptr;

                return ptr;
            }
        }
    }
}