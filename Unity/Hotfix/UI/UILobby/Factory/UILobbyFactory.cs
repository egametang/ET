using Model;
using UnityEngine;

namespace Hotfix
{
    [UIFactory(UIType.Lobby)]
    public class UILobbyFactory : IUIFactory
    {
        public UI Create(Scene scene, int type, UI parent)
        {
			GameObject bundleGameObject = scene.ModelScene.GetComponent<ResourcesComponent>().GetAsset<GameObject>("uilobby", "Lobby");
			GameObject lobby = UnityEngine.Object.Instantiate(bundleGameObject);
			lobby.layer = LayerMask.NameToLayer(LayerNames.UI);
			UI ui = new UI(scene, type, parent, lobby);
			parent.Add(ui);

	        ui.AddComponent<UILobbyComponent>();
			return ui;
        }
    }
}