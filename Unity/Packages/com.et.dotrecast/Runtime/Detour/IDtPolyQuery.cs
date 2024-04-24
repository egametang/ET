namespace DotRecast.Detour
{
    public interface IDtPolyQuery
    {
        void Process(DtMeshTile tile, DtPoly poly, long refs);
    }
}