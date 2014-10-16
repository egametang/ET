using System.Collections.Generic;
using Common.Config;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class NodeConfig: AConfig
    {
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