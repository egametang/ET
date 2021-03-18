using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
	public class UnitComponent: Entity
	{
		[BsonElement]
		[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
		public readonly Dictionary<long, Unit> idUnits = new Dictionary<long, Unit>();
		
		public int Count
		{
			get
			{
				return this.idUnits.Count;
			}
		}
	}
}