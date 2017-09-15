#if DEBUG && !UNITY_WP_8_1 && !UNITY_WSA_8_1
﻿using System;
using System.Collections.Generic;

namespace FlyingWormConsole3.LiteNetLib
{
    internal sealed class NetPeerCollection
    {
        private readonly Dictionary<NetEndPoint, NetPeer> _peersDict;
        private readonly NetPeer[] _peersArray;
        private int _count;

        public int Count
        {
            get { return _count; }
        }

        public NetPeer this[int index]
        {
            get { return _peersArray[index]; }
        }

        public NetPeerCollection(int maxPeers)
        {
            _peersArray = new NetPeer[maxPeers];
            _peersDict = new Dictionary<NetEndPoint, NetPeer>();
        }

        public bool TryGetValue(NetEndPoint endPoint, out NetPeer peer)
        {
            return _peersDict.TryGetValue(endPoint, out peer);
        }

        public void Clear()
        {
            Array.Clear(_peersArray, 0, _count);
            _peersDict.Clear();
            _count = 0;
        }

        public void Add(NetEndPoint endPoint, NetPeer peer)
        {
            _peersArray[_count] = peer;
            _peersDict.Add(endPoint, peer);
            _count++;
        }

        public bool ContainsAddress(NetEndPoint endPoint)
        {
            return _peersDict.ContainsKey(endPoint);
        }

        public NetPeer[] ToArray()
        {
            NetPeer[] result = new NetPeer[_count];
            Array.Copy(_peersArray, 0, result, 0, _count);
            return result;
        }

        public void RemoveAt(int idx)
        {
            _peersDict.Remove(_peersArray[idx].EndPoint);
            _peersArray[idx] = _peersArray[_count - 1];
            _peersArray[_count - 1] = null;
            _count--;
        }

        public void Remove(NetEndPoint endPoint)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_peersArray[i].EndPoint.Equals(endPoint))
                {
                    RemoveAt(i);
                    break;
                }
            }
        }
    }
}
#endif
