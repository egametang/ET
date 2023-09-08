using System.Collections.Generic;

namespace DotRecast.Detour
{
    public class BVItemYComparer : IComparer<BVItem>
    {
        public static readonly BVItemYComparer Shared = new BVItemYComparer();

        private BVItemYComparer()
        {
        }

        public int Compare(BVItem a, BVItem b)
        {
            return a.bmin[1].CompareTo(b.bmin[1]);
        }
    }
}