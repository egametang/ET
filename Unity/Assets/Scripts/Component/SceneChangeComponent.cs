using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ETModel
{
	public class SceneChangeComponent: Component
	{
		public AsyncOperation loadMapOperation;
		public TaskCompletionSource<bool> tcs;
	    public float deltaTime;
	    public int lastProgress = 0;

		public Task<bool> ChangeSceneAsync(SceneType sceneEnum)
		{
			this.tcs = new TaskCompletionSource<bool>();
			// 加载map
			this.loadMapOperation = SceneManager.LoadSceneAsync(sceneEnum.ToString());
			return this.tcs.Task;
		}

		public int Process
		{
			get
			{
				if (this.loadMapOperation == null)
				{
					return 0;
				}
				return (int)(this.loadMapOperation.progress * 100);
			}
		}

		public void Finish()
		{
			this.tcs.SetResult(true);
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();
		}
	}
}