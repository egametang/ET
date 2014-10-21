using System;
using System.Collections.Generic;
using Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Component = Common.Base.Component;

namespace Model
{
    public class BuffComponent: Component
    {
        [BsonElement]
        private HashSet<Buff> buffs { get; set; }

        private Dictionary<ObjectId, Buff> buffIdDict { get; set; }

        private MultiMap<BuffType, Buff> buffTypeMMap { get; set; }

        public BuffComponent()
        {
            this.buffs = new HashSet<Buff>();
            this.buffIdDict = new Dictionary<ObjectId, Buff>();
            this.buffTypeMMap = new MultiMap<BuffType, Buff>();
        }

        public override void EndInit()
        {
            base.EndInit();

            foreach (var buff in this.buffs)
            {
                this.buffIdDict.Add(buff.Id, buff);
                this.buffTypeMMap.Add(buff.Config.Type, buff);
            }
        }

        public void Add(Buff buff)
        {
            if (this.buffs.Contains(buff))
            {
                throw new ArgumentException(string.Format("already exist same buff, Id: {0} ConfigId: {1}", buff.Id, buff.Config.Id));
            }

            if (this.buffIdDict.ContainsKey(buff.Id))
            {
                throw new ArgumentException(string.Format("already exist same buff, Id: {0} ConfigId: {1}", buff.Id, buff.Config.Id));
            }

            this.buffs.Add(buff);
            this.buffIdDict.Add(buff.Id, buff);
            this.buffTypeMMap.Add(buff.Config.Type, buff);
        }

        public Buff GetById(ObjectId id)
        {
            if (!this.buffIdDict.ContainsKey(id))
            {
                return null;
            }

            return this.buffIdDict[id];
        }

        public Buff GetOneByType(BuffType type)
        {
            return this.buffTypeMMap.GetOne(type);
        }

        public Buff[] GetByType(BuffType type)
        {
            return this.buffTypeMMap.GetByKey(type);
        }

        private bool Remove(Buff buff)
        {
            if (buff == null)
            {
                return false;
            }

            this.buffs.Remove(buff);
            this.buffIdDict.Remove(buff.Id);
            this.buffTypeMMap.Remove(buff.Config.Type, buff);

            return true;
        }

        public bool RemoveById(ObjectId id)
        {
            Buff buff = this.GetById(id);
            return this.Remove(buff);
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