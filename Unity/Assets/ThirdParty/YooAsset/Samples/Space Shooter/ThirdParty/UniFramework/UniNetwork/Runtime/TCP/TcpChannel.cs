using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UniFramework.Network
{
	internal class TcpChannel : IDisposable
	{
		private readonly SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
		private readonly SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

		private readonly Queue<INetPackage> _sendQueue = new Queue<INetPackage>(10000);
		private readonly Queue<INetPackage> _receiveQueue = new Queue<INetPackage>(10000);
		private readonly List<INetPackage> _decodeTempList = new List<INetPackage>(100);

		private byte[] _receiveBuffer;
		private RingBuffer _encodeBuffer;
		private RingBuffer _decodeBuffer;
		private int _packageBodyMaxSize;
		private INetPackageEncoder _packageEncoder;
		private INetPackageDecoder _packageDecoder;
		private bool _isSending = false;
		private bool _isReceiving = false;

		/// <summary>
		/// 通信Socket
		/// </summary>
		private Socket _socket;

		/// <summary>
		/// 同步上下文
		/// </summary>
		private ThreadSyncContext _context;

		/// <summary>
		/// 初始化频道
		/// </summary>
		internal void InitChannel(ThreadSyncContext context, Socket socket, int packageBodyMaxSize, INetPackageEncoder encoder, INetPackageDecoder decoder)
		{
			if (packageBodyMaxSize <= 0)
				throw new System.ArgumentException($"PackageMaxSize is invalid : {packageBodyMaxSize}");

			_context = context;
			_socket = socket;
			_socket.NoDelay = true;

			// 创建编码解码器	
			_packageBodyMaxSize = packageBodyMaxSize;
			_packageEncoder = encoder;
			_packageEncoder.RigistHandleErrorCallback(HandleError);
			_packageDecoder = decoder;
			_packageDecoder.RigistHandleErrorCallback(HandleError);

			// 创建字节缓冲类
			// 注意：字节缓冲区长度，推荐4倍最大包体长度
			int encoderPackageMaxSize = packageBodyMaxSize + _packageEncoder.GetPackageHeaderSize();
			int decoderPakcageMaxSize = packageBodyMaxSize + _packageDecoder.GetPackageHeaderSize();
			_encodeBuffer = new RingBuffer(encoderPackageMaxSize * 4);
			_decodeBuffer = new RingBuffer(decoderPakcageMaxSize * 4);
			_receiveBuffer = new byte[decoderPakcageMaxSize];

			// 创建IOCP接收类
			_receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
			_receiveArgs.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);

			// 创建IOCP发送类
			_sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
			_sendArgs.SetBuffer(_encodeBuffer.GetBuffer(), 0, _encodeBuffer.Capacity);
		}

		/// <summary>
		/// 检测Socket是否已连接
		/// </summary>
		public bool IsConnected()
		{
			if (_socket == null)
				return false;
			return _socket.Connected;
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			try
			{
				if (_socket != null)
					_socket.Shutdown(SocketShutdown.Both);

				_receiveArgs.Dispose();
				_sendArgs.Dispose();

				_sendQueue.Clear();
				_receiveQueue.Clear();
				_decodeTempList.Clear();

				_encodeBuffer.Clear();
				_decodeBuffer.Clear();

				_isSending = false;
				_isReceiving = false;
			}
			catch (Exception)
			{
				// throws if client process has already closed, so it is not necessary to catch.
			}
			finally
			{
				if (_socket != null)
				{
					_socket.Close();
					_socket = null;
				}
			}
		}

		/// <summary>
		/// 主线程内更新
		/// </summary>
		public void Update()
		{
			if (_socket == null || _socket.Connected == false)
				return;

			// 接收数据
			UpdateReceiving();

			// 发送数据
			UpdateSending();
		}
		private void UpdateReceiving()
		{
			if (_isReceiving == false)
			{
				_isReceiving = true;

				// 请求操作
				bool willRaiseEvent = _socket.ReceiveAsync(_receiveArgs);
				if (!willRaiseEvent)
				{
					ProcessReceive(_receiveArgs);
				}
			}
		}
		private void UpdateSending()
		{
			if (_isSending == false && _sendQueue.Count > 0)
			{
				_isSending = true;

				// 清空缓存
				_encodeBuffer.Clear();

				// 合并数据一起发送
				while (_sendQueue.Count > 0)
				{
					// 如果不够写入一个最大的消息包
					int encoderPackageMaxSize = _packageBodyMaxSize + _packageEncoder.GetPackageHeaderSize();
					if (_encodeBuffer.WriteableBytes < encoderPackageMaxSize)
						break;

					// 数据压码
					INetPackage package = _sendQueue.Dequeue();
					_packageEncoder.Encode(_packageBodyMaxSize, _encodeBuffer, package);
				}

				// 请求操作
				_sendArgs.SetBuffer(0, _encodeBuffer.ReadableBytes);
				bool willRaiseEvent = _socket.SendAsync(_sendArgs);
				if (!willRaiseEvent)
				{
					ProcessSend(_sendArgs);
				}
			}
		}

		/// <summary>
		/// 发送网络包
		/// </summary>
		public void SendPackage(INetPackage package)
		{
			lock (_sendQueue)
			{
				_sendQueue.Enqueue(package);
			}
		}

		/// <summary>
		/// 获取网络包
		/// </summary>
		public INetPackage PickPackage()
		{
			INetPackage package = null;
			lock (_receiveQueue)
			{
				if (_receiveQueue.Count > 0)
					package = _receiveQueue.Dequeue();
			}
			return package;
		}


		/// <summary>
		/// This method is called whenever a receive or send operation is completed on a socket 
		/// </summary>
		private void IO_Completed(object sender, SocketAsyncEventArgs e)
		{
			// determine which type of operation just completed and call the associated handler
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Receive:
					_context.Post(ProcessReceive, e);
					break;
				case SocketAsyncOperation.Send:
					_context.Post(ProcessSend, e);
					break;
				default:
					throw new ArgumentException("The last operation completed on the socket was not a receive or send");
			}
		}

		/// <summary>
		/// 数据接收完成时
		/// </summary>
		private void ProcessReceive(object obj)
		{
			if (_socket == null)
				return;

			SocketAsyncEventArgs e = obj as SocketAsyncEventArgs;

			// check if the remote host closed the connection	
			if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
			{
				// 如果数据写穿
				if (_decodeBuffer.IsWriteable(e.BytesTransferred) == false)
				{
					HandleError(true, "The channel fatal error");
					return;
				}

				// 拷贝数据
				_decodeBuffer.WriteBytes(e.Buffer, 0, e.BytesTransferred);

				// 数据解码
				_decodeTempList.Clear();
				_packageDecoder.Decode(_packageBodyMaxSize, _decodeBuffer, _decodeTempList);
				lock (_receiveQueue)
				{
					for (int i = 0; i < _decodeTempList.Count; i++)
					{
						_receiveQueue.Enqueue(_decodeTempList[i]);
					}
				}

				// 为接收下一段数据，投递接收请求
				e.SetBuffer(0, _receiveBuffer.Length);
				bool willRaiseEvent = _socket.ReceiveAsync(e);
				if (!willRaiseEvent)
				{
					ProcessReceive(e);
				}
			}
			else
			{
				HandleError(true, $"ProcessReceive error : {e.SocketError}");
			}
		}

		/// <summary>
		/// 数据发送完成时
		/// </summary>
		private void ProcessSend(object obj)
		{
			if (_socket == null)
				return;

			SocketAsyncEventArgs e = obj as SocketAsyncEventArgs;
			if (e.SocketError == SocketError.Success)
			{
				_isSending = false;
			}
			else
			{
				HandleError(true, $"ProcessSend error : {e.SocketError}");
			}
		}

		/// <summary>
		/// 捕获异常错误
		/// </summary>
		private void HandleError(bool isDispose, string error)
		{
			UniLogger.Error(error);
			if (isDispose) Dispose();
		}
	}
}