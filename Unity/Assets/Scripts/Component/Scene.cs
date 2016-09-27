using System.Collections.Generic;
using System.Linq;

namespace Base
{
	public enum SceneType
	{
		Share,
		Game,
		Login,
		Lobby,
		Map,
		Launcher,
		Robot,
        BehaviorTreeScene,
		RobotClient,
	}

	public sealed class Scene: Entity<Scene>
	{
		public Scene Parent { get; set; }

		public string Name { get; set; }

		public SceneType SceneType { get; }

		private readonly Dictionary<long, Scene> Children = new Dictionary<long, Scene>();

		private readonly Dictionary<SceneType, HashSet<Scene>> SceneTypeChildren = new Dictionary<SceneType, HashSet<Scene>>();

		public Scene(string name, SceneType sceneType)
		{
			this.Name = name;
			this.SceneType = sceneType;
		}

		public int Count
		{
			get
			{
				return this.Children.Count;
			}
		}

		public void Add(Scene scene)
		{
			scene.Parent = this;
			this.Children.Add(scene.Id, scene);
			HashSet<Scene> listScene;
			if (!this.SceneTypeChildren.TryGetValue(scene.SceneType, out listScene))
			{
				listScene = new HashSet<Scene>();
				this.SceneTypeChildren.Add(scene.SceneType, listScene);
			}
			listScene.Add(scene);
		}

		public Scene Get(SceneType sceneType)
		{
			HashSet<Scene> scenes;
			if (!this.SceneTypeChildren.TryGetValue(sceneType, out scenes))
			{
				return null;
			}
			if (scenes.Count == 0)
			{
				return null;
			}
			return scenes.First();
		}

		public void Remove(SceneType sceneType)
		{
			HashSet<Scene> scenes;
			if (!this.SceneTypeChildren.TryGetValue(sceneType, out scenes))
			{
				return;
			}
			foreach (Scene scene in scenes)
			{
				Children.Remove(scene.Id);
				scene.Dispose();
			}
			this.SceneTypeChildren.Remove(sceneType);
		}

		public void Remove(long id)
		{
			Scene scene;
			if (!this.Children.TryGetValue(id, out scene))
			{
				return;
			}
			HashSet<Scene> scenes;
			if (!this.SceneTypeChildren.TryGetValue(scene.SceneType, out scenes))
			{
				return;
			}
			scenes.Remove(scene);
			scene.Dispose();
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			this.Parent?.Remove(this.Id);
		}
	}
}