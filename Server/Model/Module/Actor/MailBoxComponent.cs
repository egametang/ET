using System.Collections.Generic;
using System.Threading.Tasks;

namespace ETModel
{
	public struct ActorMessageInfo
	{
		public Session Session;
		public object Message;
	}

	/// <summary>
	/// 挂上这个组件表示该Entity是一个Actor,接收的消息将会队列处理
	/// </summary>
	public class MailBoxComponent: Component
	{
		// 拦截器类型，默认没有拦截器
		public string ActorInterceptType;

		// 队列处理消息
		public Queue<ActorMessageInfo> Queue = new Queue<ActorMessageInfo>();

		public ETTaskCompletionSource<ActorMessageInfo> Tcs;

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			var t = this.Tcs;
			this.Tcs = null;
			t?.SetResult(new ActorMessageInfo());
		}
	}
}