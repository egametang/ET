#if DEBUG && !UNITY_WP_8_1 && !UNITY_WSA_8_1
#define UNITY

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if WINRT && !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Connectivity;
#else
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
#endif

namespace FlyingWormConsole3.LiteNetLib
{
#if WINRT && !UNITY_EDITOR
    public enum ConsoleColor
    {
        Gray,
        Yellow,
        Cyan,
        DarkCyan,
        DarkGreen,
        Blue,
        DarkRed,
        Red,
        Green,
        DarkYellow
    }
#endif

    [Flags]
    public enum LocalAddrType
    {
        IPv4 = 1,
        IPv6 = 2,
        All = 3
    }

    public static class NetUtils
    {
        internal static int RelativeSequenceNumber(int number, int expected)
        {
            return (number - expected + NetConstants.MaxSequence + NetConstants.HalfMaxSequence) % NetConstants.MaxSequence - NetConstants.HalfMaxSequence;
        }

        internal static int GetDividedPacketsCount(int size, int mtu)
        {
            return (size / mtu) + (size % mtu == 0 ? 0 : 1);
        }

        public static void PrintInterfaceInfos()
        {
#if !WINRT || UNITY_EDITOR
            DebugWriteForce(ConsoleColor.Green, "IPv6Support: {0}", Socket.OSSupportsIPv6);
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork ||
                            ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            DebugWriteForce(
                                ConsoleColor.Green,
                                "Interface: {0}, Type: {1}, Ip: {2}, OpStatus: {3}",
                                ni.Name,
                                ni.NetworkInterfaceType.ToString(),
                                ip.Address.ToString(),
                                ni.OperationalStatus.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DebugWriteForce(ConsoleColor.Red, "Error while getting interface infos: {0}", e.ToString());
            }
#endif
        }

        public static void GetLocalIpList(List<string> targetList, LocalAddrType addrType)
        {
            bool ipv4 = (addrType & LocalAddrType.IPv4) == LocalAddrType.IPv4;
            bool ipv6 = (addrType & LocalAddrType.IPv6) == LocalAddrType.IPv6;
#if WINRT && !UNITY_EDITOR
            foreach (HostName localHostName in NetworkInformation.GetHostNames())
            {
                if (localHostName.IPInformation != null && 
                    ((ipv4 && localHostName.Type == HostNameType.Ipv4) ||
                     (ipv6 && localHostName.Type == HostNameType.Ipv6)))
                {
                    targetList.Add(localHostName.ToString());
                }
            }
#else
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    //Skip loopback
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;

                    var ipProps = ni.GetIPProperties();

                    //Skip address without gateway
                    if (ipProps.GatewayAddresses.Count == 0)
                        continue;

                    foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
                    {
                        var address = ip.Address;
                        if ((ipv4 && address.AddressFamily == AddressFamily.InterNetwork) ||
                            (ipv6 && address.AddressFamily == AddressFamily.InterNetworkV6))
                            targetList.Add(address.ToString());
                    }
                }
            }
            catch
            {
                //ignored
            }

            //Fallback mode (unity android)
            if (targetList.Count == 0)
            {
#if NETCORE
                var hostTask = Dns.GetHostEntryAsync(Dns.GetHostName());
                hostTask.Wait();
                var host = hostTask.Result;
#else
                var host = Dns.GetHostEntry(Dns.GetHostName());
#endif
                foreach (IPAddress ip in host.AddressList)
                {
                    if((ipv4 && ip.AddressFamily == AddressFamily.InterNetwork) ||
                       (ipv6 && ip.AddressFamily == AddressFamily.InterNetworkV6))
                        targetList.Add(ip.ToString());
                }
            }
#endif
            if (targetList.Count == 0)
            {
                if(ipv4)
                    targetList.Add("127.0.0.1");
                if(ipv6)
                    targetList.Add("::1");
            }
        }

        private static readonly List<string> IpList = new List<string>();
        public static string GetLocalIp(LocalAddrType addrType)
        {
            lock (IpList)
            {
                IpList.Clear();
                GetLocalIpList(IpList, addrType);
                return IpList.Count == 0 ? string.Empty : IpList[0];
            }
        }

        private static readonly object DebugLogLock = new object();

        private static void DebugWriteLogic(ConsoleColor color, string str, params object[] args)
        {
            lock (DebugLogLock)
            {

                if (NetDebug.Logger == null)
                {
#if UNITY
#if !UNITY_4_0
                    UnityEngine.Debug.LogFormat(str, args);
#endif
#elif WINRT
                    Debug.WriteLine(str, args);
#else
                    Console.ForegroundColor = color;
                    Console.WriteLine(str, args);
                    Console.ForegroundColor = ConsoleColor.Gray;
#endif
                }
                else
                {
                    NetDebug.Logger.WriteNet(color, str, args);
                }
            }
        }

        [Conditional("DEBUG_MESSAGES")]
        internal static void DebugWrite(string str, params object[] args)
        {
            DebugWriteLogic(ConsoleColor.DarkGreen, str, args);
        }

        [Conditional("DEBUG_MESSAGES")]
        internal static void DebugWrite(ConsoleColor color, string str, params object[] args)
        {
            DebugWriteLogic(color, str, args);
        }

        [Conditional("DEBUG_MESSAGES"), Conditional("DEBUG")]
        internal static void DebugWriteForce(ConsoleColor color, string str, params object[] args)
        {
            DebugWriteLogic(color, str, args);
        }

        [Conditional("DEBUG_MESSAGES"), Conditional("DEBUG")]
        internal static void DebugWriteError(string str, params object[] args)
        {
            DebugWriteLogic(ConsoleColor.Red, str, args);
        }
    }
}
#endif
