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
		public void Awake()
		{
			Log.Callback.Add(this.Id, this.LogToClient);
		}

		private void LogToClient(LogType type, string message)
		{
			if (this.Owner.Id == 0)
			{
				return;
			}
			this.GetComponent<MessageComponent>().Send(new S2C_ServerLog { Type = type, Log = message });
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
