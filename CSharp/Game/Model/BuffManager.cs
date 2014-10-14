using System.Collections.Generic;
using System.ComponentModel;
using Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Component = Common.Base.Component;

namespace Model
{
    public class BuffManager: Component, ISupportInitialize
    {
        public HashSet<Buff> Buffs { get; private set; }

        [BsonIgnore]
        public Dictionary<ObjectId, Buff> BuffGuidDict { get; private set; }

        [BsonIgnore]
        public MultiMap<int, Buff> BuffTypeDict { get; private set; }

        public BuffManager()
        {
            this.Buffs = new HashSet<Buff>();
            this.BuffGuidDict = new Dictionary<ObjectId, Buff>();
            this.BuffTypeDict = new MultiMap<int, Buff>();
        }

        void ISupportInitialize.BeginInit()
        {
        }

        void ISupportInitialize.EndInit()
        {
            foreach (var buff in this.Buffs)
            {
                this.BuffGuidDict.Add(buff.Guid, buff);
            }

            foreach (var buff in this.Buffs)
            {
                this.BuffTypeDict.Add(buff.Type, buff);
            }
        }

        public bool Add(Buff buff)
        {
            if (this.Buffs.Contains(buff))
            {
                return false;
            }

            if (this.BuffGuidDict.ContainsKey(buff.Guid))
            {
                return false;
            }

            if (this.BuffTypeDict.Get(buff.Type) != null)
            {
                return false;
            }

            this.Buffs.Add(buff);
            this.BuffGuidDict.Add(buff.Guid, buff);
            this.BuffTypeDict.Add(buff.Type, buff);

            return true;
        }

        public Buff GetByGuid(ObjectId guid)
        {
            if (!this.BuffGuidDict.ContainsKey(guid))
            {
                return null;
            }

            return this.BuffGuidDict[guid];
        }

        public Buff GetByType(int type)
        {
            return this.BuffTypeDict.Get(type);
        }

        private bool Remove(Buff buff)
        {
            if (buff == null)
            {
                return false;
            }

            this.Buffs.Remove(buff);
            this.BuffGuidDict.Remove(buff.Guid);
            this.BuffTypeDict.Remove(buff.Type, buff);

            return true;
        }

        public bool RemoveByGuid(ObjectId guid)
        {
            Buff buff = this.GetByGuid(guid);
            return this.Remove(buff);
        }

        public bool RemoveByType(int type)
        {
            Buff buff = this.GetByType(type);
            return this.Remove(buff);
        }
    }
}