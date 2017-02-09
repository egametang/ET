using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UnityScheduler : MonoBehaviour
{
	public static int MainThreadId { get; private set; }

	/// <summary>
	/// Use UpdateScheduler, LateUpdateScheduler or FixedUpdateScheduler instead.
	/// </summary>
	[Obsolete]
	public static UnityTaskScheduler MainThreadScheduler => UpdateScheduler;

	/// <summary>
	/// Executes tasks in the main thread, Update context.
	/// </summary>
	public static UnityTaskScheduler UpdateScheduler { get; private set; }
	
	/// <summary>
	/// Executes tasks in the main thread, LateUpdate context.
	/// </summary>
	public static UnityTaskScheduler LateUpdateScheduler { get; private set; }

	/// <summary>
	/// Executes tasks in the main thread, FixedUpdate context.
	/// </summary>
	public static UnityTaskScheduler FixedUpdateScheduler { get; private set; }

	public static UnityTaskScheduler EditorUpdateScheduler { get; private set; }

	/// <summary>
	/// Executes tasks in the thread pool. It's an alias for TaskScheduler.Default.
	/// </summary>
	public static TaskScheduler ThreadPoolScheduler => TaskScheduler.Default;

	[RuntimeInitializeOnLoadMethod]
	private static void Initialize()
	{
		MainThreadId = Thread.CurrentThread.ManagedThreadId;

		UpdateScheduler = new UnityTaskScheduler("Update");
		LateUpdateScheduler = new UnityTaskScheduler("LateUpdate");
		FixedUpdateScheduler = new UnityTaskScheduler("FixedUpdate");

		SynchronizationContext.SetSynchronizationContext(UpdateScheduler.Context);

		var go = new GameObject("UnityScheduler");
		go.hideFlags = HideFlags.HideAndDontSave;
		go.AddComponent<UnityScheduler>();
	}

	public static void InitializeInEditor()
	{
		MainThreadId = Thread.CurrentThread.ManagedThreadId;
		EditorUpdateScheduler = new UnityTaskScheduler("EditorUpdate");
		SynchronizationContext.SetSynchronizationContext(EditorUpdateScheduler.Context);
	}

	public static void ProcessEditorUpdate() => EditorUpdateScheduler.Activate();

	private void Update() => UpdateScheduler.Activate();

	private void LateUpdate() => LateUpdateScheduler.Activate();

	private void FixedUpdate() => FixedUpdateScheduler.Activate();
}