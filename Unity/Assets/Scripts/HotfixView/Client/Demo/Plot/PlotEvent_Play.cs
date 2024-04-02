using System;
using NativeCollection.UnsafeType;

namespace ET.Client
{
    [Event(SceneType.Demo)]
    [FriendOf(typeof(PlotComponent))]
    public class PlotEvent_Play: AEvent<Scene, Chat>
    {
        protected override async ETTask Run(Scene root, Chat chats)
        {
            var plotComponent = root.GetComponent<PlotComponent>();
            //await UIHelper.ShowChat(root,chats.TableId);
        }
    }
    
    public struct Chat
    {
        public int TableId;
    }
}