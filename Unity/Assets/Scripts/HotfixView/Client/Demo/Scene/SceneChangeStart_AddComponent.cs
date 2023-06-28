using UnityEngine.SceneManagement;

namespace ET.Client
{
    [Event(SceneType.Demo)]
    public class SceneChangeStart_AddComponent: AEvent<Fiber, EventType.SceneChangeStart>
    {
        protected override async ETTask Run(Fiber fiber, EventType.SceneChangeStart args)
        {
            Scene currentScene = fiber.CurrentScene();

            ResourcesComponent resourcesComponent = fiber.GetComponent<ResourcesComponent>();
            
            // 加载场景资源
            await resourcesComponent.LoadBundleAsync($"{currentScene.Name}.unity3d");
            // 切换到map场景

            await SceneManager.LoadSceneAsync(currentScene.Name);
			

            currentScene.AddComponent<OperaComponent>();
        }
    }
}