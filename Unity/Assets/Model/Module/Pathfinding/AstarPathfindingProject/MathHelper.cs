using UnityEngine;

namespace Pathfinding
{
    public static class MathHelper
    {
        public static Vector3 ToUnityV3(this Vector3 v3)
        {
            return new Vector3(v3.x, v3.y, v3.z);
        }
        
        public static Vector3 ToPFV3(this Vector3 v3)
        {
            return new Vector3(v3.x, v3.y, v3.z);
        }
        
        public static Vector2 ToV2(this Vector3 v3)
        {
            return new Vector2(v3.x, v3.y);
        }
        
        public static Vector2 ToUnityV2(this Vector2 v2)
        {
            return new Vector3(v2.x, v2.y);
        }
        
        public static Vector2 ToPFV2(this Vector2 v2)
        {
            return new Vector2(v2.x, v2.y);
        }
        

    }
}