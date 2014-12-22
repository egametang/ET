using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Base;
using MongoDB.Bson;

namespace TNet
{
	public class TServer: IDisposable
	{
		private IPoller poller = new TPoller();
		private TSocket acceptor;

		private readonly Dictionary<string, TSession> sessions = new Dictionary<string, TSession>();

		private readonly TimerManager timerManager = new TimerManager();
		
		public TServer(int port)
		{
			this.acceptor = new TSocket(poller);
			this.acceptor.Bind("127.0.0.1", port);
			this.acceptor.Listen(100);
		}

		public void Dispose()
		{
			if (this.poller == null)
			{
				return;
			}
			
			this.acceptor.Dispose();
			this.acceptor = null;
			this.poller = null;
		}

		public void Remove(string address)
		{
			TSession session;
			if (!this.sessions.TryGetValue(address, out session))
			{
				return;
			}
			if (session.SendTimer != ObjectId.Empty)
			{
				this.Timer.Remove(session.SendTimer);
			}

			if (session.RecvTimer != ObjectId.Empty)
			{
				this.Timer.Remove(session.RecvTimer);
			}
			this.sessions.Remove(address);
		}

		public void Push(Action action)
		{
			this.poller.Add(action);
		}

		public async Task<TSession> AcceptAsync()
		{
			TSocket newSocket = new TSocket(poller);
			await this.acceptor.AcceptAsync(newSocket);
			TSession session = new TSession(newSocket, this);
			sessions[newSocket.RemoteAddress] = session;
			return session;
		}

		public async Task<TSession> ConnectAsync(string host, int port)
		{
			TSocket newSocket = new TSocket(poller);
			await newSocket.ConnectAsync(host, port);
			TSession session = new TSession(newSocket, this);
			sessions[newSocket.RemoteAddress] = session;
			return session;
		}

		public TSession Get(string host, int port)
		{
			TSession session = null;
			this.sessions.TryGetValue(host + ":" + port, out session);
			return session;
		}

		public async Task<TSession> GetOrCreate(string host, int port)
		{
			TSession session = null;
			if (this.sessions.TryGetValue(host + ":" + port, out session))
			{
				return session;
			}
			return await ConnectAsync(host, port);
		}

		public void Start()
		{
			while (true)
			{
				poller.Run(1);
				this.timerManager.Refresh();
			}
		}

		public TimerManager Timer
		{
			get
			{
				return this.timerManager;
			}
		}
	}
}
