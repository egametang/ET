using DotRecast.Core;

namespace DotRecast.Recast
{
    /// Represents a heightfield layer within a layer set.
    /// @see rcHeightfieldLayerSet
    public class RcHeightfieldLayer
    {
        public RcVec3f bmin = new RcVec3f();

        /// < The minimum bounds in world space. [(x, y, z)]
        public RcVec3f bmax = new RcVec3f();

        /// < The maximum bounds in world space. [(x, y, z)]
        public float cs;

        /// < The size of each cell. (On the xz-plane.)
        public float ch;

        /// < The height of each cell. (The minimum increment along the y-axis.)
        public int width;

        /// < The width of the heightfield. (Along the x-axis in cell units.)
        public int height;

        /// < The height of the heightfield. (Along the z-axis in cell units.)
        public int minx;

        /// < The minimum x-bounds of usable data.
        public int maxx;

        /// < The maximum x-bounds of usable data.
        public int miny;

        /// < The minimum y-bounds of usable data. (Along the z-axis.)
        public int maxy;

        /// < The maximum y-bounds of usable data. (Along the z-axis.)
        public int hmin;

        /// < The minimum height bounds of usable data. (Along the y-axis.)
        public int hmax;

        /// < The maximum height bounds of usable data. (Along the y-axis.)
        public int[] heights;

        /// < The heightfield. [Size: width * height]
        public int[] areas;

        /// < Area ids. [Size: Same as #heights]
        public int[] cons; /// < Packed neighbor connection information. [Size: Same as #heights]
    }
}