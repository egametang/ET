using System;
using DotRecast.Core;

namespace DotRecast.Detour
{
    public class DtFindNearestPolyQuery : IDtPolyQuery
    {
        private readonly DtNavMeshQuery _query;
        private readonly RcVec3f _center;
        private long _nearestRef;
        private RcVec3f _nearestPt;
        private bool _overPoly;
        private float _nearestDistanceSqr;

        public DtFindNearestPolyQuery(DtNavMeshQuery query, RcVec3f center)
        {
            this._query = query;
            this._center = center;
            _nearestDistanceSqr = float.MaxValue;
            _nearestPt = center;
        }

        public void Process(DtMeshTile tile, DtPoly poly, long refs)
        {
            // Find nearest polygon amongst the nearby polygons.
            _query.ClosestPointOnPoly(refs, _center, out var closestPtPoly, out var posOverPoly);

            // If a point is directly over a polygon and closer than
            // climb height, favor that instead of straight line nearest point.
            float d = 0;
            RcVec3f diff = _center.Subtract(closestPtPoly);
            if (posOverPoly)
            {
                d = Math.Abs(diff.y) - tile.data.header.walkableClimb;
                d = d > 0 ? d * d : 0;
            }
            else
            {
                d = RcVec3f.LenSqr(diff);
            }

            if (d < _nearestDistanceSqr)
            {
                _nearestPt = closestPtPoly;
                _nearestDistanceSqr = d;
                _nearestRef = refs;
                _overPoly = posOverPoly;
            }
        }

        public long NearestRef()
        {
            return _nearestRef;
        }

        public RcVec3f NearestPt()
        {
            return _nearestPt;
        }

        public bool OverPoly()
        {
            return _overPoly;
        }
    }
}