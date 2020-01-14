using ETModel;

namespace ETHotfix
{
	public sealed class Scene: Entity
	{
		public SceneType SceneType { get; set; }
		public string Name { get; set; }

		public new Entity Domain
		{
			get
			{
				return this.domain;
			}
			set
			{
				this.domain = value;
			}
		}

		public new Entity Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				this.parent = value;
				this.parent.Children.Add(this.Id, this);
#if !SERVER
				if (this.ViewGO != null && this.parent.ViewGO != null)
				{
					this.ViewGO.transform.SetParent(this.parent.ViewGO.transform, false);
				}
#endif
			}
		}
	}
}