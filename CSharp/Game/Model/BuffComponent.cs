using System.Collections.Generic;
using System.ComponentModel;
using Common.Base;
using MongoDB.Bson;
using Component = Common.Base.Component;

namespace Model
{
    public class BuffComponent: Component, ISupportInitialize
    {
        public HashSet<Buff> Buffs { get; set; }

        private Dictionary<ObjectId, Buff> buffGuidDict { get; set; }

        private MultiMap<BuffType, Buff> buffTypeMMap { get; set; }

        public BuffComponent()
        {
            this.Buffs = new HashSet<Buff>();
            this.buffGuidDict = new Dictionary<ObjectId, Buff>();
            this.buffTypeMMap = new MultiMap<BuffType, Buff>();
        }

        void ISupportInitialize.BeginInit()
        {
        }

        void ISupportInitialize.EndInit()
        {
            foreach (var buff in this.Buffs)
            {
                this.buffGuidDict.Add(buff.Guid, buff);
                this.buffTypeMMap.Add(buff.Type, buff);
            }
        }

        public bool Add(Buff buff)
        {
            if (this.Buffs.Contains(buff))
            {
                return false;
            }

            if (this.buffGuidDict.ContainsKey(buff.Guid))
            {
                return false;
            }

            if (this.buffTypeMMap.GetOne(buff.Type) != null)
            {
                return false;
            }

            this.Buffs.Add(buff);
            this.buffGuidDict.Add(buff.Guid, buff);
            this.buffTypeMMap.Add(buff.Type, buff);

            return true;
        }

        public Buff GetByGuid(ObjectId guid)
        {
            if (!this.buffGuidDict.ContainsKey(guid))
            {
                return null;
            }

            return this.buffGuidDict[guid];
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

            this.Buffs.Remove(buff);
            this.buffGuidDict.Remove(buff.Guid);
            this.buffTypeMMap.Remove(buff.Type, buff);

            return true;
        }

        public bool RemoveByGuid(ObjectId guid)
        {
            Buff buff = this.GetByGuid(guid);
            return this.Remove(buff);
        }

        public void RemoveByType(BuffType type)
        {
            Buff[] buffs = this.GetByType(type);
            foreach (Buff buff in buffs)
            {
                this.Remove(buff);
            }
        }
    }
}