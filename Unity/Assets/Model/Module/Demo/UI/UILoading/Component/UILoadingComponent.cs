﻿using UnityEngine;
using UnityEngine.UI;

namespace ETModel
{
	[ObjectSystem]
	public class UiLoadingComponentAwakeSystem : AwakeSystem<UILoadingComponent>
	{
		public override void Awake(UILoadingComponent self)
		{
			self.text = self.GetParent<UI>().ViewGO.Get<GameObject>("Text").GetComponent<Text>();
		}
	}

	[ObjectSystem]
	public class UiLoadingComponentStartSystem : StartSystem<UILoadingComponent>
	{
		public override void Start(UILoadingComponent self)
		{
			StartAsync(self).Coroutine();
		}
		
		public async ETVoid StartAsync(UILoadingComponent self)
		{
			TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();
			long instanceId = self.InstanceId;
			while (true)
			{
				await timerComponent.WaitAsync(1000);

				if (self.InstanceId != instanceId)
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

	public class UILoadingComponent : Entity
	{
		public Text text;
	}
}
