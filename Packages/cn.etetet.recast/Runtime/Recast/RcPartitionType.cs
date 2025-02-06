using System.Linq;

namespace DotRecast.Recast
{
    public class RcPartitionType
    {
        public static readonly RcPartitionType WATERSHED = new RcPartitionType(RcPartition.WATERSHED);
        public static readonly RcPartitionType MONOTONE = new RcPartitionType(RcPartition.MONOTONE);
        public static readonly RcPartitionType LAYERS = new RcPartitionType(RcPartition.LAYERS);

        public static readonly RcPartitionType[] Values = { WATERSHED, MONOTONE, LAYERS };

        public readonly RcPartition EnumType;
        public readonly int Value;
        public readonly string Name;

        private RcPartitionType(RcPartition et)
        {
            EnumType = et;
            Value = (int)et;
            Name = et.ToString();
        }

        public static RcPartition OfValue(int value)
        {
            return Values.FirstOrDefault(x => x.Value == value)?.EnumType ?? RcPartition.WATERSHED;
        }

        public static RcPartitionType Of(RcPartition partition)
        {
            return Values.FirstOrDefault(x => x.EnumType == partition) ?? WATERSHED;
        }
    }
}