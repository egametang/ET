using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotRecast.Detour.TileCache
{
    public class DtTileCacheLayerBuildResult
    {
        public readonly int tx;
        public readonly int ty;
        public readonly List<byte[]> layers;

        public DtTileCacheLayerBuildResult(int tx, int ty, List<byte[]> layers)
        {
            this.tx = tx;
            this.ty = ty;
            this.layers = layers;
        }
    }
}