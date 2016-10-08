using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	/// <summary>
	/// 父子层级信息
	/// </summary>
    public class ChildrenComponent : Component
    {
		[BsonIgnore]
		public Scene Parent { get; private set; }
		
		private readonly Dictionary<long, Scene> idChildren = new Dictionary<long, Scene>();
		
		private readonly Dictionary<string, Scene> nameChildren = new Dictionary<string, Scene>();

		[BsonIgnore]
		public int Count
		{
			get
			{
				return this.idChildren.Count;
			}
		}

		public void Add(Scene scene)
		{
			scene.GetComponent<ChildrenComponent>().Parent = this.GetOwner<Scene>();
			this.idChildren.Add(scene.Id, scene);
			this.nameChildren.Add(scene.Name, scene);
		}

		public Scene[] GetChildren()
		{
			return this.idChildren.Values.ToArray();
		}

		private void Remove(Scene scene)
		{
			this.idChildren.Remove(scene.Id);
			this.nameChildren.Remove(scene.Name);
			scene.Dispose();
		}

		public void Remove(long id)
		{
			Scene scene;
			if (!this.idChildren.TryGetValue(id, out scene))
			{
				return;
			}
			this.Remove(scene);
		}

		public void Remove(string name)
		{
			Scene scene;
			if (!this.nameChildren.TryGetValue(name, out scene))
			{
				return;
			}
			this.Remove(scene);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (Scene scene in this.idChildren.Values.ToArray())
			{
				scene.Dispose();
			}

			this.Parent?.GetComponent<ChildrenComponent>().Remove(this.Id);
		}
    }

	public static class LevelHelper
	{
		public static void Add(this Scene scene, Scene child)
		{
			scene.GetComponent<ChildrenComponent>().Add(child);
		}

		public static void Remove(this Scene scene, long id)
		{
			scene.GetComponent<ChildrenComponent>().Remove(id);
		}

		public static void Remove(this Scene scene, string name)
		{
			scene.GetComponent<ChildrenComponent>().Remove(name);
		}
	}
}