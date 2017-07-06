using System;
using Base;

namespace Model
{
	[ObjectEvent]
	public class ActorComponentEvent : ObjectEvent<ActorComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}

	/// <summary>
	/// 挂上这个组件表示该Entity是一个Actor, 它会将Entity位置注册到Location Server
	/// </summary>
	public class ActorComponent: Component
	{
		private long actorId;

		public async void Awake()
		{
			try
			{
				this.actorId = this.Owner.Id;
				Game.Scene.GetComponent<ActorManagerComponent>().Add(this.Owner);
				await Game.Scene.GetComponent<LocationProxyComponent>().Add(this.actorId);
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		public override async void Dispose()
		{
			try
			{
				if (this.Id == 0)
				{
					return;
				}

				base.Dispose();

				Game.Scene.GetComponent<ActorManagerComponent>().Remove(actorId);

				await Game.Scene.GetComponent<LocationProxyComponent>().Remove(this.actorId);
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}
	}
}