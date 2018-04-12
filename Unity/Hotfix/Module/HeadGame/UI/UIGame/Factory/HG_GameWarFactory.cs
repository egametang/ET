using System;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
    [UIFactory(UIType.HG_MainGame)]
    public class HG_GameWarFactory: IUIFactory
    {
        public UI Create(Scene scene, string type, GameObject gameObject)
        {
            try
            {
                ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
                resourcesComponent.LoadBundle($"{type}.unity3d");
                resourcesComponent.LoadBundle($"{UIType.HG_Sound}.unity3d");
                resourcesComponent.LoadBundle($"{UIType.HG_HitEff}.unity3d");

                GameObject bundleGameObject = (GameObject)resourcesComponent.GetAsset($"{type}.unity3d", "MainGame");
                GameObject tarObj = UnityEngine.Object.Instantiate(bundleGameObject);
                tarObj.layer = LayerMask.NameToLayer(LayerNames.WAR);
                UI ui = ComponentFactory.Create<UI, GameObject>(tarObj);

                ui.AddComponent<HG_GameWarComponent>();
                return ui;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        public void Remove(string type)
        {
            ETModel.Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle($"{type}.unity3d");
            ETModel.Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle($"{UIType.HG_Sound}.unity3d");
            ETModel.Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle($"{UIType.HG_HitEff}.unity3d");
        }
    }
}