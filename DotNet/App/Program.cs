using System;
using System.Threading;
using ET.Server;

namespace ET
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			try
			{	
				Entry.Start();
				
				while (true)
				{
					try
					{
						Thread.Sleep(1);
						Game.Update();
						Game.LateUpdate();
						Game.FrameFinish();
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
