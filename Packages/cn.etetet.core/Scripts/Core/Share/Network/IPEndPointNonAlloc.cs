using System;
using System.Net;
using System.Net.Sockets;

namespace ET
{
    public static class EndPointHelper
    {
        public static IPEndPoint Clone(this EndPoint endPoint)
        {
            IPEndPoint ip = (IPEndPoint)endPoint;
            ip = new IPEndPoint(ip.Address, ip.Port);
            return ip;
        }
    }
}