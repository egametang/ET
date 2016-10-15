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
		private readonly Queue<byte[]> msgQueue = new Queue<byte[]>();

		private Action msgAction = () => { };
		
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
					byte[] messageBytes = await this.Get();
					Opcode opcode = (Opcode)BitConverter.ToUInt16(messageBytes, 0);
					await World.Instance.GetComponent<MessageComponent>().RunAsync(opcode, messageBytes);
				}
				catch (Exception e)
				{
					Log.Trace(e.ToString());
				}
			}
		}

		public void Add(byte[] msg)
		{
			this.msgQueue.Enqueue(msg);
			this.msgAction();
		}

		private Task<byte[]> Get()
		{
			var tcs = new TaskCompletionSource<byte[]>();
			if (this.msgQueue.Count > 0)
			{
				byte[] messageBytes = this.msgQueue.Dequeue();
				tcs.SetResult(messageBytes);
			}
			else
			{
				this.msgAction = () =>
				{
					this.msgAction = () => { };
					byte[] msg = this.msgQueue.Dequeue();
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