#if DEBUG && !UNITY_WP_8_1 && !UNITY_WSA_8_1
#if !WINRT || UNITY_EDITOR
using System;
using System.Net;
using System.Net.Sockets;

namespace FlyingWormConsole3.LiteNetLib
{
    public sealed class NetEndPoint
    {
        public string Host { get { return EndPoint.Address.ToString(); } }
        public int Port { get { return EndPoint.Port; } }

        internal readonly IPEndPoint EndPoint;

        internal NetEndPoint(IPEndPoint ipEndPoint)
        {
            EndPoint = ipEndPoint;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NetEndPoint))
            {
                return false;
            }
            return EndPoint.Equals(((NetEndPoint)obj).EndPoint);
        }

        public override string ToString()
        {
            return EndPoint.ToString();
        }

        public override int GetHashCode()
        {
            return EndPoint.GetHashCode();
        }

        public NetEndPoint(string hostStr, int port)
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(hostStr, out ipAddress))
            {
                if (Socket.OSSupportsIPv6)
                {
                    if (hostStr == "localhost")
                    {
                        ipAddress = IPAddress.IPv6Loopback;
                    }
                    else
                    {
                        ipAddress = ResolveAddress(hostStr, AddressFamily.InterNetworkV6);
                    }
                }
                if (ipAddress == null)
                {
                    ipAddress = ResolveAddress(hostStr, AddressFamily.InterNetwork);
                }
            }
            if (ipAddress == null)
            {
                throw new Exception("Invalid address: " + hostStr);
            }
            EndPoint = new IPEndPoint(ipAddress, port);
        }

        private IPAddress ResolveAddress(string hostStr, AddressFamily addressFamily)
        {
#if NETCORE
            var hostTask = Dns.GetHostEntryAsync(hostStr);
            hostTask.Wait();
            var host = hostTask.Result;
#else
            var host = Dns.GetHostEntry(hostStr);
#endif
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == addressFamily)
                {
                    return ip;
                }
            }
            return null;
        }

        internal long GetId()
        {
            byte[] addr = EndPoint.Address.GetAddressBytes();
            long id = 0;

            if (addr.Length == 4) //IPv4
            {
                id = addr[0];
                id |= (long)addr[1] << 8;
                id |= (long)addr[2] << 16;
                id |= (long)addr[3] << 24;
                id |= (long)EndPoint.Port << 32;
            }
            else if (addr.Length == 16) //IPv6
            {
                id = addr[0] ^ addr[8];
                id |= (long)(addr[1] ^ addr[9]) << 8;
                id |= (long)(addr[2] ^ addr[10]) << 16;


                id |= (long)(addr[3] ^ addr[11]) << 24;
                id |= (long)(addr[4] ^ addr[12]) << 32;
                id |= (long)(addr[5] ^ addr[13]) << 40;
                id |= (long)(addr[6] ^ addr[14]) << 48;
                id |= (long)(Port ^ addr[7] ^ addr[15]) << 56;
            }

            return id;
        }
    }
}
#else
using System;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace FlyingWormConsole3.LiteNetLib
{
    public sealed class NetEndPoint
    {
        public string Host { get { return HostName.DisplayName; } }
        public int Port { get; private set; }
        internal readonly HostName HostName;
        internal readonly string PortStr;

        internal NetEndPoint(int port)
        {
            HostName = null;
            PortStr = port.ToString();
            Port = port;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NetEndPoint))
            {
                return false;
            }
            NetEndPoint other = (NetEndPoint) obj;
            return HostName.IsEqual(other.HostName) && PortStr.Equals(other.PortStr);
        }

        public override int GetHashCode()
        {
            return HostName.CanonicalName.GetHashCode() ^ PortStr.GetHashCode();
        }

        internal long GetId()
        {
            //Check locals
            if (HostName == null)
            {
                return ParseIpToId("0.0.0.0");
            }

            if (HostName.DisplayName == "localhost")
            {
                return ParseIpToId("127.0.0.1");
            }

            //Check remote
            string hostIp = string.Empty;
            var task = DatagramSocket.GetEndpointPairsAsync(HostName, "0").AsTask();
            task.Wait();

            //IPv4
            foreach (var endpointPair in task.Result)
            {
                hostIp = endpointPair.RemoteHostName.CanonicalName;
                if (endpointPair.RemoteHostName.Type == HostNameType.Ipv4)
                {
                    return ParseIpToId(hostIp);
                }
            }

            //Else
            return hostIp.GetHashCode() ^ Port;
        }

        private long ParseIpToId(string hostIp)
        {
            long id = 0;
            string[] ip = hostIp.Split('.');
            id |= long.Parse(ip[0]);
            id |= long.Parse(ip[1]) << 8;
            id |= long.Parse(ip[2]) << 16;
            id |= long.Parse(ip[3]) << 24;
            id |= (long)Port << 32;
            return id;
        }

        public override string ToString()
        {
            return HostName.CanonicalName + ":" + PortStr;
        }

        public NetEndPoint(string hostName, int port)
        {
            var task = DatagramSocket.GetEndpointPairsAsync(new HostName(hostName), port.ToString()).AsTask();
            task.Wait();
            HostName = task.Result[0].RemoteHostName;
            Port = port;
            PortStr = port.ToString();
        }

        internal NetEndPoint(HostName hostName, string port)
        {
            HostName = hostName;
            Port = int.Parse(port);
            PortStr = port;
        }
    }
}
#endif
#endif
