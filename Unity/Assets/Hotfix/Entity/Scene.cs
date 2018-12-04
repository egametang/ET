using ETModel;

namespace ETHotfix
{
	public sealed class Scene: Entity
	{
		public ETModel.Scene ModelScene { get; set; } = new ETModel.Scene();

		public string Name { get; set; }

		public Scene()
		{
			this.InstanceId = IdGenerater.GenerateId();
		}

		public Scene(long id): base(id)
		{
			this.InstanceId = IdGenerater.GenerateId();
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			this.ModelScene.Dispose();
		}
	}
}