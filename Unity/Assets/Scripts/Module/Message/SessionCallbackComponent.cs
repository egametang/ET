using System;

namespace ETModel
{
	public class SessionCallbackComponent: Component
	{
		public Action<Session, byte, ushort, Packet> MessageCallback;
		public Action<Session> DisposeCallback;

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();

			this.DisposeCallback?.Invoke(this.GetParent<Session>());
		}
	}
}
