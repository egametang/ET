using ETModel;

namespace ETHotfix
{
    [Event(EventIdType.AfterScenesAdd)]
    public class AfterScenesAdd_CreateScene: AEvent
    {
        public override void Run()
        {
            RunInner().Coroutine();
        }

        public async ETVoid RunInner()
        {
            foreach (StartConfig startConfig in StartConfigComponent.Instance.StartConfig.List)
            {
                SceneConfig sceneConfig = startConfig.GetComponent<SceneConfig>();
                await SceneFactory.Create(Game.Scene, startConfig.Id, sceneConfig.Name, sceneConfig.SceneType);    
            }
        }
    }
}