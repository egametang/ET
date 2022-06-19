using System;

namespace ET
{
    [FriendOf(typeof(NavmeshComponent))]
    public static class NavmeshComponentSystem
    {
        public class AwakeSystem: AwakeSystem<NavmeshComponent>
        {
            public override void Awake(NavmeshComponent self)
            {
                NavmeshComponent.Instance = self;
            }
        }
        
        public static long Get(this NavmeshComponent self, string name)
        {
            long ptr;
            if (self.Navmeshs.TryGetValue(name, out ptr))
            {
                return ptr;
            }

            byte[] buffer = Game.EventSystem.Callback<string, byte[]>(CallbackType.RecastFileLoader, name);
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