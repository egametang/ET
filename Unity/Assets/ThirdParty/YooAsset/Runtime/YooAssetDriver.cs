using UnityEngine;

namespace YooAsset
{
	internal class YooAssetDriver : MonoBehaviour
	{
		void Update()
		{
			YooAssets.InternalUpdate();
		}

		void OnApplicationQuit()
		{
			YooAssets.InternalDestroy();
		}
	}
}