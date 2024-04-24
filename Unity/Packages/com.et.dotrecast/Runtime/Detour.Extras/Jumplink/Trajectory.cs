using System;
using DotRecast.Core;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class Trajectory
    {
        public float Lerp(float f, float g, float u)
        {
            return u * g + (1f - u) * f;
        }

        public virtual RcVec3f Apply(RcVec3f start, RcVec3f end, float u)
        {
            throw new NotImplementedException();
        }
    }
}