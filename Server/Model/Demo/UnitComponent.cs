﻿using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
	public class UnitComponent: Entity
	{
		[BsonElement]
		[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
		private readonly Dictionary<long, Unit> idUnits = new Dictionary<long, Unit>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();

			foreach (Unit unit in this.idUnits.Values)
			{
				unit.Dispose();
			}
			this.idUnits.Clear();
		}

		public void Add(Unit unit)
		{
			this.idUnits.Add(unit.Id, unit);
			unit.Parent = this;
		}

		public Unit Get(long id)
		{
			this.idUnits.TryGetValue(id, out Unit unit);
			return unit;
		}

		public void Remove(long id)
		{
			Unit unit;
			this.idUnits.TryGetValue(id, out unit);
			this.idUnits.Remove(id);
			unit?.Dispose();
		}

		public void RemoveNoDispose(long id)
		{
			this.idUnits.Remove(id);
		}

		public int Count
		{
			get
			{
				return this.idUnits.Count;
			}
		}

		public Unit[] GetAll()
		{
			return this.idUnits.Values.ToArray();
		}
	}
}