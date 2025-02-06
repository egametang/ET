using System.Collections.Generic;

namespace DotRecast.Recast
{
    public class RcPotentialDiagonalComparer : IComparer<RcPotentialDiagonal>
    {
        public static readonly RcPotentialDiagonalComparer Shared = new RcPotentialDiagonalComparer();

        private RcPotentialDiagonalComparer()
        {
        }

        public int Compare(RcPotentialDiagonal va, RcPotentialDiagonal vb)
        {
            RcPotentialDiagonal a = va;
            RcPotentialDiagonal b = vb;
            return a.dist.CompareTo(b.dist);
        }
    }
}