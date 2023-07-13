using System;

namespace UniFramework.Window
{
	[AttributeUsage(AttributeTargets.Class)]
	public class WindowAttribute : Attribute
	{
		/// <summary>
		/// 窗口层级
		/// </summary>
		public int WindowLayer;

		/// <summary>
		/// 全屏窗口标记
		/// </summary>
		public bool FullScreen;

		public WindowAttribute(int windowLayer, bool fullScreen)
		{
			WindowLayer = windowLayer;
			FullScreen = fullScreen;
		}
	}
}