using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;

namespace Base
{
	public static class Game
	{
		private static HashSet<Disposer> disposers;

		private static EntityEventManager entityEventManager;

		private static Scene scene;

		public static Scene Scene
		{
			get
			{
				return scene ?? (scene = new Scene());
			}
		}

		public static HashSet<Disposer> Disposers
		{
			get
			{
				return disposers ?? (disposers = new HashSet<Disposer>());
			}
		}

		public static void CloseScene()
		{
			scene.Dispose();
			scene = null;
		}

		public static void ClearDisposers()
		{
			foreach (Disposer disposer in Disposers)
			{
				disposer.Dispose();
			}
			disposers.Clear();
			disposers = null;
		}

		public static EntityEventManager EntityEventManager
		{
			get
			{
				return entityEventManager ?? (entityEventManager = new EntityEventManager());
			}
		}

		public static string DisposerInfo()
		{
			var info = new Dictionary<string, int>();
			foreach (Disposer disposer in Disposers)
			{
				if (info.ContainsKey(disposer.GetType().Name))
				{
					info[disposer.GetType().Name] += 1;
				}
				else
				{
					info[disposer.GetType().Name] = 1;
				}
			}
			info = info.OrderByDescending(s => s.Value).ToDictionary(p => p.Key, p => p.Value);
			StringBuilder sb = new StringBuilder();
			sb.Append("\r\n");
			foreach (string key in info.Keys)
			{
				sb.Append($"{info[key],10} {key}\r\n");
			}
			
			return sb.ToString();
		}
	}
}