using System;
using ETModel;
using UnityEngine;


namespace ETHotfix
{
    [UIFactory(UIType.HG_UIResult)]
    public class HG_ResultPanelFactory : IUIFactory
    {
        public UI Create(Scene scene, string type, GameObject gameObject)
        {
            try
            {
                ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
                resourcesComponent.LoadBundle($"{type}.unity3d");
                GameObject bundleGameObject = (GameObject)resourcesComponent.GetAsset($"{type}.unity3d", "GameFinishPlane");
                GameObject tarObj = UnityEngine.Object.Instantiate(bundleGameObject);
                tarObj.layer = LayerMask.NameToLayer(LayerNames.UI);
                UI ui = ComponentFactory.Create<UI, GameObject>(tarObj);

                ui.AddComponent<HG_PanelResultCp,bool>(true);
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
