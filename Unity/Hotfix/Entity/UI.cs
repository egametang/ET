using System.Collections.Generic;
using Model;
using UnityEngine;

namespace Hotfix
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

		public Dictionary<string, UI> children = new Dictionary<string, UI>();

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (UI ui in this.children.Values)
			{
				ui.Dispose();
			}

			UnityEngine.Object.Destroy(this.GameObject);
		}

		public void SetAsFirstSibling()
		{
			this.GameObject.transform.SetAsFirstSibling();
		}

		public UI(Scene scene, UIType uiType, UI parent, GameObject gameObject)
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
			UI ui;
			if (!this.children.TryGetValue(name, out ui))
			{
				return;
			}
			this.children.Remove(name);
			ui.Dispose();
		}
	}
}