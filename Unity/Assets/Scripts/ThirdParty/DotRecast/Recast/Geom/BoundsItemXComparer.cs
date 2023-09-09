using System.Collections.Generic;

namespace DotRecast.Recast.Geom
{
    public class BoundsItemXComparer : IComparer<BoundsItem>
    {
        public static readonly BoundsItemXComparer Shared = new BoundsItemXComparer();

        private BoundsItemXComparer()
        {
        }

        public int Compare(BoundsItem a, BoundsItem b)
        {
            return a.bmin.x.CompareTo(b.bmin.x);
        }
    }
}