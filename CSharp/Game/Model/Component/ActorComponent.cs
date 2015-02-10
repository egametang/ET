using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Base;
using Common.Event;
using Common.Logger;

namespace Model
{
	public class ActorComponent : Component<Unit>
	{
		private readonly Queue<Env> msgEnvQueue = new Queue<Env>();

		public Action msgAction = () => {};

		public Env Env { get; private set; }

		public async void Run()
		{
			while (true)
			{
				try
				{
					Env env = await this.Get();
					this.Env = env;
					var message = env.Get<byte[]>(EnvKey.Message);
					int opcode = BitConverter.ToUInt16(message, 0);
					await World.Instance.GetComponent<EventComponent<MessageAttribute>>().RunAsync(opcode, env);
				}
				catch (Exception e)
				{
					Log.Trace(string.Format(e.ToString()));
				}
			}
		}

		public void Add(Env msgEnv)
		{
			this.msgEnvQueue.Enqueue(msgEnv);
			msgAction();
		}

		private Task<Env> Get()
		{
			var tcs = new TaskCompletionSource<Env>();
			if (this.msgEnvQueue.Count > 0)
			{
				Env env = this.msgEnvQueue.Dequeue();
				tcs.SetResult(env);
			}
			else
			{
				msgAction = () =>
				{
					msgAction = () => { };
					Env msg = this.msgEnvQueue.Dequeue();
					tcs.SetResult(msg);
				};
			}
			return tcs.Task;
		}
	}
}
