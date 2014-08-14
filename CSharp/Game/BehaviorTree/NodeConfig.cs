using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace BehaviorTree
{
    public class NodeConfig
    {
        public uint Id { get; set; }

        public int Type { get; set; }

        [BsonIgnoreIfNull]
        public List<string> Args { get; set; }

        [BsonIgnoreIfNull]
        public List<NodeConfig> SubConfigs { get; set; }

        public void AddArgs(string arg)
        {
            if (this.Args == null)
            {
                this.Args = new List<string>();
            }

            this.Args.Add(arg);
        }

        public void AddSubConfig(NodeConfig subConfig)
        {
            if (this.SubConfigs == null)
            {
                this.SubConfigs = new List<NodeConfig>();
            }

            this.SubConfigs.Add(subConfig);
        }
    }
}