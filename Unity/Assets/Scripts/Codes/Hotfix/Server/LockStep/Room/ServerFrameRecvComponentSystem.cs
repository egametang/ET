using System;
using System.Collections.Generic;

namespace ET.Server
{
    [FriendOf(typeof(ServerFrameRecvComponent))]
    public static class ServerFrameRecvComponentSystem
    {
        public static OneFrameMessages Add(this ServerFrameRecvComponent self , FrameMessage message)
        {
            if (message.Frame < self.NowFrame)
            {
                return null;   
            }
            
            OneFrameMessages oneFrameMessages;
            if (!self.FrameMessages.TryGetValue(message.Frame, out oneFrameMessages))
            {
                oneFrameMessages = new OneFrameMessages
                {
                    Frame = message.Frame,
                };
                self.FrameMessages.Add(oneFrameMessages.Frame, oneFrameMessages);
            }

            oneFrameMessages.InputInfos[message.PlayerId] = message.InputInfo;

            if (oneFrameMessages.Frame > self.NowFrame)
            {
                return null;   
            }

            if (oneFrameMessages.InputInfos.Count != LSConstValue.MatchCount)
            {
                return null;
            }
            self.FrameMessages.Remove(oneFrameMessages.Frame);
            ++self.NowFrame;
            return oneFrameMessages;
        }
    }
}