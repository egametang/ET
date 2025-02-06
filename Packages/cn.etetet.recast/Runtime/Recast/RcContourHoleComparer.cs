using System.Collections.Generic;

namespace DotRecast.Recast
{
    public class RcContourHoleComparer : IComparer<RcContourHole>
    {
        public static readonly RcContourHoleComparer Shared = new RcContourHoleComparer();

        private RcContourHoleComparer()
        {
        }

        public int Compare(RcContourHole a, RcContourHole b)
        {
            if (a.minx == b.minx)
            {
                return a.minz.CompareTo(b.minz);
            }
            else
            {
                return a.minx.CompareTo(b.minx);
            }
        }
    }
}