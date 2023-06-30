using System;
using UnityEngine.SceneManagement;

namespace ET.Client
{
    [Event(SceneType.Demo)]
    public class SceneChangeStart_AddComponent: AEvent<Scene, EventType.SceneChangeStart>
    {
        protected override async ETTask Run(Scene root, EventType.SceneChangeStart args)
        {
            try
            {
                Scene currentScene = root.CurrentScene();

                ResourcesComponent resourcesComponent = root.GetComponent<ResourcesComponent>();
            
                // 加载场景资源
                await resourcesComponent.LoadBundleAsync($"{currentScene.Name}.unity3d");
                // 切换到map场景

                await SceneManager.LoadSceneAsync(currentScene.Name);
			

                currentScene.AddComponent<OperaComponent>();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

        }
    }
}