﻿using UnityEngine;
using UnityEngine.UI;

namespace Model
{
	[ObjectSystem]
	public class UiLoadingComponentSystem : ObjectSystem<UILoadingComponent>, IAwake, IStart
	{
		public void Awake()
		{
			UILoadingComponent self = this.Get();
			self.text = self.GetParent<UI>().GameObject.Get<GameObject>("Text").GetComponent<Text>();
		}

		public async void Start()
		{
			UILoadingComponent self = this.Get();

			TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();
			
			while (true)
			{
				await timerComponent.WaitAsync(1000);
				
				if (self.Id == 0)
				{
					return;
				}

				BundleDownloaderComponent bundleDownloaderComponent = Game.Scene.GetComponent<BundleDownloaderComponent>();
				if (bundleDownloaderComponent == null)
				{
					continue;
				}
				self.text.text = $"{bundleDownloaderComponent.Progress}%";
			}
		}
	}
	
	public class UILoadingComponent : Component
	{
		public Text text;
	}
}
