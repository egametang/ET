using System.Collections.Generic;
using Common.Config;
using MongoDB.Bson.Serialization.Attributes;

namespace BehaviorTree
{
    public class NodeConfig: IConfig
    {
        public int Id { get; set; }

        [BsonIgnoreIfNull]
        public List<string> Args { get; set; }

        [BsonIgnoreIfNull]
        public List<NodeConfig> SubConfigs { get; set; }
    }

    [Config]
    public class NodeCategory: ACategory<NodeConfig>
    {
    }
}