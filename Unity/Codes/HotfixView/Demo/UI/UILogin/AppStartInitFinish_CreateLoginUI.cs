using System;
using UnityEngine;

namespace ET
{
	public class AppStartInitFinish_CreateLoginUI: AEvent<EventType.AppStartInitFinish>
	{
		protected override async ETTask Run(EventType.AppStartInitFinish arg)
		{
			await UIHelper.Create(arg.ZoneScene, UIType.UILogin, UILayer.Mid);

			ETCancellationToken cancellationToken = new ETCancellationToken();
			MoveToAsync(Vector3.zero, cancellationToken).Coroutine();
			
			cancellationToken.Cancel();
			
			/*var result = await Task.Run(() =>
			{
				Log.Debug("开始");
				Thread.Sleep(1000);
				return 25;
			});
			Log.Debug("结束");
			Log.Debug(result.ToString());*/

			/*var awaiter = Task.Run(() =>
			{
				Log.Debug("开始");
				Thread.Sleep(1000);
				return 25;
			}).ContinueWith((t) =>
			{
				Log.Debug(t.Result.ToString());
			});
			
			Log.Debug("111111");*/
		}

		private async ETTask MoveToAsync(Vector3 pos,ETCancellationToken cancellationToken)
		{
			Log.Debug("角色开始移动");

			bool result = await TimerComponent.Instance.WaitAsync(3000, cancellationToken);
			if (result)
			{
				Log.Debug("Move Over! ! !");
			}
			else
			{
				Log.Debug("Move Stop! ! !");
			}
		}
	}
}
