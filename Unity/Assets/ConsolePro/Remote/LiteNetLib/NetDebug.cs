#if DEBUG && !UNITY_WP_8_1 && !UNITY_WSA_8_1
﻿using System;

namespace FlyingWormConsole3.LiteNetLib
{
    public interface INetLogger
    {
        void WriteNet(ConsoleColor color, string str, params object[] args);
    }

    public static class NetDebug
    {
        public static INetLogger Logger = null;
    }
}
#endif
