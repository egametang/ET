using System;
using ETModel;
using UnityEngine;


namespace ETHotfix
{
    [UIFactory(UIType.HG_UIPause)]
    public class HG_PausePanelFactory : IUIFactory
    {
        public UI Create(Scene scene, string type, GameObject gameObject)
        {
            try
            {
                ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
                resourcesComponent.LoadBundle($"{type}.unity3d");
                GameObject bundleGameObject = (GameObject)resourcesComponent.GetAsset($"{type}.unity3d", "PausePlane");
                GameObject tarObj = UnityEngine.Object.Instantiate(bundleGameObject);
                tarObj.layer = LayerMask.NameToLayer(LayerNames.UI);
                UI ui = ComponentFactory.Create<UI, GameObject>(tarObj);

                ui.AddComponent<HG_PanelPauseCp>();
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
        }
    }
}
