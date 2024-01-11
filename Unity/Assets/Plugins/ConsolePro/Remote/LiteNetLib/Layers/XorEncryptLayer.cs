using System;
using System.Net;
using System.Text;

namespace FlyingWormConsole3.LiteNetLib.Layers
{
    public class XorEncryptLayer : PacketLayerBase
    {
        private byte[] _byteKey;

        public XorEncryptLayer() : base(0)
        {

        }

        public XorEncryptLayer(byte[] key) : this()
        {
            SetKey(key);
        }

        public XorEncryptLayer(string key) : this()
        {
            SetKey(key);
        }

        public void SetKey(string key)
        {
            _byteKey = Encoding.UTF8.GetBytes(key);
        }

        public void SetKey(byte[] key)
        {
            if (_byteKey == null || _byteKey.Length != key.Length)
                _byteKey = new byte[key.Length];
            Buffer.BlockCopy(key, 0, _byteKey, 0, key.Length);
        }

        public override void ProcessInboundPacket(IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
        {
            if (_byteKey == null)
                return;
            var cur = offset;
            for (var i = 0; i < length; i++, cur++)
            {
                data[cur] = (byte)(data[cur] ^ _byteKey[i % _byteKey.Length]);
            }
        }

        public override void ProcessOutBoundPacket(IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
        {
            if (_byteKey == null)
                return;
            var cur = offset;
            for (var i = 0; i < length; i++, cur++)
            {
                data[cur] = (byte)(data[cur] ^ _byteKey[i % _byteKey.Length]);
            }
        }
    }
}
