﻿using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
	namespace EventType
	{
		public class NumbericChange: DisposeObject
		{
			public static readonly NumbericChange Instance = new NumbericChange();
			
			public Entity Parent;
			public int NumericType;
			public long Old;
			public long New;
		}
	}
	
	[ObjectSystem]
	public class NumericComponentAwakeSystem : AwakeSystem<NumericComponent>
	{
		public override void Awake(NumericComponent self)
		{
			self.Awake();
		}
	}

	public class NumericComponent: Entity, IAwake, ITransfer
	{
		[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
		public Dictionary<int, long> NumericDic = new Dictionary<int, long>();

		public void Awake()
		{
			// 这里初始化base值
		}

		public float GetAsFloat(int numericType)
		{
			return (float)GetByKey(numericType) / 10000;
		}

		public int GetAsInt(int numericType)
		{
			return (int)GetByKey(numericType);
		}
		
		public long GetAsLong(int numericType)
		{
			return GetByKey(numericType);
		}

		public void Set(int nt, float value)
		{
			this[nt] = (int) (value * 10000);
		}

		public void Set(int nt, int value)
		{
			this[nt] = value;
		}
		
		public void Set(int nt, long value)
		{
			this[nt] = value;
		}

		public void SetNoEvent(int numericType, long value)
		{
			this.Insert(numericType,value,false);
		}
		
		
		public long this[int numericType]
		{
			get
			{
				return this.GetByKey(numericType);
			}
			set
			{
				
				this.Insert(numericType,value);
			}
		}
		
		private void Insert(int numericType, long value,bool isPublicEvent = true)
		{
			long oldValue = this.GetByKey(numericType);
			if (oldValue == value)
			{
				return;
			}

			NumericDic[numericType] = value;

			if (numericType >= NumericType.Max)
			{
				Update(numericType,isPublicEvent);
				return;
			}

			if (isPublicEvent)
			{
				EventType.NumbericChange args = EventType.NumbericChange.Instance;
				args.Parent = this.Parent;
				args.NumericType = numericType;
				args.Old = oldValue;
				args.New = value;
				Game.EventSystem.PublishClass(args);
			}
		}
		
		private long GetByKey(int key)
		{
			long value = 0;
			this.NumericDic.TryGetValue(key, out value);
			return value;
		}

		public void Update(int numericType,bool isPublicEvent)
		{
			int final = (int) numericType / 10;
			int bas = final * 10 + 1; 
			int add = final * 10 + 2;
			int pct = final * 10 + 3;
			int finalAdd = final * 10 + 4;
			int finalPct = final * 10 + 5;

			// 一个数值可能会多种情况影响，比如速度,加个buff可能增加速度绝对值100，也有些buff增加10%速度，所以一个值可以由5个值进行控制其最终结果
			// final = (((base + add) * (100 + pct) / 100) + finalAdd) * (100 + finalPct) / 100;
			long result = (long)(((this.GetByKey(bas) + this.GetByKey(add)) * (100 + this.GetAsFloat(pct)) / 100f + this.GetByKey(finalAdd)) * (100 + this.GetAsFloat(finalPct)) / 100f);
			this.Insert(final,result,isPublicEvent);
		}
	}
}