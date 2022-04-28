﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace ET
{
    [FriendClass(typeof(NetThreadComponent))]
    [ObjectSystem]
    public class NetInnerComponentAwakeSystem: AwakeSystem<NetInnerComponent, int>
    {
        public override void Awake(NetInnerComponent self, int sessionStreamDispatcherType)
        {
            NetInnerComponent.Instance = self;
            self.SessionStreamDispatcherType = sessionStreamDispatcherType;
            
            self.Service = new TService(NetThreadComponent.Instance.ThreadSynchronizationContext, ServiceType.Inner);
            self.Service.ErrorCallback += self.OnError;
            self.Service.ReadCallback += self.OnRead;

            NetThreadComponent.Instance.Add(self.Service);
        }
    }

    [FriendClass(typeof(NetThreadComponent))]
    [ObjectSystem]
    public class NetInnerComponentAwake1System: AwakeSystem<NetInnerComponent, IPEndPoint, int>
    {
        public override void Awake(NetInnerComponent self, IPEndPoint address, int sessionStreamDispatcherType)
        {
            NetInnerComponent.Instance = self;
            self.SessionStreamDispatcherType = sessionStreamDispatcherType;

            self.Service = new TService(NetThreadComponent.Instance.ThreadSynchronizationContext, address, ServiceType.Inner);
            self.Service.ErrorCallback += self.OnError;
            self.Service.ReadCallback += self.OnRead;
            self.Service.AcceptCallback += self.OnAccept;

            NetThreadComponent.Instance.Add(self.Service);
        }
    }

    [ObjectSystem]
    public class NetInnerComponentDestroySystem: DestroySystem<NetInnerComponent>
    {
        public override void Destroy(NetInnerComponent self)
        {
            NetThreadComponent.Instance.Remove(self.Service);
            self.Service.Destroy();
        }
    }

    [FriendClass(typeof(NetInnerComponent))]
    public static class NetInnerComponentSystem
    {
        public static void OnRead(this NetInnerComponent self, long channelId, MemoryStream memoryStream)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.LastRecvTime = TimeHelper.ClientNow();
            SessionStreamDispatcher.Instance.Dispatch(self.SessionStreamDispatcherType, session, memoryStream);
        }

        public static void OnError(this NetInnerComponent self, long channelId, int error)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.Error = error;
            session.Dispose();
        }

        // 这个channelId是由CreateAcceptChannelId生成的
        public static void OnAccept(this NetInnerComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.Service);
            session.RemoteAddress = ipEndPoint;
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);
        }

        // 这个channelId是由CreateConnectChannelId生成的
        public static Session Create(this NetInnerComponent self, IPEndPoint ipEndPoint)
        {
            uint localConn = self.Service.CreateRandomLocalConn();
            long channelId = self.Service.CreateConnectChannelId(localConn);
            Session session = self.CreateInner(channelId, ipEndPoint);
            return session;
        }

        private static Session CreateInner(this NetInnerComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.Service);

            session.RemoteAddress = ipEndPoint;

            self.Service.GetOrCreate(channelId, ipEndPoint);

            //session.AddComponent<InnerPingComponent>();
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);

            return session;
        }

        // 内网actor session，channelId是进程号
        public static Session Get(this NetInnerComponent self, long channelId)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                IPEndPoint ipEndPoint = StartProcessConfigCategory.Instance.Get((int) channelId).InnerIPPort;
                session = self.CreateInner(channelId, ipEndPoint);
            }

            return session;
        }
    }
}