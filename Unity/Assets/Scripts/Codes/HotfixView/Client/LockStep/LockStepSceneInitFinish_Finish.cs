namespace ET.Client
{
    [Event(SceneType.Client)]
    public class LockStepSceneInitFinish_Finish: AEvent<Scene, EventType.LockStepSceneInitFinish>
    {
        protected override async ETTask Run(Scene clientScene, EventType.LockStepSceneInitFinish args)
        {
            Room room = clientScene.GetComponent<Room>();
            
            room.AddComponent<CameraComponent>();
            
            room.AddComponent<LSOperaComponent>();
            await ETTask.CompletedTask;
        }
    }
}