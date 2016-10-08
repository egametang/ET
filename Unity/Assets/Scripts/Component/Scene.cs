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
	}

	public sealed class Scene: Entity
	{
		public string Name { get; }

		public SceneType SceneType { get; }

		public Scene(string name, SceneType sceneType)
		{
			this.Name = name;
			this.SceneType = sceneType;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}