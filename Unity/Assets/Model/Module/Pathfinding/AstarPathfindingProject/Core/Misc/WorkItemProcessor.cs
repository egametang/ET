namespace Pathfinding {
	using UnityEngine;

	/** An item of work that can be executed when graphs are safe to update.
	 * \see #AstarPath.UpdateGraphs
	 * \see #AstarPath.AddWorkItem
	 */
	public struct AstarWorkItem {
		/** Init function.
		 * May be null if no initialization is needed.
		 * Will be called once, right before the first call to #update.
		 */
		public System.Action init;

		/** Init function.
		 * May be null if no initialization is needed.
		 * Will be called once, right before the first call to #update.
		 *
		 * A context object is sent as a parameter. This can be used
		 * to for example queue a flood fill that will be executed either
		 * when a work item calls EnsureValidFloodFill or all work items have
		 * been completed. If multiple work items are updating nodes
		 * so that they need a flood fill afterwards, using the QueueFloodFill
		 * method is preferred since then only a single flood fill needs
		 * to be performed for all of the work items instead of one
		 * per work item.
		 */
		public System.Action<IWorkItemContext> initWithContext;

		/** Update function, called once per frame when the work item executes.
		 * Takes a param \a force. If that is true, the work item should try to complete the whole item in one go instead
		 * of spreading it out over multiple frames.
		 * \returns True when the work item is completed.
		 */
		public System.Func<bool, bool> update;

		/** Update function, called once per frame when the work item executes.
		 * Takes a param \a force. If that is true, the work item should try to complete the whole item in one go instead
		 * of spreading it out over multiple frames.
		 * \returns True when the work item is completed.
		 *
		 * A context object is sent as a parameter. This can be used
		 * to for example queue a flood fill that will be executed either
		 * when a work item calls EnsureValidFloodFill or all work items have
		 * been completed. If multiple work items are updating nodes
		 * so that they need a flood fill afterwards, using the QueueFloodFill
		 * method is preferred since then only a single flood fill needs
		 * to be performed for all of the work items instead of one
		 * per work item.
		 */
		public System.Func<IWorkItemContext, bool, bool> updateWithContext;

		public AstarWorkItem (System.Func<bool, bool> update) {
			this.init = null;
			this.initWithContext = null;
			this.updateWithContext = null;
			this.update = update;
		}

		public AstarWorkItem (System.Func<IWorkItemContext, bool, bool> update) {
			this.init = null;
			this.initWithContext = null;
			this.updateWithContext = update;
			this.update = null;
		}

		public AstarWorkItem (System.Action init, System.Func<bool, bool> update = null) {
			this.init = init;
			this.initWithContext = null;
			this.update = update;
			this.updateWithContext = null;
		}

		public AstarWorkItem (System.Action<IWorkItemContext> init, System.Func<IWorkItemContext, bool, bool> update = null) {
			this.init = null;
			this.initWithContext = init;
			this.update = null;
			this.updateWithContext = update;
		}
	}

	/** Interface to expose a subset of the WorkItemProcessor functionality */
	public interface IWorkItemContext {
		/** Call during work items to queue a flood fill.
		 * An instant flood fill can be done via FloodFill()
		 * but this method can be used to batch several updates into one
		 * to increase performance.
		 * WorkItems which require a valid Flood Fill in their execution can call EnsureValidFloodFill
		 * to ensure that a flood fill is done if any earlier work items queued one.
		 *
		 * Once a flood fill is queued it will be done after all WorkItems have been executed.
		 */
		void QueueFloodFill ();

		/** If a WorkItem needs to have a valid flood fill during execution, call this method to ensure there are no pending flood fills */
		void EnsureValidFloodFill ();
	}

	class WorkItemProcessor : IWorkItemContext {
		/** Used to prevent waiting for work items to complete inside other work items as that will cause the program to hang */
		public bool workItemsInProgressRightNow { get; private set; }

		readonly IndexedQueue<AstarWorkItem> workItems = new IndexedQueue<AstarWorkItem>();

		/** True if any work items have queued a flood fill.
		 * \see QueueWorkItemFloodFill
		 */
		bool queuedWorkItemFloodFill = false;

		/**
		 * True while a batch of work items are being processed.
		 * Set to true when a work item is started to be processed, reset to false when all work items are complete.
		 *
		 * Work item updates are often spread out over several frames, this flag will be true during the whole time the
		 * updates are in progress.
		 */
		public bool workItemsInProgress { get; private set; }

		/** Similar to Queue<T> but allows random access */
		class IndexedQueue<T> {
			T[] buffer = new T[4];
			int start;

			public T this[int index] {
				get {
					if (index < 0 || index >= Count) throw new System.IndexOutOfRangeException();
					return buffer[(start + index) % buffer.Length];
				}
				set {
					if (index < 0 || index >= Count) throw new System.IndexOutOfRangeException();
					buffer[(start + index) % buffer.Length] = value;
				}
			}

			public int Count { get; private set; }

			public void Enqueue (T item) {
				if (Count == buffer.Length) {
					var newBuffer = new T[buffer.Length*2];
					for (int i = 0; i < Count; i++) {
						newBuffer[i] = this[i];
					}
					buffer = newBuffer;
					start = 0;
				}

				buffer[(start + Count) % buffer.Length] = item;
				Count++;
			}

			public T Dequeue () {
				if (Count == 0) throw new System.InvalidOperationException();
				var item = buffer[start];
				start = (start + 1) % buffer.Length;
				Count--;
				return item;
			}
		}

		/** Call during work items to queue a flood fill.
		 * An instant flood fill can be done via FloodFill()
		 * but this method can be used to batch several updates into one
		 * to increase performance.
		 * WorkItems which require a valid Flood Fill in their execution can call EnsureValidFloodFill
		 * to ensure that a flood fill is done if any earlier work items queued one.
		 *
		 * Once a flood fill is queued it will be done after all WorkItems have been executed.
		 */
		void IWorkItemContext.QueueFloodFill () {
			queuedWorkItemFloodFill = true;
		}

		/** If a WorkItem needs to have a valid flood fill during execution, call this method to ensure there are no pending flood fills */
		public void EnsureValidFloodFill () {
			if (queuedWorkItemFloodFill) {
				AstarPath.active.FloodFill();
			}
		}

		public void OnFloodFill () {
			queuedWorkItemFloodFill = false;
		}

		/** Add a work item to be processed when pathfinding is paused.
		 *
		 * \see ProcessWorkItems
		 */
		public void AddWorkItem (AstarWorkItem item) {
			workItems.Enqueue(item);
		}

		/** Process graph updating work items.
		 * Process all queued work items, e.g graph updates and the likes.
		 *
		 * \returns
		 * - false if there are still items to be processed.
		 * - true if the last work items was processed and pathfinding threads are ready to be resumed.
		 *
		 * \see AddWorkItem
		 * \see threadSafeUpdateState
		 * \see Update
		 */
		public bool ProcessWorkItems (bool force) {
			if (workItemsInProgressRightNow) throw new System.Exception("Processing work items recursively. Please do not wait for other work items to be completed inside work items. " +
					"If you think this is not caused by any of your scripts, this might be a bug.");

			workItemsInProgressRightNow = true;
			AstarPath.active.data.LockGraphStructure(true);
			while (workItems.Count > 0) {
				// Working on a new batch
				if (!workItemsInProgress) {
					workItemsInProgress = true;
					queuedWorkItemFloodFill = false;
				}

				// Peek at first item in the queue
				AstarWorkItem itm = workItems[0];

				// Call init the first time the item is seen
				if (itm.init != null) {
					itm.init();
					itm.init = null;
				}

				if (itm.initWithContext != null) {
					itm.initWithContext(this);
					itm.initWithContext = null;
				}

				// Make sure the item in the queue is up to date
				workItems[0] = itm;

				bool status;
				try {
					if (itm.update != null) {
						status = itm.update(force);
					} else if (itm.updateWithContext != null) {
						status = itm.updateWithContext(this, force);
					} else {
						status = true;
					}
				} catch {
					workItems.Dequeue();
					workItemsInProgressRightNow = false;
					AstarPath.active.data.UnlockGraphStructure();
					throw;
				}

				if (!status) {
					if (force) {
						Debug.LogError("Misbehaving WorkItem. 'force'=true but the work item did not complete.\nIf force=true is passed to a WorkItem it should always return true.");
					}

					// Still work items to process
					workItemsInProgressRightNow = false;
					AstarPath.active.data.UnlockGraphStructure();
					return false;
				} else {
					workItems.Dequeue();
				}
			}

			EnsureValidFloodFill();

			workItemsInProgressRightNow = false;
			workItemsInProgress = false;
			AstarPath.active.data.UnlockGraphStructure();
			return true;
		}
	}
}
