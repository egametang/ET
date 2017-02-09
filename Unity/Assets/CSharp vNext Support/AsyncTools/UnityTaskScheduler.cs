using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class UnityTaskScheduler : TaskScheduler
{
	public string Name { get; }
	public UnitySynchronizationContext Context { get; }
	private readonly LinkedList<Task> queue = new LinkedList<Task>();

	public UnityTaskScheduler(string name)
	{
		Name = name;
		Context = new UnitySynchronizationContext(name);
	}

	public void Activate()
	{
		SynchronizationContext.SetSynchronizationContext(Context);
		Context.Activate();

		ExecutePendingTasks();
		Context.ExecutePendingContinuations();
	}

	protected override IEnumerable<Task> GetScheduledTasks()
	{
		lock (queue)
		{
			return queue.ToArray();
		}
	}

	protected override void QueueTask(Task task)
	{
		lock (queue)
		{
			queue.AddLast(task);
		}
	}

	protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
	{
		if (Context != SynchronizationContext.Current)
		{
			return false;
		}

		if (taskWasPreviouslyQueued)
		{
			lock (queue)
			{
				queue.Remove(task);
			}
		}

		return TryExecuteTask(task);
	}

	private void ExecutePendingTasks()
	{
		while (true)
		{
			Task task;
			lock (queue)
			{
				if (queue.Count == 0)
				{
					break;
				}
				task = queue.First.Value;
				queue.RemoveFirst();
			}

			if (task != null)
			{
				var result = TryExecuteTask(task);
				if (result == false)
				{
					throw new InvalidOperationException();
				}
			}
		}
	}
}