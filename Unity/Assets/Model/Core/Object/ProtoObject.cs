using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace ET
{
    public class ProtoObject: Object
    {

#if UNITY_EDITOR
        public virtual void BeginInitEditor()
        {
        }
#endif
        
        public object Clone()
        {
            byte[] bytes = ProtobufHelper.ToBytes(this);
            return ProtobufHelper.FromBytes(this.GetType(), bytes, 0, bytes.Length);
        }
    }

    public class ProtoObject<M, N>: ProtoObject where M : new() where N : ProtoObject, IConfig
    {
        [ProtoIgnore]
        public Dictionary<int, N> dict = new Dictionary<int, N>();

        public static M Instance
        {
            get;
            protected set;
        }

        public N Get(int id)
        {
            this.dict.TryGetValue(id, out N item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {typeof(N).Name}，配置id: {id}");
            }

            return item;
        }

        public bool ContainConfig(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, N> GetAll()
        {
            return this.dict;
        }

        public N GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }

            return this.dict.Values.First();
        }

        public N GetRandomOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }

            int index = RandomHelper.RandomNumber(0,this.dict.Count);
            foreach (var v in this.dict)
            {
                index--;
                if (index < 0)
                {
                    return v.Value;
                }
            }
            return null;
            
        }
    }
}