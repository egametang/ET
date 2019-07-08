using UnityEngine;

namespace Pathfinding {
	/** Exposes internal methods from #Pathfinding.VersionedMonoBehaviour */
	public interface IVersionedMonoBehaviourInternal {
		int OnUpgradeSerializedData (int version, bool unityThread);
	}

	/** Base class for all components in the package */
	public abstract class VersionedMonoBehaviour : MonoBehaviour, ISerializationCallbackReceiver, IVersionedMonoBehaviourInternal {
		/** Version of the serialized data. Used for script upgrades. */
		[SerializeField]
		[HideInInspector]
		int version = 0;

		protected virtual void Awake () {
			// Make sure the version field is up to date for components created during runtime.
			// Reset is not called when in play mode.
			// If the data had to be upgraded then OnAfterDeserialize would have been called earlier.
			if (Application.isPlaying) version = OnUpgradeSerializedData(int.MaxValue, true);
		}

		/** Handle serialization backwards compatibility */
		void Reset () {
			// Set initial version when adding the component for the first time
			version = OnUpgradeSerializedData(int.MaxValue, true);
		}

		/** Handle serialization backwards compatibility */
		void ISerializationCallbackReceiver.OnBeforeSerialize () {
		}

		/** Handle serialization backwards compatibility */
		void ISerializationCallbackReceiver.OnAfterDeserialize () {
			version = OnUpgradeSerializedData(version, false);
		}

		/** Handle serialization backwards compatibility */
		protected virtual int OnUpgradeSerializedData (int version, bool unityThread) {
			return 1;
		}

		int IVersionedMonoBehaviourInternal.OnUpgradeSerializedData (int version, bool unityThread) {
			return OnUpgradeSerializedData(version, unityThread);
		}
	}
}
