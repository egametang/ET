using ETModel;
using MongoDB.Bson.Serialization.Attributes;

namespace PF
{
    public class AStarConfig: Component
    {
        public static AStarConfig Instance { get; private set; }

        public AStarConfig()
        {
            Instance = this;
        }
        
        [BsonIgnore]
        public NavGraph[] graphs;

        public ushort debugPathID;

        public bool prioritizeGraphs;
        
        public bool fullGetNearestSearch;

        public float heuristicScale = 1;

        public Heuristic heuristic = Heuristic.Euclidean;

        public PathLog logPathResults = PathLog.None;
        
        // prioritizeGraphs为true才起作用
        public float prioritizeGraphsLimit = 1f;
        
        public float maxFrameTime = 1f;
        
        public EuclideanEmbedding euclideanEmbedding = new EuclideanEmbedding();
        
        public float maxNearestNodeDistance = 4f;

        /** Max Nearest Node Distance Squared.
         * \see #maxNearestNodeDistance */
        [BsonIgnore]
        public float maxNearestNodeDistanceSqr {
            get
            {
                return maxNearestNodeDistance * maxNearestNodeDistance;
            }
        }

        [BsonIgnore]
        public PathHandler debugPathData;
        
        [BsonIgnore]
        public PathProcessor pathProcessor;
    }
}