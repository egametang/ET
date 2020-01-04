﻿using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
	[ObjectSystem]
	public class UnitComponentSystem : AwakeSystem<UnitComponent>
	{
		public override void Awake(UnitComponent self)
		{
			self.Awake();
		}
	}
	
	public class UnitComponent: Entity
	{
		public static UnitComponent Instance { get; private set; }

		public Unit MyUnit;
		
		private readonly Dictionary<long, Unit> idUnits = new Dictionary<long, Unit>();

		public void Awake()
		{
			Instance = this;
		}

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

			Instance = null;
		}

		public void Add(Unit unit)
		{
			this.idUnits.Add(unit.Id, unit);
			unit.Parent = this;
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
	}
}