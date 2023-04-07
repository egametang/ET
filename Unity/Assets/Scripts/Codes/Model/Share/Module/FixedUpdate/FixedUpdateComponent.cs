using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class FixedUpdateComponent: Entity
    {
        public long idGenerator;
        
        public int ToFrame;
        public int NowFrame;
        
        public struct UpdateInfo
        {
            public long UpdateId;
            public int Type;
            public Entity Entity;
        }

        [BsonIgnore]
        public SortedSet<long> updateIds = new SortedSet<long>();
        [BsonIgnore]
        public Queue<long> addUpdateIds = new Queue<long>();
        [BsonIgnore]
        public Queue<long> removeUpdateIds = new Queue<long>();
        [BsonIgnore]
        public Dictionary<long, UpdateInfo> updateEntities = new Dictionary<long, UpdateInfo>();
    }
}