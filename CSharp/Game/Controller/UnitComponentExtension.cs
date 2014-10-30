using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using MongoDB.Bson;

namespace Controller
{
    /// <summary>
    /// 控制复杂的unit逻辑,可以reload
    /// </summary>
    public static class UnitComponentExtension
    {
        public static void Add(this UnitComponent unitComponent, Unit unit)
        {
            unitComponent.units.Add(unit.Id, unit);
            if (!unitComponent.typeUnits.ContainsKey(unit.Config.Type))
            {
                unitComponent.typeUnits.Add(unit.Config.Type, new Dictionary<ObjectId, Unit>());
            }
            unitComponent.typeUnits[unit.Config.Type].Add(unit.Id, unit);
        }

        public static Unit Get(this UnitComponent unitComponent, ObjectId id)
        {
            Unit unit = null;
            unitComponent.units.TryGetValue(id, out unit);
            return unit;
        }

        public static Unit[] GetOneType(this UnitComponent unitComponent, int type)
        {
            Dictionary<ObjectId, Unit> oneTypeUnits = null;
            if (!unitComponent.typeUnits.TryGetValue(type, out oneTypeUnits))
            {
                return new Unit[0];
            }
            return oneTypeUnits.Values.ToArray();
        }

        public static bool Remove(this UnitComponent unitComponent, Unit unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }
            if (!unitComponent.units.Remove(unit.Id))
            {
                return false;
            }
            if (!unitComponent.typeUnits[unit.Config.Type].Remove(unit.Id))
            {
                return false;
            }
            return true;
        }

        public static bool Remove(this UnitComponent unitComponent, ObjectId id)
        {
            Unit unit = unitComponent.Get(id);
            if (unit == null)
            {
                return false;
            }
            return unitComponent.Remove(unit);
        }
    }
}