using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Base;
using Common.Logger;

namespace Model
{
	public class ActorComponent: Component<Unit>
	{
		private readonly Queue<Env> msgEnvQueue = new Queue<Env>();

		private Action msgAction = () => { };

		private Env Env { get; set; }

		public ActorComponent()
		{
			this.Start();
		}

		private async void Start()
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
			this.msgAction();
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
				this.msgAction = () =>
				{
					this.msgAction = () => { };
					Env msg = this.msgEnvQueue.Dequeue();
					tcs.SetResult(msg);
				};
			}
			return tcs.Task;
		}
	}
}