using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	/// <summary>
	/// 父子层级信息
	/// </summary>
	public class ChildrenComponent<T>: Component where T : Entity
	{
		[BsonIgnore]
		public T Parent { get; private set; }

		private readonly Dictionary<long, T> idChildren = new Dictionary<long, T>();

		[BsonIgnore]
		public int Count
		{
			get
			{
				return this.idChildren.Count;
			}
		}

		public void Add(T child)
		{
			child.GetComponent<ChildrenComponent<T>>().Parent = (T) this.Owner;
			this.idChildren.Add(child.Id, child);
		}

		public T Get(long id)
		{
			T child = null;
			this.idChildren.TryGetValue(id, out child);
			return child;
		}

		public T[] GetChildren()
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
			T child;
			if (!this.idChildren.TryGetValue(id, out child))
			{
				return;
			}
			this.Remove(child);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (T child in this.idChildren.Values.ToArray())
			{
				child.Dispose();
			}

			this.Parent?.GetComponent<ChildrenComponent<T>>().Remove(this.Id);
		}
	}

	public static class ChildrenHelper
	{
		public static void AddChild<T>(this T parent, T child) where T : Entity
		{
			parent.GetComponent<ChildrenComponent<T>>().Add(child);
		}

		public static void RemoveChild<T>(this T entity, long id) where T : Entity
		{
			entity.GetComponent<ChildrenComponent<T>>().Remove(id);
		}

		public static T GetChild<T>(this T entity, long id) where T : Entity
		{
			return entity.GetComponent<ChildrenComponent<T>>().Get(id);
		}

		public static T[] GetChildren<T>(this T entity) where T : Entity
		{
			return entity.GetComponent<ChildrenComponent<T>>().GetChildren();
		}
	}
}