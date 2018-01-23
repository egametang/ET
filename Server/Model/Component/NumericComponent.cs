using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Model
{
	[ObjectSystem]
	public class NumericComponentSystem : ObjectSystem<NumericComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}

	public class NumericComponent : Component, ISerializeToEntity
	{
		[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
		public Dictionary<NumericType, int> NumericDic;

		public void Awake()
		{
			this.NumericDic = new Dictionary<NumericType, int>();
		}

		public float GetAsFloat(NumericType numericType)
		{
			return (float)GetByKey(numericType) / 10000;
		}

		public int GetAsInt(NumericType numericType)
		{
			return this[numericType];
		}

		public void Set(NumericType nt, float value)
		{
			this[nt] = (int)(value * 10000);
		}

		public void Set(NumericType nt, int value)
		{
			this[nt] = value;
		}

		public int this[NumericType numericType]
		{
			get
			{
				return this.GetByKey(numericType);
			}
			set
			{
				int v = this.GetByKey(numericType);
				if (v == value)
				{
					return;
				}

				NumericDic[numericType] = value;
				Game.EventSystem.Run(EventIdType.NumbericChange, this.Parent.Id, numericType, value);
			}
		}

		private int GetByKey(NumericType key)
		{
			int value;
			this.NumericDic.TryGetValue(key, out value);
			return value;
		}
	}
}