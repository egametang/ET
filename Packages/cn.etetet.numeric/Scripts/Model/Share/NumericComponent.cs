using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    [FriendOf(typeof (NumericComponent))]
    public static class NumericComponentSystem
    {
        public static float GetAsFloat(this NumericComponent self, int numericType)
        {
            return (float)self.GetByKey(numericType) / 10000;
        }

        public static int GetAsInt(this NumericComponent self, int numericType)
        {
            return (int)self.GetByKey(numericType);
        }

        public static long GetAsLong(this NumericComponent self, int numericType)
        {
            return self.GetByKey(numericType);
        }

        public static void Set(this NumericComponent self, int nt, float value)
        {
            self[nt] = (long)(value * 10000);
        }

        public static void Set(this NumericComponent self, int nt, int value)
        {
            self[nt] = value;
        }

        public static void Set(this NumericComponent self, int nt, long value)
        {
            self[nt] = value;
        }

        public static void SetNoEvent(this NumericComponent self, int numericType, long value)
        {
            self.Insert(numericType, value, false);
        }

        public static void Insert(this NumericComponent self, int numericType, long value, bool isPublicEvent = true)
        {
            long oldValue = self.GetByKey(numericType);
            if (oldValue == value)
            {
                return;
            }

            self.NumericDic[numericType] = value;

            if (numericType >= NumericType.Max)
            {
                self.Update(numericType, isPublicEvent);
                return;
            }

            if (isPublicEvent)
            {
                EventSystem.Instance.Publish(self.Scene(),
                    new NumbericChange() { Unit = self.GetParent<Unit>(), New = value, Old = oldValue, NumericType = numericType });
            }
        }

        public static long GetByKey(this NumericComponent self, int key)
        {
            long value = 0;
            self.NumericDic.TryGetValue(key, out value);
            return value;
        }

        public static void Update(this NumericComponent self, int numericType, bool isPublicEvent)
        {
            int final = (int)numericType / 10;
            int bas = final * 10 + 1;
            int add = final * 10 + 2;
            int pct = final * 10 + 3;
            int finalAdd = final * 10 + 4;
            int finalPct = final * 10 + 5;

            // 一个数值可能会多种情况影响，比如速度,加个buff可能增加速度绝对值100，也有些buff增加10%速度，所以一个值可以由5个值进行控制其最终结果
            // final = (((base + add) * (100 + pct) / 100) + finalAdd) * (100 + finalPct) / 100;
            long result = (long)(((self.GetByKey(bas) + self.GetByKey(add)) * (100 + self.GetAsFloat(pct)) / 100f + self.GetByKey(finalAdd)) *
                (100 + self.GetAsFloat(finalPct)) / 100f);
            self.Insert(final, result, isPublicEvent);
        }
    }
    
    public struct NumbericChange
    {
        public Unit Unit;
        public int NumericType;
        public long Old;
        public long New;
    }

    [ComponentOf(typeof (Unit))]
    public class NumericComponent: Entity, IAwake, ITransfer
    {
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, long> NumericDic = new Dictionary<int, long>();

        public long this[int numericType]
        {
            get
            {
                return this.GetByKey(numericType);
            }
            set
            {
                this.Insert(numericType, value);
            }
        }
    }
}