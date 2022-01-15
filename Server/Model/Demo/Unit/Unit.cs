﻿using System;
using MongoDB.Bson.Serialization.Attributes;
using UnityEngine;

namespace ET
{
    public sealed class Unit: Entity, IAwake<int>
    {
        public int ConfigId; //配置表id

        [BsonIgnore]
        public UnitType Type => (UnitType)this.Config.Type;

        [BsonIgnore]
        public UnitConfig Config => UnitConfigCategory.Instance.Get(this.ConfigId);

        private Vector3 position; //坐标

        public Vector3 Position
        {
            get => this.position;
            set
            {
                this.position = value;
                Game.EventSystem.Publish(new EventType.ChangePosition() { Unit = this });
            }
        }

        [BsonIgnore]
        public Vector3 Forward
        {
            get => this.Rotation * Vector3.forward;
            set => this.Rotation = Quaternion.LookRotation(value, Vector3.up);
        }

        private Quaternion rotation;
        public Quaternion Rotation
        {
            get => this.rotation;
            set
            {
                this.rotation = value;
                Game.EventSystem.Publish(new EventType.ChangeRotation() {Unit = this});
            }
        }
    }
}