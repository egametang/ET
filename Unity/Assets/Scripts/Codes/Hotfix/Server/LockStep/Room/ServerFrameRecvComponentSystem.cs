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
                    Messages = new Dictionary<long, FrameMessage>()
                };
                self.FrameMessages.Add(oneFrameMessages.Frame, oneFrameMessages);
            }
            oneFrameMessages.Messages.Add(message.PlayerId, message);

            if (oneFrameMessages.Frame > self.NowFrame)
            {
                return null;   
            }

            if (oneFrameMessages.Messages.Count != LSConstValue.MatchCount)
            {
                return null;
            }
            self.FrameMessages.Remove(oneFrameMessages.Frame);
            ++self.NowFrame;
            return oneFrameMessages;
        }
    }
}