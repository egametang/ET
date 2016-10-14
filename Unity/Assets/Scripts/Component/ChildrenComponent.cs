using System.Collections.Generic;
using System.Linq;
using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	/// <summary>
	/// 父子层级信息
	/// </summary>
    public class ChildrenComponent: Component
    {
		[BsonIgnore]
		public Entity Parent { get; private set; }
		
		private readonly Dictionary<long, Entity> idChildren = new Dictionary<long, Entity>();

		[BsonIgnore]
		public int Count
		{
			get
			{
				return this.idChildren.Count;
			}
		}

		public void Add(Entity entity)
		{
			entity.GetComponent<ChildrenComponent>().Parent = this.Owner;
			this.idChildren.Add(entity.Id, entity);
		}

		public Entity Get(long id)
		{
			Entity entity = null;
			this.idChildren.TryGetValue(id, out entity);
			return entity;
		}

		public Entity[] GetChildren()
		{
			return this.idChildren.Values.ToArray();
		}

		private void Remove(Entity entity)
		{
			this.idChildren.Remove(entity.Id);
			entity.Dispose();
		}

		public void Remove(long id)
		{
			Entity entity;
			if (!this.idChildren.TryGetValue(id, out entity))
			{
				return;
			}
			this.Remove(entity);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (Entity entity in this.idChildren.Values.ToArray())
			{
				entity.Dispose();
			}

			this.Parent?.GetComponent<ChildrenComponent>().Remove(this.Id);
		}
    }

	public static partial class ChildrenHelper
	{
		public static void Add(this Entity entity, Entity child)
		{
			entity.GetComponent<ChildrenComponent>().Add(child);
		}

		public static void Remove(this Entity entity, long id)
		{
			entity.GetComponent<ChildrenComponent>().Remove(id);
		}
	}
}