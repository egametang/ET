using System.Collections.Generic;
using Common.Base;
using MongoDB.Bson;

namespace Model
{
    public class UnitComponent: Component<World>
    {
        public readonly Dictionary<ObjectId, Unit> units =
                new Dictionary<ObjectId, Unit>();

        public readonly Dictionary<int, Dictionary<ObjectId, Unit>> typeUnits =
                new Dictionary<int, Dictionary<ObjectId, Unit>>();
    }
}