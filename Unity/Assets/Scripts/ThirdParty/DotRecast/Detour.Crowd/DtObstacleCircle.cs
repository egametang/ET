using DotRecast.Core;

namespace DotRecast.Detour.Crowd
{
    /// < Max number of adaptive rings.
    public class DtObstacleCircle
    {
        /** Position of the obstacle */
        public RcVec3f p = new RcVec3f();

        /** Velocity of the obstacle */
        public RcVec3f vel = new RcVec3f();

        /** Velocity of the obstacle */
        public RcVec3f dvel = new RcVec3f();

        /** Radius of the obstacle */
        public float rad;

        /** Use for side selection during sampling. */
        public RcVec3f dp = new RcVec3f();

        /** Use for side selection during sampling. */
        public RcVec3f np = new RcVec3f();
    }
}