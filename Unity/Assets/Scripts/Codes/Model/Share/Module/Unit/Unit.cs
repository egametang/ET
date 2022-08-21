using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;
using UnityEngine;

namespace ET
{
    [ChildOf(typeof(UnitComponent))]
    [DebuggerDisplay("ViewGoName,nq")]
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
                EventSystem.Instance.Publish(this.DomainScene(), new EventType.ChangePosition() { Unit = this, OldPos = oldPos });
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
                EventSystem.Instance.Publish(this.DomainScene(), new EventType.ChangeRotation() { Unit = this });
            }
        }

        
#if ENABLE_CODES
        protected override string ViewGoName
        {
            get
            {
                return $"{this.GetType().Name} ({this.Id})";
            }
        }
#endif
    }
}