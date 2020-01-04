namespace ETModel
{
	public sealed class Scene: Entity
	{
		public SceneType SceneType { get; set; }
		
		public string Name { get; set; }

		public Scene Get(long id)
		{
			return (Scene)this.Children[id];
		}

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
			}
		}
	}
}