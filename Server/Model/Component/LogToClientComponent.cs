using Base;

namespace Model
{
	[ObjectEvent]
	public class LogToClientComponentEvent : ObjectEvent<LogToClientComponent>, IAwake
	{
		public void Awake()
		{
			this.GetValue().Awake();
		}
	}

	public class LogToClientComponent : Component
	{
		private string appType;

		public void Awake()
		{
			this.appType = Game.Scene.GetComponent<OptionsComponent>().Options.AppType;
			Log.Callback.Add(this.Id, this.LogToClient);
		}

		private void LogToClient(LogType type, string message)
		{
			if (this.Owner.Id == 0)
			{
				return;
			}
			this.GetComponent<MessageComponent>().Send(new S2C_ServerLog { AppType = this.appType, Type = type, Log = message });
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			long id = this.Id;

			base.Dispose();

			Log.Callback.Remove(id);
		}
	}
}
