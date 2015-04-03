using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Base;
using Common.Logger;
using MongoDB.Bson;

namespace Model
{
	public class Actor : Entity<Unit>, IDisposable
	{
		private readonly Queue<Env> msgEnvQueue = new Queue<Env>();

		private Action msgAction = () => { };

		private Env Env { get; set; }

		private bool isStop;

		public Actor(ObjectId id): base(id)
		{
			this.Start();
		}

		private async void Start()
		{
			while (!this.isStop)
			{
				try
				{
					Env env = await this.Get();
					this.Env = env;
					ushort opcode = env.Get<ushort>(EnvKey.Opcode);
					await World.Instance.GetComponent<MessageComponent>().RunAsync(opcode, env);
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

		public void Dispose()
		{
			this.isStop = true;
		}
	}
}