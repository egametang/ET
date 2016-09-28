using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	/// <summary>
	/// 父子层级信息
	/// </summary>
    public class LevelComponent<T> : Component<T> where T: Entity<T>
    {
		[BsonIgnore]
		public T Parent { get; private set; }

		[BsonElement]
		private readonly List<T> children = new List<T>();
		
		private readonly Dictionary<long, T> idChildren = new Dictionary<long, T>();
		
		private readonly Dictionary<string, T> nameChildren = new Dictionary<string, T>();

		[BsonIgnore]
		public int Count
		{
			get
			{
				return this.idChildren.Count;
			}
		}

		public void Add(T t)
		{
			t.GetComponent<LevelComponent<T>>().Parent = this.Owner;
			this.children.Add(t);
			this.idChildren.Add(t.Id, t);
			this.nameChildren.Add(t.Name, t);
		}

		public T[] GetChildren()
		{
			return this.children.ToArray();
		}

		private void Remove(T t)
		{
			this.idChildren.Remove(t.Id);
			this.nameChildren.Remove(t.Name);
			t.Dispose();
		}

		public void Remove(long id)
		{
			T t;
			if (!this.idChildren.TryGetValue(id, out t))
			{
				return;
			}
			this.Remove(t);
		}

		public void Remove(string name)
		{
			T t;
			if (!this.nameChildren.TryGetValue(name, out t))
			{
				return;
			}
			this.Remove(t);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (T t in this.children)
			{
				t.Dispose();
			}

			this.Parent?.GetComponent<LevelComponent<T>>().Remove(this.Id);
		}
    }

	public static class LevelHelper
	{
		public static void AddChild<T>(this Entity<T> entity, T t) where T : Entity<T>
		{
			entity.GetComponent<LevelComponent<T>>().Add(t);
		}

		public static void RemoveChild<T>(this Entity<T> entity, long id) where T : Entity<T>
		{
			entity.GetComponent<LevelComponent<T>>().Remove(id);
		}

		public static void RemoveChild<T>(this Entity<T> entity, string name) where T : Entity<T>
		{
			entity.GetComponent<LevelComponent<T>>().Remove(name);
		}
	}
}