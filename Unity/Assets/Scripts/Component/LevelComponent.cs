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

		private List<T> children;

		[BsonElement, BsonIgnoreIfNull]
		private Dictionary<long, T> idChildren;

		[BsonElement, BsonIgnoreIfNull]
		private Dictionary<string, T> nameChildren;

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
			if (this.idChildren == null)
			{
				this.children = new List<T>();
				this.idChildren = new Dictionary<long, T>();
				this.nameChildren = new Dictionary<string, T>();
			}
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
			if (this.idChildren.Count == 0)
			{
				this.children = null;
				this.idChildren = null;
				this.nameChildren = null;
			}
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
}