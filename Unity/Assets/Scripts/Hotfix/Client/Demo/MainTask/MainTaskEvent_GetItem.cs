using System;

namespace ET.Client
{
    [Event(SceneType.Demo)]
    [FriendOf(typeof(MainTaskComponent))]
    public class MainTaskEvent_GetItem: AEvent<Scene, GetItem>
    {
        protected override async ETTask Run(Scene root, GetItem args)
        {
            var taskComponent = root.GetComponent<MainTaskComponent>();
            if (taskComponent.Table.Type == 1)
            {
                Log.Debug("修改前 taskComponent.Progress="+taskComponent.Progress);
                taskComponent.UpdateProgress(taskComponent.Progress + args.ItemValue);
                Log.Debug("修改后 taskComponent.Progress="+taskComponent.Progress);
            }
        }
    }
    
    public struct GetItem
    {
        public int ItemId;
        public int ItemValue;
    }
}