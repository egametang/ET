using System;
using System.Threading;
using System.Threading.Tasks;

namespace ET
{
	public class AppStartInitFinish_CreateLoginUI: AEvent<EventType.AppStartInitFinish>
	{
		protected override async ETTask Run(EventType.AppStartInitFinish arg)
		{
			await UIHelper.Create(arg.ZoneScene, UIType.UILogin, UILayer.Mid);

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

	}
}
