//#define ASTAR_NO_POOLING // Disable pooling for some reason. Maybe for debugging or just for measuring the difference.
using System;
using System.Collections.Generic;

namespace PF {
	/** Pools path objects to reduce load on the garbage collector */
	public static class PathPool {
		static readonly Dictionary<Type, Stack<Path> > pool = new Dictionary<Type, Stack<Path> >();
		static readonly Dictionary<Type, int> totalCreated = new Dictionary<Type, int>();

		/** Adds a path to the pool.
		 * This function should not be used directly. Instead use the Path.Claim and Path.Release functions.
		 */
		public static void Pool (Path path) {
			#if !ASTAR_NO_POOLING
			lock (pool) {
				if (((IPathInternals)path).Pooled) {
					throw new System.ArgumentException("The path is already pooled.");
				}

				Stack<Path> poolStack;
				if (!pool.TryGetValue(path.GetType(), out poolStack)) {
					poolStack = new Stack<Path>();
					pool[path.GetType()] = poolStack;
				}

				((IPathInternals)path).Pooled = true;
				((IPathInternals)path).OnEnterPool();
				poolStack.Push(path);
			}
			#endif
		}

		/** Total created instances of paths of the specified type */
		public static int GetTotalCreated (Type type) {
			int created;

			if (totalCreated.TryGetValue(type, out created)) {
				return created;
			} else {
				return 0;
			}
		}

		/** Number of pooled instances of a path of the specified type */
		public static int GetSize (Type type) {
			Stack<Path> poolStack;
			if (pool.TryGetValue(type, out poolStack)) {
				return poolStack.Count;
			} else {
				return 0;
			}
		}

		/** Get a path from the pool or create a new one if the pool is empty */
		public static T GetPath<T>() where T : Path, new() {
			#if ASTAR_NO_POOLING
			T result = new T();
			((IPathInternals)result).Reset();
			return result;
			#else
			lock (pool) {
				T result;
				Stack<Path> poolStack;
				if (pool.TryGetValue(typeof(T), out poolStack) && poolStack.Count > 0) {
					// Guaranteed to have the correct type
					result = poolStack.Pop() as T;
				} else {
					result = new T();

					// Make sure an entry for the path type exists
					if (!totalCreated.ContainsKey(typeof(T))) {
						totalCreated[typeof(T)] = 0;
					}

					totalCreated[typeof(T)]++;
				}

				((IPathInternals)result).Pooled = false;
				((IPathInternals)result).Reset();
				return result;
			}
			#endif
		}
	}

	/** Pools path objects to reduce load on the garbage collector.
	 * \deprecated Generic version is now obsolete to trade an extremely tiny performance decrease for a large decrease in boilerplate for Path classes
	 */
	[System.Obsolete("Generic version is now obsolete to trade an extremely tiny performance decrease for a large decrease in boilerplate for Path classes")]
	public static class PathPool<T> where T : Path, new() {
		/** Recycles a path and puts in the pool.
		 * This function should not be used directly. Instead use the Path.Claim and Path.Release functions.
		 */
		public static void Recycle (T path) {
			PathPool.Pool(path);
		}

		/** Warms up path, node list and vector list pools.
		 * Makes sure there is at least \a count paths, each with a minimum capacity for paths with length \a length in the pool.
		 * The capacity means that paths shorter or equal to the capacity can be calculated without any large allocations taking place.
		 */
		public static void Warmup (int count, int length) {
			ListPool<GraphNode>.Warmup(count, length);
			ListPool<Vector3>.Warmup(count, length);

			var tmp = new Path[count];
			for (int i = 0; i < count; i++) { tmp[i] = GetPath(); tmp[i].Claim(tmp); }
			for (int i = 0; i < count; i++) tmp[i].Release(tmp);
		}

		public static int GetTotalCreated () {
			return PathPool.GetTotalCreated(typeof(T));
		}

		public static int GetSize () {
			return PathPool.GetSize(typeof(T));
		}

		[System.Obsolete("Use PathPool.GetPath<T> instead of PathPool<T>.GetPath")]
		public static T GetPath () {
			return PathPool.GetPath<T>();
		}
	}
}
