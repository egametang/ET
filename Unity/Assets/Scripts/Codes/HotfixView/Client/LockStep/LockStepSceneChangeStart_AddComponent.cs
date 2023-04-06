using UnityEngine.SceneManagement;

namespace ET.Client
{
    [Event(SceneType.Client)]
    public class LockStepSceneChangeStart_AddComponent: AEvent<EventType.LockStepSceneChangeStart>
    {
        protected override async ETTask Run(Scene scene, EventType.LockStepSceneChangeStart args)
        {
            Scene currentScene = scene.CurrentScene();
            
            // 加载场景资源
            await ResourcesComponent.Instance.LoadBundleAsync($"{currentScene.Name}.unity3d");
            // 切换到map场景

            await SceneManager.LoadSceneAsync(currentScene.Name);
			

            currentScene.AddComponent<OperaComponent>();
        }
    }
}