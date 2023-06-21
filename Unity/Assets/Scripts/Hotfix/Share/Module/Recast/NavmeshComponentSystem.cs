using System;

namespace ET
{
    [FriendOf(typeof(NavmeshComponent))]
    public static partial class NavmeshComponentSystem
    {
        public static long Get(this NavmeshComponent self, string name)
        {
            long ptr;
            if (self.Navmeshs.TryGetValue(name, out ptr))
            {
                return ptr;
            }

            byte[] buffer = EventSystem.Instance.Invoke<NavmeshComponent.RecastFileLoader, byte[]>(new NavmeshComponent.RecastFileLoader() {Name = name});
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