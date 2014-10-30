using System.Collections.Generic;
using Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class BuffComponent: Component<Unit>
    {
        [BsonElement]
        public HashSet<Buff> buffs;

        [BsonIgnore]
        public Dictionary<ObjectId, Buff> idBuff;

        [BsonIgnore]
        public MultiMap<BuffType, Buff> typeBuff;

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
            }
        }
    }
}