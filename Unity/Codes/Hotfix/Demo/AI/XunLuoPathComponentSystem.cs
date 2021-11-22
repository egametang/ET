using UnityEngine;

namespace ET
{
    public static class XunLuoPathComponentSystem
    {
        public static Vector3 GetCurrent(this XunLuoPathComponent self)
        {
            return self.path[self.Index];
        }
        
        public static void MoveNext(this XunLuoPathComponent self)
        {
            self.Index = ++self.Index % self.path.Length;
        }
    }
}