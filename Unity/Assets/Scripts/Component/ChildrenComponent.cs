using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	/// <summary>
	/// 父子层级信息
	/// </summary>
    public class ChildrenComponent: Component
    {
		[BsonIgnore]
		public Unit Parent { get; private set; }
		
		private readonly Dictionary<long, Unit> idChildren = new Dictionary<long, Unit>();

		[BsonIgnore]
		public int Count
		{
			get
			{
				return this.idChildren.Count;
			}
		}

		public void Add(Unit unit)
		{
			unit.GetComponent<ChildrenComponent>().Parent = this.Owner;
			this.idChildren.Add(unit.Id, unit);
		}

		public Unit Get(long id)
		{
			Unit unit = null;
			this.idChildren.TryGetValue(id, out unit);
			return unit;
		}

		public Unit[] GetChildren()
		{
			return this.idChildren.Values.ToArray();
		}

		private void Remove(Unit unit)
		{
			this.idChildren.Remove(unit.Id);
			unit.Dispose();
		}

		public void Remove(long id)
		{
			Unit unit;
			if (!this.idChildren.TryGetValue(id, out unit))
			{
				return;
			}
			this.Remove(unit);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (Unit entity in this.idChildren.Values.ToArray())
			{
				entity.Dispose();
			}

			this.Parent?.GetComponent<ChildrenComponent>().Remove(this.Id);
		}
    }

	public static partial class ChildrenHelper
	{
		public static void Add(this Unit unit, Unit child)
		{
			unit.GetComponent<ChildrenComponent>().Add(child);
		}

		public static void Remove(this Unit unit, long id)
		{
			unit.GetComponent<ChildrenComponent>().Remove(id);
		}
	}
}