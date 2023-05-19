using UnityEngine.SceneManagement;

namespace ET.Client
{
    [Event(SceneType.Client)]
    public class SceneChangeStart_AddComponent: AEvent<Scene, EventType.SceneChangeStart>
    {
        protected override async ETTask Run(Scene scene, EventType.SceneChangeStart args)
        {
            Scene currentScene = scene.CurrentScene();

            await ResComponent.Instance.LoadSceneAsync(ResPathHelper.GetScenePath(currentScene.Name));
            
            // // 加载场景资源
            // await ResourcesComponent.Instance.LoadBundleAsync($"{currentScene.Name}.unity3d");
            // // 切换到map场景
            //
            // await SceneManager.LoadSceneAsync(currentScene.Name);
			

            currentScene.AddComponent<OperaComponent>();
        }
    }
}