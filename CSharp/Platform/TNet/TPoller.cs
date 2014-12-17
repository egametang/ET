using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TNet
{
	public class TPoller
	{
		private readonly BlockingCollection<TSocketState> blockingCollection = new BlockingCollection<TSocketState>();

		public HashSet<TSocket> CanWriteSocket = new HashSet<TSocket>();

		public void Add(TSocketState tSocketState)
		{
			this.blockingCollection.Add(tSocketState);
		}

		public void Dispose()
		{
		}

		public void RunOnce(int timeout)
		{
			foreach (TSocket socket in CanWriteSocket)
			{
				if (socket.IsSending)
				{
					continue;
				}
				socket.BeginSend();
			}
			this.CanWriteSocket.Clear();

			TSocketState socketState;
			if (!this.blockingCollection.TryTake(out socketState, timeout))
			{
				return;
			}

			var stateQueue = new Queue<TSocketState>();
			stateQueue.Enqueue(socketState);

			while (true)
			{
				if (!this.blockingCollection.TryTake(out socketState, 0))
				{
					break;
				}
				stateQueue.Enqueue(socketState);
			}

			while (stateQueue.Count > 0)
			{
				TSocketState state = stateQueue.Dequeue();
				state.Run();
			}
		}

		public void Run()
		{
			while (true)
			{
				this.RunOnce(1);
			}
		}
	}
}
