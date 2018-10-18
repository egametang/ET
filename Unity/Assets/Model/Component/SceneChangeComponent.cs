using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ETModel
{
	[ObjectSystem]
	public class SceneChangeComponentUpdateSystem: UpdateSystem<SceneChangeComponent>
	{
		public override void Update(SceneChangeComponent self)
		{
			if (self.loadMapOperation.isDone)
			{
				self.tcs.SetResult(true);
			}
		}
	}

	public class SceneChangeComponent: Component
	{
		public AsyncOperation loadMapOperation;
		public ETTaskCompletionSource<bool> tcs;
	    public float deltaTime;
	    public int lastProgress = 0;

		public ETTask<bool> ChangeSceneAsync(string sceneName)
		{
			this.tcs = new ETTaskCompletionSource<bool>();
			// 加载map
			this.loadMapOperation = SceneManager.LoadSceneAsync(sceneName);
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

			if (this.Entity.IsDisposed)
			{
				return;
			}
			
			this.Entity.RemoveComponent<SceneChangeComponent>();
		}
	}
}