using System;
using System.Threading.Tasks;

namespace Model
{
	public struct ActorMessageInfo
	{
		public Session Session;
		public ActorRequest Message;
	}

	/// <summary>
	/// 挂上这个组件表示该Entity是一个Actor, 它会将Entity位置注册到Location Server, 接收的消息将会队列处理
	/// </summary>
	public class ActorComponent: Component
	{
		public IEntityActorHandler entityActorHandler;

		public long actorId;

		// 队列处理消息
		public EQueue<ActorMessageInfo> queue;

		public TaskCompletionSource<ActorMessageInfo> tcs;

		public override void Dispose()
		{
			try
			{
				if (this.Id == 0)
				{
					return;
				}

				base.Dispose();

				Game.Scene.GetComponent<ActorManagerComponent>().Remove(actorId);
			}
			catch (Exception)
			{
				Log.Error($"unregister actor fail: {this.actorId}");
			}
		}
	}
}