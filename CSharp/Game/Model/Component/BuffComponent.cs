using System;
using System.Collections.Generic;
using Common.Base;
using Common.Event;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class BuffComponent : Component<Unit>
    {
        [BsonElement]
        private HashSet<Buff> buffs;

        [BsonIgnore]
        private Dictionary<ObjectId, Buff> idBuff;

        [BsonIgnore]
        private MultiMap<BuffType, Buff> typeBuff;

        public BuffComponent()
        {
            this.buffs = new HashSet<Buff>();
            this.idBuff = new Dictionary<ObjectId, Buff>();
            this.typeBuff = new MultiMap<BuffType, Buff>();
        }

        public override void BeginInit()
        {
            base.BeginInit();

            this.buffs = new HashSet<Buff>();
            this.idBuff = new Dictionary<ObjectId, Buff>();
            this.typeBuff = new MultiMap<BuffType, Buff>();
        }

        public override void EndInit()
        {
            base.EndInit();

            foreach (var buff in this.buffs)
            {
                this.idBuff.Add(buff.Id, buff);
                this.typeBuff.Add(buff.Config.Type, buff);
                AddToTimer(this.Owner, buff);
            }
        }

        private static void AddToTimer(Unit owner, Buff buff)
        {
            if (buff.Expiration == 0)
            {
                return;
            }
            Env env = new Env();
            env[EnvKey.OwnerId] = owner.Id;
            env[EnvKey.BuffId] = buff.Id;
            buff.TimerId = World.Instance.GetComponent<TimerComponent>()
                    .Add(buff.Expiration, CallbackType.BuffTimeoutCallback, env);
        }

        private static void RemoveFromTimer(Buff buff)
        {
            if (buff.Expiration == 0)
            {
                return;
            }
            World.Instance.GetComponent<TimerComponent>().Remove(buff.TimerId);
        }

        public void Add(Buff buff)
        {
            if (this.buffs.Contains(buff))
            {
                throw new ArgumentException(string.Format("already exist same buff, Id: {0} ConfigId: {1}", buff.Id, buff.Config.Id));
            }

            if (this.idBuff.ContainsKey(buff.Id))
            {
                throw new ArgumentException(string.Format("already exist same buff, Id: {0} ConfigId: {1}", buff.Id, buff.Config.Id));
            }

            Env env = new Env();
            env[EnvKey.Owner] = this.Owner;
            env[EnvKey.Buff] = buff;

            World.Instance.GetComponent<EventComponent<EventAttribute>>().Run(EventType.BeforeAddBuff, env);

            this.buffs.Add(buff);
            this.idBuff.Add(buff.Id, buff);
            this.typeBuff.Add(buff.Config.Type, buff);
            AddToTimer(this.Owner, buff);

            World.Instance.GetComponent<EventComponent<EventAttribute>>().Run(EventType.AfterAddBuff, env);
        }

        public Buff GetById(ObjectId id)
        {
            if (!this.idBuff.ContainsKey(id))
            {
                return null;
            }

            return this.idBuff[id];
        }

        public Buff GetOneByType(BuffType type)
        {
            return this.typeBuff.GetOne(type);
        }

        public Buff[] GetByType(BuffType type)
        {
            return this.typeBuff.GetAll(type);
        }

        private void Remove(Buff buff)
        {
            if (buff == null)
            {
                return;
            }

            Env env = new Env();
            env[EnvKey.Owner] = this.Owner;
            env[EnvKey.Buff] = buff;

            World.Instance.GetComponent<EventComponent<EventAttribute>>().Run(EventType.BeforeRemoveBuff, env);

            this.buffs.Remove(buff);
            this.idBuff.Remove(buff.Id);
            this.typeBuff.Remove(buff.Config.Type, buff);
            RemoveFromTimer(buff);

            World.Instance.GetComponent<EventComponent<EventAttribute>>().Run(EventType.AfterRemoveBuff, env);
        }

        public void RemoveById(ObjectId id)
        {
            Buff buff = this.GetById(id);
            this.Remove(buff);
        }

        public void RemoveByType(BuffType type)
        {
            Buff[] allbuffs = this.GetByType(type);
            foreach (Buff buff in allbuffs)
            {
                this.Remove(buff);
            }
        }
    }
}