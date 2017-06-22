using System.Collections.Generic;
using UnityEngine;

namespace Hotfix
{
	public sealed class UI: HotfixEntity
	{
		public Scene Scene { get; set; }

		public int UIType { get; }

		public string Name
		{
			get
			{
				return this.GameObject.name;
			}
		}

		public GameObject GameObject { get; }

		public Dictionary<string, UI> children = new Dictionary<string, UI>();

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

		public UI(Scene scene, int uiType, UI parent, GameObject gameObject): base(EntityType.UI)
		{
			this.Scene = scene;
			this.UIType = uiType;

			if (parent != null)
			{
				gameObject.transform.SetParent(parent.GameObject.transform, false);
			}
			this.GameObject = gameObject;
		}

		public void Add(UI ui)
		{
			this.children.Add(ui.Name, ui);
		}

		public void Remove(string name)
		{
			if (!this.children.TryGetValue(name, out UI ui))
			{
				return;
			}
			this.children.Remove(name);
			ui.Dispose();
		}
	}
}