using Model;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Controller
{
    [UIFactory(UIType.UILogin)]
    public class UILoginFactory : IUIFactory
    {
        public UI Create(Scene scene, UIType type, UI parent)
        {
            GameObject mainPrefab = Resources.Load<GameObject>("UI/LoginPanel");
            mainPrefab = Object.Instantiate(mainPrefab);
			mainPrefab.layer = LayerMask.NameToLayer(LayerNames.UI);

			UI ui = new UI(scene, type, parent, mainPrefab);
			parent.AddChild(ui);
            
            return ui;
        }
    }
}