using System;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class PolyQueryInvoker : IDtPolyQuery
    {
        public readonly Action<DtMeshTile, DtPoly, long> _callback;

        public PolyQueryInvoker(Action<DtMeshTile, DtPoly, long> callback)
        {
            _callback = callback;
        }

        public void Process(DtMeshTile tile, DtPoly poly, long refs)
        {
            _callback?.Invoke(tile, poly, refs);
        }
    }
}