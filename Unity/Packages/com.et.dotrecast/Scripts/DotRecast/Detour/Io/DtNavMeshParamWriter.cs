using System.IO;
using DotRecast.Core;

namespace DotRecast.Detour.Io
{
    public class DtNavMeshParamWriter : DtWriter
    {
        public void Write(BinaryWriter stream, DtNavMeshParams option, RcByteOrder order)
        {
            Write(stream, option.orig.x, order);
            Write(stream, option.orig.y, order);
            Write(stream, option.orig.z, order);
            Write(stream, option.tileWidth, order);
            Write(stream, option.tileHeight, order);
            Write(stream, option.maxTiles, order);
            Write(stream, option.maxPolys, order);
        }
    }
}