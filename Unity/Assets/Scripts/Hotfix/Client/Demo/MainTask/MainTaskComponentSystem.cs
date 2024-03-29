using System.Collections.Generic;

namespace ET.Client
{
    [EntitySystemOf(typeof(MainTaskComponent))]
    [FriendOf(typeof(MainTaskComponent))]
    public static partial  class MainTaskComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.MainTaskComponent self)
        {

        }
        
        public static void SetTaskId(this ET.Client.MainTaskComponent self,int mainId,int subId)
        {
            self.MainId = mainId;
            self.SubId = subId;
        }
        
        public static void SetType(this ET.Client.MainTaskComponent self,int type)
        {
            self.Type = type;
        }
        
        public static void SetNeedProgress(this ET.Client.MainTaskComponent self,int needProgress)
        {
            self.NeedProgress = needProgress;
        }

        public static void SetProgress(this ET.Client.MainTaskComponent self,int progress)
        {
            self.Progress = progress;
            self.CheckProgress();
        }

        private static void CheckProgress(this ET.Client.MainTaskComponent self)
        {
            if (self.Progress >= self.NeedProgress)
            {
                if (self.IsAutoComplete == 1)
                {
                    EventSystem.Instance.Publish(self.Root(), new MainTaskUpdate());
                    self.SetTaskId(self.MainId, self.SubId);
                    return;
                }
                //标记为已完成
                self.Status = 1;
                EventSystem.Instance.Publish(self.Root(), new MainTaskComplete());
            }
        }

        [EntitySystem]
        private static void Destroy(this ET.Client.MainTaskComponent self)
        {

        }
    }
    
    public struct MainTaskUpdate
    {
    }
    
    public struct MainTaskComplete
    {
    }
}