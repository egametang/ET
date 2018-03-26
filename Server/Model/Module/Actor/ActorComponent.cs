﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ETModel
{
	public struct ActorMessageInfo
	{
		public Session Session;
		public IActorMessage Message;
	}

	/// <summary>
	/// 挂上这个组件表示该Entity是一个Actor, 它会将Entity位置注册到Location Server, 接收的消息将会队列处理
	/// </summary>
	public class ActorComponent: Component
	{
		public IEntityActorHandler entityActorHandler;

		// 队列处理消息
		public Queue<ActorMessageInfo> queue;

		public TaskCompletionSource<ActorMessageInfo> tcs;

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			var t = this.tcs;
			this.tcs = null;
			t?.SetResult(new ActorMessageInfo());
		}
	}
}