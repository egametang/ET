using System;
using System.Collections.Generic;
using System.Linq;
using Common.Base;
using MongoDB.Bson;

namespace Model
{
    public class UnitComponent : Component<World>
    {
        private readonly Dictionary<ObjectId, Unit> units =
                new Dictionary<ObjectId, Unit>();

        private readonly Dictionary<int, Dictionary<ObjectId, Unit>> typeUnits =
                new Dictionary<int, Dictionary<ObjectId, Unit>>();

        public void Add(Unit unit)
        {
            this.units.Add(unit.Id, unit);
            if (!this.typeUnits.ContainsKey(unit.Config.Type))
            {
                this.typeUnits.Add(unit.Config.Type, new Dictionary<ObjectId, Unit>());
            }
            this.typeUnits[unit.Config.Type].Add(unit.Id, unit);
        }

        public Unit Get(ObjectId id)
        {
            Unit unit = null;
            this.units.TryGetValue(id, out unit);
            return unit;
        }

        public Unit[] GetOneType(int type)
        {
            Dictionary<ObjectId, Unit> oneTypeUnits = null;
            if (!this.typeUnits.TryGetValue(type, out oneTypeUnits))
            {
                return new Unit[0];
            }
            return oneTypeUnits.Values.ToArray();
        }

        public bool Remove(Unit unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }
            if (!this.units.Remove(unit.Id))
            {
                return false;
            }
            if (!this.typeUnits[unit.Config.Type].Remove(unit.Id))
            {
                return false;
            }
            return true;
        }

        public bool Remove(ObjectId id)
        {
            Unit unit = this.Get(id);
            if (unit == null)
            {
                return false;
            }
            return this.Remove(unit);
        }
    }
}