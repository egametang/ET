using System.Collections.Generic;

using UnityEngine;

namespace ET
{
	[ObjectSystem]
	public class UIAwakeSystem : AwakeSystem<UI, string, GameObject>
	{
		public override void Awake(UI self, string name, GameObject gameObject)
		{

			self.Awake(name, gameObject);
		}
	}
	
	public sealed class UI: Entity, IAwake<string, GameObject>
	{
		public GameObject GameObject;
		
		public string Name { get; private set; }

		public Dictionary<string, UI> nameChildren = new Dictionary<string, UI>();
		
		public void Awake(string name, GameObject gameObject)
		{
			this.nameChildren.Clear();
			gameObject.AddComponent<ComponentView>().Component = this;
			gameObject.layer = LayerMask.NameToLayer(LayerNames.UI);
			this.Name = name;
			this.GameObject = gameObject;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			foreach (UI ui in this.nameChildren.Values)
			{
				ui.Dispose();
			}
			
			UnityEngine.Object.Destroy(this.GameObject);
			this.nameChildren.Clear();
		}

		public void SetAsFirstSibling()
		{
			this.GameObject.transform.SetAsFirstSibling();
		}

		public void Add(UI ui)
		{
			this.nameChildren.Add(ui.Name, ui);
		}

		public void Remove(string name)
		{
			UI ui;
			if (!this.nameChildren.TryGetValue(name, out ui))
			{
				return;
			}
			this.nameChildren.Remove(name);
			ui.Dispose();
		}

		public UI Get(string name)
		{
			UI child;
			if (this.nameChildren.TryGetValue(name, out child))
			{
				return child;
			}
			GameObject childGameObject = this.GameObject.transform.Find(name)?.gameObject;
			if (childGameObject == null)
			{
				return null;
			}
			child = this.AddChild<UI, string, GameObject>(name, childGameObject);
			this.Add(child);
			return child;
		}
	}
}