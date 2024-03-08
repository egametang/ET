using Unity.Mathematics;

namespace ET.Client
{
    [FriendOf(typeof(XunLuoPathComponent))]
    public static partial class XunLuoPathComponentSystem
    {
        public static float3 GetCurrent(this XunLuoPathComponent self)
        {
            return self.path[self.Index];
        }
        
        public static void MoveNext(this XunLuoPathComponent self)
        {
            self.Index = ++self.Index % self.path.Length;
        }
    }
}