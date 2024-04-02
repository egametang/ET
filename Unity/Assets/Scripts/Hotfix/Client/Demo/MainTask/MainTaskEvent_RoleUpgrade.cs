using System;

namespace ET.Client
{
    [Event(SceneType.Demo)]
    [FriendOf(typeof(MainTaskComponent))]
    public class MainTaskEvent_RoleUpgrade: AEvent<Scene, RoleUpgrade>
    {
        protected override async ETTask Run(Scene root, RoleUpgrade args)
        {
            var taskComponent = root.GetComponent<MainTaskComponent>();
            if (taskComponent.Table.Type == 1)
            {
                Log.Debug("修改前 taskComponent.Progress="+taskComponent.Progress);
                taskComponent.UpdateProgress(taskComponent.Progress + 1);
                Log.Debug("修改后 taskComponent.Progress="+taskComponent.Progress);
            }
        }
    }
    
    public struct RoleUpgrade
    {
        public int Level;
    }
}