#if NETFX_CORE
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using TP = System.Reflection.TypeInfo;
#else
using TP = System.Type;
#endif

namespace PF {
	public static class WindowsStoreCompatibility {
		public static System.Type GetTypeFromInfo (TP type) {
#if NETFX_CORE
			return type.AsType();
#else
			return type;
#endif
		}

		public static TP GetTypeInfo (System.Type type) {
#if NETFX_CORE
			return type.GetTypeInfo();
#else
			return type;
#endif
		}

#if NETFX_CORE
		public static void Close (this BinaryWriter stream) {
			stream.Dispose();
		}

		public static void Close (this BinaryReader stream) {
			stream.Dispose();
		}

		public static void Close (this StreamWriter stream) {
			stream.Dispose();
		}
#endif
	}

#if NETFX_CORE
	public delegate void ParameterizedThreadStart (System.Object ob);
	public delegate void ThreadStart ();

	public class Thread {
		//
		// Fields
		//
		private Pathfinding.WindowsStore.ParameterizedThreadStart _paramThreadStart;

		private CancellationTokenSource _taskCancellationTokenSource;

		private Task _task = null;

		private Pathfinding.WindowsStore.ThreadStart _threadStart;

		private static ManualResetEvent SleepEvent = new ManualResetEvent(false);

		//
		// Properties
		//
		public bool IsAlive {
			get {
				return this._task != null && !this._task.IsCompleted;
			}
			set {
				throw new System.NotImplementedException();
			}
		}

		public bool IsBackground {
			get {
				return false;
			}
			set {
			}
		}

		public string Name {
			get;
			set;
		}

		//
		// Constructors
		//
		public Thread (Pathfinding.WindowsStore.ParameterizedThreadStart start) {
			this._taskCancellationTokenSource = new CancellationTokenSource();
			this._paramThreadStart = start;
		}

		public Thread (Pathfinding.WindowsStore.ThreadStart start) {
			this._taskCancellationTokenSource = new CancellationTokenSource();
			this._threadStart = start;
		}

		//
		// Static Methods
		//
		public static void Sleep (int ms) {
			SleepEvent.WaitOne(ms);
		}

		//
		// Methods
		//
		public void Abort () {
			if (this._taskCancellationTokenSource != null) {
				this._taskCancellationTokenSource.Cancel();
			}
		}

		private void EnsureTask (object paramThreadStartParam = null) {
			if (this._task == null) {
				if (this._paramThreadStart != null) {
					this._task = new Task(delegate {
						this._paramThreadStart(paramThreadStartParam);
					}, this._taskCancellationTokenSource.Token);
				} else {
					if (this._threadStart != null) {
						this._task = new Task(delegate {
							this._threadStart();
						}, this._taskCancellationTokenSource.Token);
					}
				}
			}
		}

		public bool Join (int ms) {
			this.EnsureTask();
			return this._task.Wait(ms, this._taskCancellationTokenSource.Token);
		}

		public void Start () {
			this.EnsureTask();
			this._task.Start(TaskScheduler.Default);
		}

		public void Start (object param) {
			this.EnsureTask(param);
			this._task.Start(TaskScheduler.Default);
		}
	}
#endif
}
