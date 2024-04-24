using DotRecast.Core;

namespace DotRecast.Detour.Io
{
    public class DtNavMeshParamsReader
    {
        public DtNavMeshParams Read(RcByteBuffer bb)
        {
            DtNavMeshParams option = new DtNavMeshParams();
            option.orig.x = bb.GetFloat();
            option.orig.y = bb.GetFloat();
            option.orig.z = bb.GetFloat();
            option.tileWidth = bb.GetFloat();
            option.tileHeight = bb.GetFloat();
            option.maxTiles = bb.GetInt();
            option.maxPolys = bb.GetInt();
            return option;
        }
    }
}