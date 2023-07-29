using System;
using MongoDB.Bson.Serialization.Attributes;
using TrueSync;

namespace ET
{
    [ChildOf(typeof(LSUnitComponent))]
    public class LSUnit: LSEntity, IAwake, ISerializeToEntity
    {
        public TSVector Position
        {
            get;
            set;
        }

        [BsonIgnore]
        public TSVector Forward
        {
            get => this.Rotation * TSVector.forward;
            set => this.Rotation = TSQuaternion.LookRotation(value, TSVector.up);
        }
        
        public TSQuaternion Rotation
        {
            get;
            set;
        }
    }
}