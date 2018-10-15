using PF;
using UnityEngine;

namespace Pathfinding {
	/** Base for all path modifiers.
	 * \see MonoModifier
	 * Modifier */
	public interface IPathModifier {
		int Order { get; }

		void Apply (Path path);
		void PreProcess (Path path);
	}

	/** Base class for path modifiers which are not attached to GameObjects.
	 * \see MonoModifier */
	[System.Serializable]
	public abstract class PathModifier : IPathModifier {
		[System.NonSerialized]
		public Seeker seeker;

		/** Modifiers will be executed from lower order to higher order.
		 * This value is assumed to stay constant.
		 */
		public abstract int Order { get; }

		public void Awake (Seeker seeker) {
			this.seeker = seeker;
			if (seeker != null) {
				seeker.RegisterModifier(this);
			}
		}

		public void OnDestroy (Seeker seeker) {
			if (seeker != null) {
				seeker.DeregisterModifier(this);
			}
		}

		public virtual void PreProcess (Path path) {
			// Required by IPathModifier
		}

		/** Main Post-Processing function */
		public abstract void Apply (Path path);
	}

	/** Base class for path modifiers which can be attached to GameObjects.
	 * \see Menubar -> Component -> Pathfinding -> Modifiers
	 */
	[System.Serializable]
	public abstract class MonoModifier : VersionedMonoBehaviour, IPathModifier {
		[System.NonSerialized]
		public Seeker seeker;

		/** Alerts the Seeker that this modifier exists */
		protected virtual void OnEnable () {
			seeker = GetComponent<Seeker>();

			if (seeker != null) {
				seeker.RegisterModifier(this);
			}
		}

		protected virtual void OnDisable () {
			if (seeker != null) {
				seeker.DeregisterModifier(this);
			}
		}

		/** Modifiers will be executed from lower order to higher order.
		 * This value is assumed to stay constant.
		 */
		public abstract int Order { get; }

		public virtual void PreProcess (Path path) {
			// Required by IPathModifier
		}

		/** Called for each path that the Seeker calculates after the calculation has finished */
		public abstract void Apply (Path path);
	}
}
