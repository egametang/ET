#if !UNITY_EDITOR
// Extra optimizations when not running in the editor, but less error checking
#define ASTAR_OPTIMIZE_POOLING
#endif

using System;
using System.Collections.Generic;

namespace PF {
	/** Lightweight Array Pool.
	 * Handy class for pooling arrays of type T.
	 *
	 * Usage:
	 * - Claim a new array using \code SomeClass[] foo = ArrayPool<SomeClass>.Claim (capacity); \endcode
	 * - Use it and do stuff with it
	 * - Release it with \code ArrayPool<SomeClass>.Release (foo); \endcode
	 *
	 * \warning Arrays returned from the Claim method may contain arbitrary data.
	 *  You cannot rely on it being zeroed out.
	 *
	 * After you have released a array, you should never use it again, if you do use it
	 * your code may modify it at the same time as some other code is using it which
	 * will likely lead to bad results.
	 *
	 * \since Version 3.8.6
	 * \see Pathfinding.Util.ListPool
	 */
	public static class ArrayPool<T>{
#if !ASTAR_NO_POOLING
		/** Maximum length of an array pooled using ClaimWithExactLength.
		 * Arrays with lengths longer than this will silently not be pooled.
		 */
		const int MaximumExactArrayLength = 256;

		/** Internal pool.
		 * The arrays in each bucket have lengths of 2^i
		 */
		static readonly Stack<T[]>[] pool = new Stack<T[]>[31];
		static readonly Stack<T[]>[] exactPool = new Stack<T[]>[MaximumExactArrayLength+1];
		static readonly HashSet<T[]> inPool = new HashSet<T[]>();
#endif

		/** Returns an array with at least the specified length */
		public static T[] Claim (int minimumLength) {
			if (minimumLength <= 0) {
				return ClaimWithExactLength(0);
			}

			int bucketIndex = 0;
			while ((1 << bucketIndex) < minimumLength && bucketIndex < 30) {
				bucketIndex++;
			}

			if (bucketIndex == 30)
				throw new System.ArgumentException("Too high minimum length");

#if !ASTAR_NO_POOLING
			lock (pool) {
				if (pool[bucketIndex] == null) {
					pool[bucketIndex] = new Stack<T[]>();
				}

				if (pool[bucketIndex].Count > 0) {
					var array = pool[bucketIndex].Pop();
					inPool.Remove(array);
					return array;
				}
			}
#endif
			return new T[1 << bucketIndex];
		}

		/** Returns an array with the specified length.
		 * Use with caution as pooling too many arrays with different lengths that
		 * are rarely being reused will lead to an effective memory leak.
		 *
		 * Use #Claim if you just need an array that is at least as large as some value.
		 */
		public static T[] ClaimWithExactLength (int length) {
#if !ASTAR_NO_POOLING
			bool isPowerOfTwo = length != 0 && (length & (length - 1)) == 0;
			if (isPowerOfTwo) {
				// Will return the correct array length
				return Claim(length);
			}

			if (length <= MaximumExactArrayLength) {
				lock (pool) {
					Stack<T[]> stack = exactPool[length];
					if (stack != null && stack.Count > 0) {
						var array = stack.Pop();
#if !ASTAR_OPTIMIZE_POOLING
						inPool.Remove(array);
#endif
						return array;
					}
				}
			}
#endif
			return new T[length];
		}

		/** Pool an array.
		 * If the array was got using the #ClaimWithExactLength method then the \a allowNonPowerOfTwo parameter must be set to true.
		 * The parameter exists to make sure that non power of two arrays are not pooled unintentionally which could lead to memory leaks.
		 */
		public static void Release (ref T[] array, bool allowNonPowerOfTwo = false) {
			if (array == null) return;
			if (array.GetType() != typeof(T[])) {
				throw new System.ArgumentException("Expected array type " + typeof(T[]).Name + " but found " + array.GetType().Name + "\nAre you using the correct generic class?\n");
			}

#if !ASTAR_NO_POOLING
			bool isPowerOfTwo = array.Length != 0 && (array.Length & (array.Length - 1)) == 0;
			if (!isPowerOfTwo && !allowNonPowerOfTwo && array.Length != 0) throw new System.ArgumentException("Length is not a power of 2");

			lock (pool) {
#if !ASTAR_OPTIMIZE_POOLING
				if (!inPool.Add(array)) {
					throw new InvalidOperationException("You are trying to pool an array twice. Please make sure that you only pool it once.");
				}
#endif
				if (isPowerOfTwo) {
					int bucketIndex = 0;
					while ((1 << bucketIndex) < array.Length && bucketIndex < 30) {
						bucketIndex++;
					}

					if (pool[bucketIndex] == null) {
						pool[bucketIndex] = new Stack<T[]>();
					}

					pool[bucketIndex].Push(array);
				} else if (array.Length <= MaximumExactArrayLength) {
					Stack<T[]> stack = exactPool[array.Length];
					if (stack == null) stack = exactPool[array.Length] = new Stack<T[]>();
					stack.Push(array);
				}
			}
#endif
			array = null;
		}
	}

	/** Extension methods for List<T> */
	public static class ListExtensions {
		/** Identical to ToArray but it uses ArrayPool<T> to avoid allocations if possible.
		 *
		 * Use with caution as pooling too many arrays with different lengths that
		 * are rarely being reused will lead to an effective memory leak.
		 */
		public static T[] ToArrayFromPool<T>(this List<T> list) {
			var arr = ArrayPool<T>.ClaimWithExactLength(list.Count);

			for (int i = 0; i < arr.Length; i++) {
				arr[i] = list[i];
			}
			return arr;
		}

		/** Clear a list faster than List<T>.Clear.
		 * It turns out that the List<T>.Clear method will clear all elements in the underlaying array
		 * not just the ones up to Count. If the list only has a few elements, but the capacity
		 * is huge, this can cause performance problems. Using the RemoveRange method to remove
		 * all elements in the list does not have this problem, however it is implemented in a
		 * stupid way, so it will clear the elements twice (completely unnecessarily) so it will
		 * only be faster than using the Clear method if the number of elements in the list is
		 * less than half of the capacity of the list.
		 *
		 * Hopefully this method can be removed when Unity upgrades to a newer version of Mono.
		 */
		public static void ClearFast<T>(this List<T> list) {
			if (list.Count*2 < list.Capacity) {
				list.RemoveRange(0, list.Count);
			} else {
				list.Clear();
			}
		}
	}
}
