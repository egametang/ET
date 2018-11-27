using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding {
	/** GraphModifier is used for modifying graphs or processing graph data based on events.
	 * This class is a simple container for a number of events.
	 *
	 * \warning Some events will be called both in play mode <b>and in editor mode</b> (at least the scan events).
	 * So make sure your code handles both cases well. You may choose to ignore editor events.
	 * \see Application.IsPlaying
	 */
	[ExecuteInEditMode]
	public abstract class GraphModifier : VersionedMonoBehaviour {
		/** All active graph modifiers */
		private static GraphModifier root;

		private GraphModifier prev;
		private GraphModifier next;

		/** Unique persistent ID for this component, used for serialization */
		[SerializeField]
		[HideInInspector]
		protected ulong uniqueID;

		/** Maps persistent IDs to the component that uses it */
		protected static Dictionary<ulong, GraphModifier> usedIDs = new Dictionary<ulong, GraphModifier>();

		protected static List<T> GetModifiersOfType<T>() where T : GraphModifier {
			var current = root;
			var result = new List<T>();

			while (current != null) {
				var cast = current as T;
				if (cast != null) result.Add(cast);
				current = current.next;
			}
			return result;
		}

		public static void FindAllModifiers () {
			var allModifiers = FindObjectsOfType(typeof(GraphModifier)) as GraphModifier[];

			for (int i = 0; i < allModifiers.Length; i++) {
				if (allModifiers[i].enabled) allModifiers[i].OnEnable();
			}
		}

		/** GraphModifier event type */
		public enum EventType {
			PostScan = 1 << 0,
			PreScan = 1 << 1,
			LatePostScan = 1 << 2,
			PreUpdate = 1 << 3,
			PostUpdate = 1 << 4,
			PostCacheLoad = 1 << 5
		}

		/** Triggers an event for all active graph modifiers */
		public static void TriggerEvent (GraphModifier.EventType type) {
			if (!Application.isPlaying) {
				FindAllModifiers();
			}

			GraphModifier c = root;
			switch (type) {
			case EventType.PreScan:
				while (c != null) { c.OnPreScan(); c = c.next; }
				break;
			case EventType.PostScan:
				while (c != null) { c.OnPostScan(); c = c.next; }
				break;
			case EventType.LatePostScan:
				while (c != null) { c.OnLatePostScan(); c = c.next; }
				break;
			case EventType.PreUpdate:
				while (c != null) { c.OnGraphsPreUpdate(); c = c.next; }
				break;
			case EventType.PostUpdate:
				while (c != null) { c.OnGraphsPostUpdate(); c = c.next; }
				break;
			case EventType.PostCacheLoad:
				while (c != null) { c.OnPostCacheLoad(); c = c.next; }
				break;
			}
		}

		/** Adds this modifier to list of active modifiers */
		protected virtual void OnEnable () {
			RemoveFromLinkedList();
			AddToLinkedList();
			ConfigureUniqueID();
		}

		/** Removes this modifier from list of active modifiers */
		protected virtual void OnDisable () {
			RemoveFromLinkedList();
		}

		protected override void Awake () {
			base.Awake();
			ConfigureUniqueID();
		}

		void ConfigureUniqueID () {
			// Check if any other object is using the same uniqueID
			// In that case this object may have been duplicated
			GraphModifier usedBy;

			if (usedIDs.TryGetValue(uniqueID, out usedBy) && usedBy != this) {
				Reset();
			}

			usedIDs[uniqueID] = this;
		}

		void AddToLinkedList () {
			if (root == null) {
				root = this;
			} else {
				next = root;
				root.prev = this;
				root = this;
			}
		}

		void RemoveFromLinkedList () {
			if (root == this) {
				root = next;
				if (root != null) root.prev = null;
			} else {
				if (prev != null) prev.next = next;
				if (next != null) next.prev = prev;
			}
			prev = null;
			next = null;
		}

		protected virtual void OnDestroy () {
			usedIDs.Remove(uniqueID);
		}

		/** Called right after all graphs have been scanned.
		 * FloodFill and other post processing has not been done.
		 *
		 * \warning Since OnEnable and Awake are called roughly in the same time, the only way
		 * to ensure that these scripts get this call when scanning in Awake is to
		 * set the Script Execution Order for AstarPath to some time later than default time
		 * (see Edit -> Project Settings -> Script Execution Order).
		 * \todo Is this still relevant? A call to FindAllModifiers should have before this method is called
		 * so the above warning is probably not relevant anymore.
		 *
		 * \see OnLatePostScan
		 */
		public virtual void OnPostScan () {}

		/** Called right before graphs are going to be scanned.
		 *
		 * \warning Since OnEnable and Awake are called roughly in the same time, the only way
		 * to ensure that these scripts get this call when scanning in Awake is to
		 * set the Script Execution Order for AstarPath to some time later than default time
		 * (see Edit -> Project Settings -> Script Execution Order).
		 * \todo Is this still relevant? A call to FindAllModifiers should have before this method is called
		 * so the above warning is probably not relevant anymore.
		 *
		 * \see OnLatePostScan
		 * */
		public virtual void OnPreScan () {}

		/** Called at the end of the scanning procedure.
		 * This is the absolute last thing done by Scan.
		 *
		 */
		public virtual void OnLatePostScan () {}

		/** Called after cached graphs have been loaded.
		 * When using cached startup, this event is analogous to OnLatePostScan and implementing scripts
		 * should do roughly the same thing for both events.
		 */
		public virtual void OnPostCacheLoad () {}

		/** Called before graphs are updated using GraphUpdateObjects */
		public virtual void OnGraphsPreUpdate () {}

		/** Called after graphs have been updated using GraphUpdateObjects.
		 * Eventual flood filling has been done */
		public virtual void OnGraphsPostUpdate () {}

		void Reset () {
			// Create a new random 64 bit value (62 bit actually because we skip negative numbers, but that's still enough by a huge margin)
			var rnd1 = (ulong)Random.Range(0, int.MaxValue);
			var rnd2 = ((ulong)Random.Range(0, int.MaxValue) << 32);

			uniqueID = rnd1 | rnd2;
			usedIDs[uniqueID] = this;
		}
	}
}
