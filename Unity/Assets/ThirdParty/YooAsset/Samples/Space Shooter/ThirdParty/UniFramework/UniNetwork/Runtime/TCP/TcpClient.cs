using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UniFramework.Network
{
	public class TcpClient : IDisposable
	{
		private class UserToken
		{
			public System.Action<SocketError> Callback;
		}

		private TcpChannel _channel;
		private readonly int _packageBodyMaxSize;
		private readonly INetPackageEncoder _encoder;
		private readonly INetPackageDecoder _decoder;
		private readonly ThreadSyncContext _syncContext;

		private TcpClient()
		{
		}
		internal TcpClient(int packageBodyMaxSize, INetPackageEncoder encoder, INetPackageDecoder decoder)
		{
			_packageBodyMaxSize = packageBodyMaxSize;
			_encoder = encoder;
			_decoder = decoder;
			_syncContext = new ThreadSyncContext();
		}

		/// <summary>
		/// 更新网络
		/// </summary>
		internal void Update()
		{
			_syncContext.Update();

			if (_channel != null)
				_channel.Update();
		}

		/// <summary>
		/// 销毁网络
		/// </summary>
		internal void Destroy()
		{
			Dispose();
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			if (_channel != null)
			{
				_channel.Dispose();
				_channel = null;
			}
		}

		/// <summary>
		/// 发送网络包
		/// </summary>
		public void SendPackage(INetPackage package)
		{
			if (_channel != null)
				_channel.SendPackage(package);
		}

		/// <summary>
		/// 获取网络包
		/// </summary>
		public INetPackage PickPackage()
		{
			if (_channel == null)
				return null;

			return _channel.PickPackage();
		}

		/// <summary>
		/// 检测Socket是否已连接
		/// </summary>
		public bool IsConnected()
		{
			if (_channel == null)
				return false;

			return _channel.IsConnected();
		}


		/// <summary>
		/// 异步连接
		/// </summary>
		/// <param name="remote">IP终端</param>
		/// <param name="callback">连接回调</param>
		public void ConnectAsync(IPEndPoint remote, System.Action<SocketError> callback)
		{
			UserToken token = new UserToken()
			{
				Callback = callback,
			};

			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			args.RemoteEndPoint = remote;
			args.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
			args.UserToken = token;

			Socket clientSock = new Socket(remote.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			bool willRaiseEvent = clientSock.ConnectAsync(args);
			if (!willRaiseEvent)
			{
				ProcessConnected(args);
			}
		}

		/// <summary>
		/// 处理连接请求
		/// </summary>
		private void ProcessConnected(object obj)
		{
			SocketAsyncEventArgs e = obj as SocketAsyncEventArgs;
			UserToken token = (UserToken)e.UserToken;
			if (e.SocketError == SocketError.Success)
			{
				if (_channel != null)
					throw new Exception("TcpClient channel is created.");

				// 创建频道
				_channel = new TcpChannel();
				_channel.InitChannel(_syncContext, e.ConnectSocket, _packageBodyMaxSize, _encoder, _decoder);
			}
			else
			{
				UniLogger.Error($"Network connecte error : {e.SocketError}");
			}

			// 回调函数		
			if (token.Callback != null)
				token.Callback.Invoke(e.SocketError);
		}

		private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Connect:
					_syncContext.Post(ProcessConnected, e);
					break;
				default:
					throw new ArgumentException("The last operation completed on the socket was not a connect");
			}
		}
	}
}
