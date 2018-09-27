using System;
using System.IO;

namespace ETModel
{
	public class SessionCallbackComponent: Component
	{
		public Action<Session, byte, ushort, MemoryStream> MessageCallback;
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
