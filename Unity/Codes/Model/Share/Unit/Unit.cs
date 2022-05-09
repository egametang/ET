using MongoDB.Bson.Serialization.Attributes;
using UnityEngine;

namespace ET
{
    public class Unit: Entity, IAwake<int>
    {
        public int ConfigId { get; set; } //配置表id

        [BsonIgnore]
        public UnitConfig Config => UnitConfigCategory.Instance.Get(this.ConfigId);

        public UnitType Type => (UnitType)UnitConfigCategory.Instance.Get(this.ConfigId).Type;

        [BsonElement]
        private Vector3 position; //坐标

        [BsonIgnore]
        public Vector3 Position
        {
            get => this.position;
            set
            {
                Vector3 oldPos = this.position;
                this.position = value;
                Game.EventSystem.Publish(this, new EventType.ChangePosition() { OldPos = oldPos });
            }
        }

        [BsonIgnore]
        public Vector3 Forward
        {
            get => this.Rotation * Vector3.forward;
            set => this.Rotation = Quaternion.LookRotation(value, Vector3.up);
        }
        
        [BsonElement]
        private Quaternion rotation;
        
        [BsonIgnore]
        public Quaternion Rotation
        {
            get => this.rotation;
            set
            {
                this.rotation = value;
                Game.EventSystem.Publish(this, new EventType.ChangeRotation());
            }
        }
    }
}