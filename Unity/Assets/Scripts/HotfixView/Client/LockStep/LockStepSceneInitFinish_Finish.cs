namespace ET.Client
{
    [Event(SceneType.LockStep)]
    public class LockStepSceneInitFinish_Finish: AEvent<Scene, EventType.LockStepSceneInitFinish>
    {
        protected override async ETTask Run(Scene clientScene, EventType.LockStepSceneInitFinish args)
        {
            Room room = clientScene.GetComponent<Room>();
            
            room.AddComponent<CameraComponent>();
            
            room.AddComponent<LSOperaComponent>();
            
            await UIHelper.Remove(clientScene, UIType.UILSLobby);
            await ETTask.CompletedTask;
        }
    }
}