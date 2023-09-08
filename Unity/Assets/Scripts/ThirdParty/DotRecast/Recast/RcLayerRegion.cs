using System.Collections.Generic;

namespace DotRecast.Recast
{
    public class RcLayerRegion
    {
        public int id;
        public int layerId;
        public bool @base;
        public int ymin, ymax;
        public List<int> layers;
        public List<int> neis;

        public RcLayerRegion(int i)
        {
            id = i;
            ymin = 0xFFFF;
            layerId = 0xff;
            layers = new List<int>();
            neis = new List<int>();
        }
    };
}