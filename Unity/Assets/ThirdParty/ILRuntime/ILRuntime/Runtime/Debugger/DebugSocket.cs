using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace ILRuntime.Runtime.Debugger
{
    public class DebugSocket
    {
        private Socket _socket = null;
        private bool _ready = false;
        bool connectFailed = false;
        private const int MAX_BUFF_SIZE = 256 * 1024;
        private const int HEAD_SIZE = 8;
        private byte[] _headBuffer = new byte[HEAD_SIZE];
        private byte[] _sendBuffer = new byte[64 * 1024];
        //private MemoryPoolSafe<Package> _packagePool = new MemoryPoolSafe<Package>();
        //private Package _currPackage = null;
        private System.IO.MemoryStream _sendStream = null;
        BinaryWriter bw;
        const int RECV_BUFFER_SIZE = 1024;
        private MemoryStream recvBuffer = new MemoryStream();
        private int lastMsgLength = -1;

        private byte[] socketAsyncBuffer = new byte[RECV_BUFFER_SIZE];
        private SocketAsyncEventArgs saeArgs;
        private object socketLockObj = new object();
        private byte[] _sendHeaderBuffer = new byte[HEAD_SIZE];

        public bool Disconnected { get { return _socket == null || !_socket.Connected; } }
        public Action OnConnect { get; set; }

        public Action OnConnectFailed { get; set; }
        public Action OnClose { get; set; }

        public Action<DebugMessageType, byte[]> OnReciveMessage { get; set; }


        public DebugSocket()
        {
            _sendStream = new System.IO.MemoryStream(_sendBuffer);
            bw = new BinaryWriter(_sendStream);
        }
        public DebugSocket(Socket _socket)
            : this()
        {
            this._socket = _socket;
            BeginReceive();
            _ready = true;
        }
        public void Connect(string ip, int port)
        {
            Close();
            Socket socket;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.BeginConnect(ip, port, new AsyncCallback(onConnected), this);
            _socket = socket;
            _ready = false;
        }

        private void AsyncRecv_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                try
                {
                    ReceivePayload(e.Buffer, e.BytesTransferred);
                }
                catch (Exception ex)
                {
                    Close();
                    return;
                }
            }
            else
            {
                Close();
                return;
            }

            try
            {
                //继续接受数据
                if (!_socket.ReceiveAsync(saeArgs))
                {
                    AsyncRecv_Completed(null, saeArgs);
                }
            }
            catch (Exception ex)
            {
                Close();
                throw ex;
            }
        }

        private void ReceivePayload(byte[] data, int length)
        {
            if (_socket == null)
                return;
            if (!_socket.Connected)
            {
                Close();
                return;
            }
            //接受数据并拼接成message
            byte[] msgBuff;
            //写入缓存
            recvBuffer.Position = recvBuffer.Length;
            recvBuffer.Write(data, 0, length);
            //如果长度有错，返回
            if (lastMsgLength < 0 && recvBuffer.Length < 4)
            {
                msgBuff = null;
                return;
            }

            recvBuffer.Position = 0;
            BinaryReader br = new BinaryReader(recvBuffer);
            //读取消息长度
            if (lastMsgLength < 0)
            {
                lastMsgLength = br.ReadInt32() - 4;
                if (lastMsgLength > MAX_BUFF_SIZE)
                {
                    Close();
                    throw new Exception("Too long package length!");
                }
            }
            int remaining = (int)(recvBuffer.Length - recvBuffer.Position);
            //消息已经完整
            while (remaining >= lastMsgLength && lastMsgLength > 0)
            {

                //读取一条消息
                int type = br.ReadInt32();
                msgBuff = br.ReadBytes(lastMsgLength - 4);

                try
                {
                    if (OnReciveMessage != null)
                        OnReciveMessage((DebugMessageType)type, msgBuff);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                lastMsgLength = -1;
                remaining = (int)(recvBuffer.Length - recvBuffer.Position);
                //保留剩余数据
                if (remaining >= 4)
                {
                    lastMsgLength = br.ReadInt32() - 4;
                    remaining -= 4;
                    if (lastMsgLength > MAX_BUFF_SIZE)
                    {
                        Close();
                        throw new Exception("Too long package length!");
                    }
                }
            }

            remaining = (int)(recvBuffer.Length - recvBuffer.Position);
            if (remaining > 0)
            {
                byte[] buffer = recvBuffer.GetBuffer();
                Array.Copy(buffer, recvBuffer.Position, buffer, 0, remaining);
            }
            recvBuffer.Position = 0;
            recvBuffer.SetLength(remaining);
        }
        private void onConnected(IAsyncResult result)
        {
            if (_socket.Connected)
            {
                _socket.EndConnect(result);

                BeginReceive();
                if (OnConnect != null)
                    OnConnect();
                //ReceiveOnce();
            }
            else
            {
                if (OnConnectFailed != null)
                    OnConnectFailed();
            }
        }

        void BeginReceive()
        {
            saeArgs = new SocketAsyncEventArgs();
            saeArgs.Completed += AsyncRecv_Completed;
            saeArgs.SetBuffer(socketAsyncBuffer, 0, socketAsyncBuffer.Length);
            _socket.ReceiveAsync(saeArgs);
            _ready = true;
        }

        //len type msg
        public void Send(DebugMessageType type, byte[] buffer, int len)
        {
            if (!_ready)
                return;

            //timeStamp = UnityEngine.Time.realtimeSinceStartup;
            _sendStream.Position = 0;
            bw.Write(len + HEAD_SIZE);
            bw.Write((int)type);
            bw.Write(buffer, 0, len);
            int totalLen = (int)_sendStream.Position;

            RawSend(_socket, _sendBuffer, totalLen);
            //_socket.Send(_sendBuffer, len, SocketFlags.None);
        }

        private void RawSend(Socket sock, byte[] buf, int end)
        {
            if (sock == null)
                return;
            if (end < 0)
                end = buf.Length;
            sock.Send(buf, end, SocketFlags.None);
        }

        public void Close()
        {
            if (_socket == null || !_ready)
                return;
            if (saeArgs != null)
                saeArgs.Dispose();
            _socket.Close();
            _socket = null;
            _ready = false;
            if (OnClose != null)
            {
                OnClose();
            }
        }
    }
}
