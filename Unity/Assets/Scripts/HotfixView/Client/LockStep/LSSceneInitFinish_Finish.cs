namespace ET.Client
{
    [Event(SceneType.LockStep)]
    public class LSSceneInitFinish_Finish: AEvent<Scene, EventType.LSSceneInitFinish>
    {
        protected override async ETTask Run(Scene clientScene, EventType.LSSceneInitFinish args)
        {
            Room room = clientScene.GetComponent<Room>();
            
            room.AddComponent<LSCameraComponent>();
            
            room.AddComponent<LSOperaComponent>();
            
            await UIHelper.Remove(clientScene, UIType.UILSLobby);
            await ETTask.CompletedTask;
        }
    }
}