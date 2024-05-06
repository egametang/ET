using System.Collections.Generic;
using DotRecast.Core;

namespace DotRecast.Detour.TileCache.Io.Compress
{
    public class DtTileCacheCompressorFactory : IDtTileCacheCompressorFactory
    {
        public static readonly DtTileCacheCompressorFactory Shared = new DtTileCacheCompressorFactory();

        private readonly Dictionary<int, IRcCompressor> _compressors = new Dictionary<int, IRcCompressor>();

        public bool TryAdd(int type, IRcCompressor compressor)
        {
            if (0 == type)
                return false;

            _compressors[type] = compressor;
            return true;
        }

        public IRcCompressor Create(int compatibility)
        {
            // default
            if (0 == compatibility)
            {
                return DtTileCacheFastLzCompressor.Shared;
            }

            if (!_compressors.TryGetValue(compatibility, out var comp))
            {
                return null;
            }

            return comp;
        }
    }
}