using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;

namespace Base
{
	public static class Game
	{
		private static readonly HashSet<Disposer> disposers = new HashSet<Disposer>();

		private static DisposerEventManager disposerEventManager;

		private static Scene scene;

		public static Scene Scene
		{
			get
			{
				return scene ?? (scene = new Scene());
			}
		}

		public static void CloseScene()
		{
			scene.Dispose();
			scene = null;
		}

		public static void ClearDisposers()
		{
			foreach (Disposer disposer in disposers)
			{
				disposer.Dispose();
			}
			disposers.Clear();
		}

		public static DisposerEventManager DisposerEventManager
		{
			get
			{
				return disposerEventManager ?? (disposerEventManager = new DisposerEventManager());
			}
		}

		public static void Add(Disposer disposer)
		{
			disposers.Add(disposer);
			disposerEventManager.Add(disposer);
		}

		public static void Remove(Disposer disposer)
		{
			disposers.Remove(disposer);
			disposerEventManager.Remove(disposer);
		}

		public static void Update()
		{
			disposerEventManager.Update();
		}

		public static string DisposerInfo()
		{
			var info = new Dictionary<string, int>();
			foreach (Disposer disposer in disposers)
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