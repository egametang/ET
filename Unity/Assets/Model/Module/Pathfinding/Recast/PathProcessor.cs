using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using PF;

namespace PF {
#if NETFX_CORE
	using Thread = Pathfinding.WindowsStore.Thread;
#else
	using Thread = System.Threading.Thread;
#endif

	public class PathProcessor {
		public event System.Action<Path> OnPathPreSearch;
		public event System.Action<Path> OnPathPostSearch;
		public event System.Action OnQueueUnblocked;

		internal readonly ThreadControlQueue queue;
		readonly PathReturnQueue returnQueue;

		readonly PathHandler[] pathHandlers;

		/** References to each of the pathfinding threads */
		readonly Thread[] threads;

		/** When no multithreading is used, the IEnumerator is stored here.
		 * When no multithreading is used, a coroutine is used instead. It is not directly called with StartCoroutine
		 * but a separate function has just a while loop which increments the main IEnumerator.
		 * This is done so other functions can step the thread forward at any time, without having to wait for Unity to update it.
		 * \see CalculatePaths
		 * \see CalculatePathsHandler
		 */
		IEnumerator threadCoroutine;

		/** Holds the next node index which has not been used by any previous node.
		 * \see nodeIndexPool
		 */
		int nextNodeIndex = 1;

		/** Holds indices for nodes that have been destroyed.
		 * To avoid trashing a lot of memory structures when nodes are
		 * frequently deleted and created, node indices are reused.
		 */
		readonly Stack<int> nodeIndexPool = new Stack<int>();

		readonly List<int> locks = new List<int>();
		int nextLockID = 0;

		/** Number of parallel pathfinders.
		 * Returns the number of concurrent processes which can calculate paths at once.
		 * When using multithreading, this will be the number of threads, if not using multithreading it is always 1 (since only 1 coroutine is used).
		 * \see threadInfos
		 * \see IsUsingMultithreading
		 */
		public int NumThreads {
			get {
				return pathHandlers.Length;
			}
		}

		/** Returns whether or not multithreading is used */
		public bool IsUsingMultithreading {
			get {
				return threads != null;
			}
		}

		internal PathProcessor (PathReturnQueue returnQueue, int processors, bool multithreaded) {
			this.returnQueue = returnQueue;

			if (processors < 0) {
				throw new System.ArgumentOutOfRangeException("processors");
			}

			if (!multithreaded && processors != 1) {
				throw new System.Exception("Only a single non-multithreaded processor is allowed");
			}

			// Set up path queue with the specified number of receivers
			queue = new ThreadControlQueue(processors);
			pathHandlers = new PathHandler[processors];

			for (int i = 0; i < processors; i++) {
				pathHandlers[i] = new PathHandler(i, processors);
			}

			if (multithreaded) {
				threads = new Thread[processors];

				// Start lots of threads
				for (int i = 0; i < processors; i++) {
					var pathHandler = pathHandlers[i];
					threads[i] = new Thread(() => CalculatePathsThreaded(pathHandler));
					threads[i].Name = "Pathfinding Thread " + i;
					threads[i].IsBackground = true;
					threads[i].Start();
				}
			} else {
				// Start coroutine if not using multithreading
				threadCoroutine = CalculatePaths(pathHandlers[0]);
			}
		}

		/** Prevents pathfinding from running while held */
		public struct GraphUpdateLock {
			PathProcessor pathProcessor;
			int id;

			public GraphUpdateLock (PathProcessor pathProcessor, bool block) {
				this.pathProcessor = pathProcessor;
				id = pathProcessor.nextLockID++;
			}

			/** True while this lock is preventing the pathfinding threads from processing more paths.
			 * Note that the pathfinding threads may not be paused yet (if this lock was obtained using PausePathfinding(false)).
			 */
			public bool Held {
				get {
					return pathProcessor != null && pathProcessor.locks.Contains (id);
				}
			}

			/** Allow pathfinding to start running again if no other locks are still held */
			public void Release () {
			}
		}

		/** Prevents pathfinding threads from starting to calculate any new paths.
		 *
		 * \param block If true, this call will block until all pathfinding threads are paused.
		 * otherwise the threads will be paused as soon as they are done with what they are currently doing.
		 *
		 * \returns A lock object. You need to call Unlock on that object to allow pathfinding to resume.
		 *
		 * \note In most cases this should not be called from user code.
		 */
		public GraphUpdateLock PausePathfinding (bool block) {
			return new GraphUpdateLock(this, block);
		}

		public void TickNonMultithreaded () {
			// Process paths
			if (threadCoroutine != null) {
				try {
					threadCoroutine.MoveNext();
				} catch (System.Exception e) {
					//This will kill pathfinding
					threadCoroutine = null;
					queue.PopNoBlock(false);
				}
			}
		}

		/** Calls 'Join' on each of the threads to block until they have completed */
		public void JoinThreads () {
			if (threads != null) {
				for (int i = 0; i < threads.Length; i++) {
#if UNITY_WEBPLAYER
					if (!threads[i].Join(200)) {
						Debug.LogError("Could not terminate pathfinding thread["+i+"] in 200ms." +
							"Not good.\nUnity webplayer does not support Thread.Abort\nHoping that it will be terminated by Unity WebPlayer");
					}
#else
					if (!threads[i].Join(50)) {
#if !SERVER
						UnityEngine.Debug.LogError("Could not terminate pathfinding thread["+i+"] in 50ms, trying Thread.Abort");
#endif
						threads[i].Abort();
					}
#endif
				}
			}
		}

		/** Calls 'Abort' on each of the threads */
		public void AbortThreads () {
#if !UNITY_WEBPLAYER
			if (threads == null) return;
			// Unity webplayer does not support Abort (even though it supports starting threads). Hope that UnityPlayer aborts the threads
			for (int i = 0; i < threads.Length; i++) {
				if (threads[i] != null && threads[i].IsAlive) threads[i].Abort();
			}
#endif
		}

		/** Returns a new global node index.
		 * \warning This method should not be called directly. It is used by the GraphNode constructor.
		 */
		public int GetNewNodeIndex () {
			return nodeIndexPool.Count > 0 ? nodeIndexPool.Pop() : nextNodeIndex++;
		}

		/** Initializes temporary path data for a node.
		 * \warning This method should not be called directly. It is used by the GraphNode constructor.
		 */
		public void InitializeNode (GraphNode node) {
			// 单线程不考虑
			//if (!queue.AllReceiversBlocked) {
			//	throw new System.Exception("Trying to initialize a node when it is not safe to initialize any nodes. Must be done during a graph update. See http://arongranberg.com/astar/docs/graph-updates.php#direct");
			//}

			for (int i = 0; i < pathHandlers.Length; i++) {
				pathHandlers[i].InitializeNode(node);
			}
		}

		/** Destroyes the given node.
		 * This is to be called after the node has been disconnected from the graph so that it cannot be reached from any other nodes.
		 * It should only be called during graph updates, that is when the pathfinding threads are either not running or paused.
		 *
		 * \warning This method should not be called by user code. It is used internally by the system.
		 */
		public void DestroyNode (GraphNode node) {
			if (node.NodeIndex == -1) return;

			nodeIndexPool.Push(node.NodeIndex);

			for (int i = 0; i < pathHandlers.Length; i++) {
				pathHandlers[i].DestroyNode(node);
			}
		}

		/** Main pathfinding method (multithreaded).
		 * This method will calculate the paths in the pathfinding queue when multithreading is enabled.
		 *
		 * \see CalculatePaths
		 * \see StartPath
		 *
		 * \astarpro
		 */
		void CalculatePathsThreaded (PathHandler pathHandler) {
#if !ASTAR_FAST_BUT_NO_EXCEPTIONS
			try {
#endif

			// Max number of ticks we are allowed to continue working in one run.
			// One tick is 1/10000 of a millisecond.
			// We need to check once in a while if the thread should be stopped.
			long maxTicks = (long)(10*10000);
			long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

			while (true) {
				// The path we are currently calculating
				Path path = queue.Pop();
				// Access the internal implementation methods
				IPathInternals ipath = (IPathInternals)path;

				ipath.PrepareBase(pathHandler);

				// Now processing the path
				// Will advance to Processing
				ipath.AdvanceState(PathState.Processing);

				// Call some callbacks
				if (OnPathPreSearch != null) {
					OnPathPreSearch(path);
				}

				// Tick for when the path started, used for calculating how long time the calculation took
				long startTicks = System.DateTime.UtcNow.Ticks;

				// Prepare the path
				ipath.Prepare();

				if (!path.IsDone()) {
					// For visualization purposes, we set the last computed path to p, so we can view debug info on it in the editor (scene view).
					PathFindHelper.debugPathData = ipath.PathHandler;
					PathFindHelper.debugPathID = path.pathID;


					// Initialize the path, now ready to begin search
					ipath.Initialize();


					// Loop while the path has not been fully calculated
					while (!path.IsDone()) {
						// Do some work on the path calculation.
						// The function will return when it has taken too much time
						// or when it has finished calculation
						ipath.CalculateStep(targetTick);

						targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

					}

					path.duration = (System.DateTime.UtcNow.Ticks - startTicks)*0.0001F;

#if ProfileAstar
					System.Threading.Interlocked.Increment(ref AstarPath.PathsCompleted);
					System.Threading.Interlocked.Add(ref AstarPath.TotalSearchTime, System.DateTime.UtcNow.Ticks - startTicks);
#endif
				}

				// Cleans up node tagging and other things
				ipath.Cleanup();


				if (path.immediateCallback != null) path.immediateCallback(path);

				if (OnPathPostSearch != null) {
					OnPathPostSearch(path);
				}

				// Push the path onto the return stack
				// It will be detected by the main Unity thread and returned as fast as possible (the next late update hopefully)
				returnQueue.Enqueue(path);

				// Will advance to ReturnQueue
				ipath.AdvanceState(PathState.ReturnQueue);
			}
#if !ASTAR_FAST_BUT_NO_EXCEPTIONS
		}
		catch (System.Exception e) {
			{
				if (PathFindHelper.logPathResults == PathLog.Heavy)
#if !SERVER
					UnityEngine.Debug.LogWarning("Shutting down pathfinding thread #" + pathHandler.threadID);
#endif
				return;
			}
#if !SERVER
			UnityEngine.Debug.LogException(e);
			UnityEngine.Debug.LogError("Unhandled exception during pathfinding. Terminating.");
#endif
			// Unhandled exception, kill pathfinding
		}
#endif

#if !SERVER
			UnityEngine.Debug.LogError("Error : This part should never be reached.");
#endif
		}

		public IEnumerator CalculatePaths()
		{
			return this.CalculatePaths(this.pathHandlers[0]);
		}

		/** Main pathfinding method.
		 * This method will calculate the paths in the pathfinding queue.
		 *
		 * \see CalculatePathsThreaded
		 * \see StartPath
		 */
		IEnumerator CalculatePaths (PathHandler pathHandler) {
			// Max number of ticks before yielding/sleeping
			long maxTicks = (long)(PathFindHelper.GetConfig().maxFrameTime*10000);
			long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

			while (true) {
				// The path we are currently calculating
				Path p = null;

				while (p == null) {
					try {
						p = queue.PopNoBlock(false);
					} catch (Exception e) {
						yield break;
					}

					if (p == null) {
						yield return null;
					}
				}


				IPathInternals ip = (IPathInternals)p;

				// Max number of ticks we are allowed to continue working in one run
				// One tick is 1/10000 of a millisecond
				maxTicks = (long)(PathFindHelper.GetConfig().maxFrameTime*10000);

				ip.PrepareBase(pathHandler);

				// Now processing the path
				// Will advance to Processing
				ip.AdvanceState(PathState.Processing);

				// Call some callbacks
				// It needs to be stored in a local variable to avoid race conditions
				var tmpOnPathPreSearch = OnPathPreSearch;
				if (tmpOnPathPreSearch != null) tmpOnPathPreSearch(p);

				// Tick for when the path started, used for calculating how long time the calculation took
				long startTicks = System.DateTime.UtcNow.Ticks;
				long totalTicks = 0;

				ip.Prepare();

				// Check if the Prepare call caused the path to complete
				// If this happens the path usually failed
				if (!p.IsDone()) {
					// For debug uses, we set the last computed path to p, so we can view debug info on it in the editor (scene view).
					PathFindHelper.debugPathData = ip.PathHandler;
					PathFindHelper.debugPathID = p.pathID;

					// Initialize the path, now ready to begin search
					ip.Initialize();

					// The error can turn up in the Init function
					while (!p.IsDone()) {
						// Do some work on the path calculation.
						// The function will return when it has taken too much time
						// or when it has finished calculation
						ip.CalculateStep(targetTick);

						// If the path has finished calculation, we can break here directly instead of sleeping
						// Improves latency
						if (p.IsDone()) break;

						totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
						// Yield/sleep so other threads can work

						yield return null;

						startTicks = System.DateTime.UtcNow.Ticks;

						targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
					}

					totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
					p.duration = totalTicks*0.0001F;

					#if ProfileAstar
					System.Threading.Interlocked.Increment(ref AstarPath.PathsCompleted);
					#endif
				}

				// Cleans up node tagging and other things
				ip.Cleanup();


				// Call the immediate callback
				// It needs to be stored in a local variable to avoid race conditions
				var tmpImmediateCallback = p.immediateCallback;
				if (tmpImmediateCallback != null) tmpImmediateCallback(p);


				// It needs to be stored in a local variable to avoid race conditions
				var tmpOnPathPostSearch = OnPathPostSearch;
				if (tmpOnPathPostSearch != null) tmpOnPathPostSearch(p);


				// Push the path onto the return stack
				// It will be detected by the main Unity thread and returned as fast as possible (the next late update)
				returnQueue.Enqueue(p);

				ip.AdvanceState(PathState.ReturnQueue);

				// Wait a bit if we have calculated a lot of paths
				if (System.DateTime.UtcNow.Ticks > targetTick) {
					yield return null;
					targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
				}
			}
		}
	}
}
