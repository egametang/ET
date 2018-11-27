#if !UNITY_EDITOR
// Extra optimizations when not running in the editor, but less error checking
#define ASTAR_OPTIMIZE_POOLING
#endif

using System;
using System.Collections.Generic;

namespace PF {
	public interface IAstarPooledObject {
		void OnEnterPool ();
	}

	/** Lightweight object Pool for IAstarPooledObject.
	 * Handy class for pooling objects of type T which implements the IAstarPooledObject interface.
	 *
	 * Usage:
	 * - Claim a new object using \code SomeClass foo = ObjectPool<SomeClass>.Claim (); \endcode
	 * - Use it and do stuff with it
	 * - Release it with \code ObjectPool<SomeClass>.Release (foo); \endcode
	 *
	 * After you have released a object, you should never use it again.
	 *
	 * \since Version 3.2
	 * \version Since 3.7.6 this class is thread safe
	 * \see Pathfinding.Util.ListPool
	 * \see ObjectPoolSimple
	 */
	public static class ObjectPool<T> where T : class, IAstarPooledObject, new(){
		public static T Claim () {
			return ObjectPoolSimple<T>.Claim();
		}

		public static void Release (ref T obj) {
			// obj will be set to null so we need to copy the reference
			var tmp = obj;

			ObjectPoolSimple<T>.Release(ref obj);
			tmp.OnEnterPool();
		}
	}

	/** Lightweight object Pool.
	 * Handy class for pooling objects of type T.
	 *
	 * Usage:
	 * - Claim a new object using \code SomeClass foo = ObjectPool<SomeClass>.Claim (); \endcode
	 * - Use it and do stuff with it
	 * - Release it with \code ObjectPool<SomeClass>.Release (foo); \endcode
	 *
	 * After you have released a object, you should never use it again.
	 *
	 * \since Version 3.2
	 * \version Since 3.7.6 this class is thread safe
	 * \see Pathfinding.Util.ListPool
	 * \see ObjectPool
	 */
	public static class ObjectPoolSimple<T> where T : class, new(){
		/** Internal pool */
		static List<T> pool = new List<T>();

#if !ASTAR_NO_POOLING
		static readonly HashSet<T> inPool = new HashSet<T>();
#endif

		/** Claim a object.
		 * Returns a pooled object if any are in the pool.
		 * Otherwise it creates a new one.
		 * After usage, this object should be released using the Release function (though not strictly necessary).
		 */
		public static T Claim () {
#if ASTAR_NO_POOLING
			return new T();
#else
			lock (pool) {
				if (pool.Count > 0) {
					T ls = pool[pool.Count-1];
					pool.RemoveAt(pool.Count-1);
					inPool.Remove(ls);
					return ls;
				} else {
					return new T();
				}
			}
#endif
		}

		/** Releases an object.
		 * After the object has been released it should not be used anymore.
		 * The variable will be set to null to prevent silly mistakes.
		 *
		 * \throws System.InvalidOperationException
		 * Releasing an object when it has already been released will cause an exception to be thrown.
		 * However enabling ASTAR_OPTIMIZE_POOLING will prevent this check.
		 *
		 * \see Claim
		 */
		public static void Release (ref T obj) {
#if !ASTAR_NO_POOLING
			lock (pool) {
#if !ASTAR_OPTIMIZE_POOLING
				if (!inPool.Add(obj)) {
					throw new InvalidOperationException("You are trying to pool an object twice. Please make sure that you only pool it once.");
				}
#endif
				pool.Add(obj);
			}
#endif
			obj = null;
		}

		/** Clears the pool for objects of this type.
		 * This is an O(n) operation, where n is the number of pooled objects.
		 */
		public static void Clear () {
			lock (pool) {
#if !ASTAR_OPTIMIZE_POOLING && !ASTAR_NO_POOLING
				inPool.Clear();
#endif
				pool.Clear();
			}
		}

		/** Number of objects of this type in the pool */
		public static int GetSize () {
			return pool.Count;
		}
	}
}
