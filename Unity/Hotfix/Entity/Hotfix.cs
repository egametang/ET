﻿namespace Hotfix
{
	public static class Hotfix
	{
		private static Scene scene;

		public static Scene Scene
		{
			get
			{
				if (scene != null)
				{
					return scene;
				}
				scene = new Scene();
				return scene;
			}
		}
		
		public static void Close()
		{
			scene.Dispose();
			scene = null;
		}
	}
}