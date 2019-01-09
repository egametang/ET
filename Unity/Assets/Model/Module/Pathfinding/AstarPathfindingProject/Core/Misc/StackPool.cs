//#define ASTAR_NO_POOLING //@SHOWINEDITOR Disable pooling for some reason. Could be debugging or just for measuring the difference.

using System.Collections.Generic;

namespace Pathfinding.Util {
	/** Lightweight Stack Pool.
	 * Handy class for pooling stacks of type T.
	 *
	 * Usage:
	 * - Claim a new stack using \code Stack<SomeClass> foo = StackPool<SomeClass>.Claim (); \endcode
	 * - Use it and do stuff with it
	 * - Release it with \code StackPool<SomeClass>.Release (foo); \endcode
	 *
	 * You do not need to clear the stack before releasing it.
	 * After you have released a stack, you should never use it again.
	 *
	 * \warning This class is not thread safe
	 *
	 * \since Version 3.2
	 * \see Pathfinding.Util.ListPool
	 */
	public static class StackPool<T>{
		/** Internal pool */
		static readonly List<Stack<T> > pool;

		/** Static constructor */
		static StackPool () {
			pool = new List<Stack<T> >();
		}

		/** Claim a stack.
		 * Returns a pooled stack if any are in the pool.
		 * Otherwise it creates a new one.
		 * After usage, this stack should be released using the Release function (though not strictly necessary).
		 */
		public static Stack<T> Claim () {
#if ASTAR_NO_POOLING
			return new Stack<T>();
#else
			if (pool.Count > 0) {
				Stack<T> ls = pool[pool.Count-1];
				pool.RemoveAt(pool.Count-1);
				return ls;
			}

			return new Stack<T>();
#endif
		}

		/** Makes sure the pool contains at least \a count pooled items.
		 * This is good if you want to do all allocations at start.
		 */
		public static void Warmup (int count) {
			var tmp = new Stack<T>[count];

			for (int i = 0; i < count; i++) tmp[i] = Claim();
			for (int i = 0; i < count; i++) Release(tmp[i]);
		}

		/** Releases a stack.
		 * After the stack has been released it should not be used anymore.
		 * Releasing a stack twice will cause an error.
		 */
		public static void Release (Stack<T> stack) {
#if !ASTAR_NO_POOLING
			for (int i = 0; i < pool.Count; i++)
				if (pool[i] == stack) UnityEngine.Debug.LogError("The Stack is released even though it is inside the pool");

			stack.Clear();
			pool.Add(stack);
#endif
		}

		/** Clears all pooled stacks of this type.
		 * This is an O(n) operation, where n is the number of pooled stacks
		 */
		public static void Clear () {
			pool.Clear();
		}

		/** Number of stacks of this type in the pool */
		public static int GetSize () {
			return pool.Count;
		}
	}
}
