namespace ET.Client
{
    [Event(SceneType.Client)]
    public class LockStepSceneInitFinish_Finish: AEvent<Scene, EventType.LockStepSceneInitFinish>
    {
        protected override async ETTask Run(Scene clientScene, EventType.LockStepSceneInitFinish args)
        {
            BattleScene battleScene = clientScene.GetComponent<BattleScene>();
            
            battleScene.AddComponent<CameraComponent>();
            
            battleScene.AddComponent<LockStepOperaComponent>();
            await ETTask.CompletedTask;
        }
    }
}