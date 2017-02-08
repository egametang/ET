using UnityEngine;

namespace Model
{
	public sealed class UI: Entity
	{
		public Scene Scene { get; set; }

		public UIType UIType { get; }

		public string Name
		{
			get
			{
				return this.GameObject.name;
			}
		}

		public GameObject GameObject { get; }

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}

		public void SetAsFirstSibling()
		{
			this.GameObject.transform.SetAsFirstSibling();
		}

		public UI(Scene scene, UIType uiType, UI parent, GameObject gameObject): base(EntityType.UI)
		{
			this.Scene = scene;
			this.UIType = uiType;

			gameObject.transform.SetParent(parent?.GameObject.transform);
			this.GameObject = gameObject;
			this.AddComponent<ChildrenComponent<UI>>();
		}
	}
}