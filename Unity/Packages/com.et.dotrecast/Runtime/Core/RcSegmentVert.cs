namespace DotRecast.Core
{
    public struct RcSegmentVert
    {
        public RcVec3f vmin;
        public RcVec3f vmax;

        public RcSegmentVert(float v0, float v1, float v2, float v3, float v4, float v5)
        {
            vmin.x = v0;
            vmin.y = v1;
            vmin.z = v2;
            
            vmax.x = v3;
            vmax.y = v4;
            vmax.z = v5;
        }

    }
}