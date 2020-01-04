﻿using System.Collections.Generic;
using ETModel;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class UiAwakeSystem : AwakeSystem<UI, string, GameObject>
	{
		public override void Awake(UI self, string name, GameObject gameObject)
		{

			self.Awake(name, gameObject);
		}
	}
	
	[HideInHierarchy]
	public sealed class UI: Entity
	{
		public string Name { get; private set; }

		public Dictionary<string, UI> children = new Dictionary<string, UI>();
		
		public void Awake(string name, GameObject gameObject)
		{
			this.children.Clear();
			gameObject.AddComponent<ComponentView>().Component = this;
			gameObject.layer = LayerMask.NameToLayer(LayerNames.UI);
			this.Name = name;
			this.ViewGO = gameObject;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			foreach (UI ui in this.children.Values)
			{
				ui.Dispose();
			}
			
			UnityEngine.Object.Destroy(this.ViewGO);
			children.Clear();
		}

		public void SetAsFirstSibling()
		{
			this.ViewGO.transform.SetAsFirstSibling();
		}

		public void Add(UI ui)
		{
			this.children.Add(ui.Name, ui);
			ui.Parent = this;
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

		public UI Get(string name)
		{
			UI child;
			if (this.children.TryGetValue(name, out child))
			{
				return child;
			}
			GameObject childGameObject = this.ViewGO.transform.Find(name)?.gameObject;
			if (childGameObject == null)
			{
				return null;
			}
			child = EntityFactory.Create<UI, string, GameObject>(this.Domain, name, childGameObject);
			this.Add(child);
			return child;
		}
	}
}