using System;

namespace ET
{
    [FriendClass(typeof(NavmeshComponent))]
    public static class NavmeshComponentSystem
    {
        public class AwakeSystem: AwakeSystem<NavmeshComponent, Func<string, byte[]>>
        {
            public override void Awake(NavmeshComponent self, Func<string, byte[]> loader)
            {
                NavmeshComponent.Instance = self;
                self.Loader = loader;
            }
        }
        
        public static long Get(this NavmeshComponent self, string name)
        {
            long ptr;
            if (self.Navmeshs.TryGetValue(name, out ptr))
            {
                return ptr;
            }

            byte[] buffer = self.Loader(name);
            if (buffer.Length == 0)
            {
                throw new Exception($"no nav data: {name}");
            }

            ptr = Recast.RecastLoadLong(name.GetHashCode(), buffer, buffer.Length);
            self.Navmeshs[name] = ptr;

            return ptr;
        }
    }
}