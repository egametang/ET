﻿using System.Collections.Generic;
using System.Linq;

namespace Model
{
	[ObjectEvent]
	public class UnitComponentEvent : ObjectEvent<UnitComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}
	
	public class UnitComponent: Component
	{
		public static UnitComponent Instance { get; private set; }

		public Unit MyUnit;
		
		private readonly Dictionary<long, Unit> idUnits = new Dictionary<long, Unit>();

		public void Awake()
		{
			Instance = this;
		}

		public void Add(Unit unit)
		{
			this.idUnits.Add(unit.Id, unit);
		}

		public Unit Get(long id)
		{
			Unit unit;
			this.idUnits.TryGetValue(id, out unit);
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

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			base.Dispose();

			foreach (Unit unit in this.idUnits.Values)
			{
				unit.Dispose();
			}

			Instance = null;
		}
	}
}