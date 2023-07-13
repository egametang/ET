using UnityEngine;
using YooAsset;

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(GameObjectAssetReference), true)]
public class GameObjectAssetReferenceInspector : UnityEditor.Editor
{
	private bool _init = false;
	private GameObject _cacheObject;

	public override void OnInspectorGUI()
	{
		GameObjectAssetReference mono = (GameObjectAssetReference)target;

		if (_init == false)
		{
			_init = true;
			if (string.IsNullOrEmpty(mono.AssetGUID) == false)
			{
				string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(mono.AssetGUID);
				if (string.IsNullOrEmpty(assetPath) == false)
				{
					_cacheObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
				}
			}
		}

		GameObject go = (GameObject)UnityEditor.EditorGUILayout.ObjectField(_cacheObject, typeof(GameObject), false);
		if (go != _cacheObject)
		{
			_cacheObject = go;
			string assetPath = UnityEditor.AssetDatabase.GetAssetPath(go);
			mono.AssetGUID = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
			UnityEditor.EditorUtility.SetDirty(target);
		}

		UnityEditor.EditorGUILayout.LabelField("Asset GUID", mono.AssetGUID);
	}
}
#endif


public class GameObjectAssetReference : MonoBehaviour
{
	[HideInInspector]
	public string AssetGUID = "";

	private AssetOperationHandle _handle;

	public void Start()
	{
		var package = YooAssets.GetPackage("DefaultPackage");
		var assetInfo = package.GetAssetInfoByGUID(AssetGUID);
		_handle = package.LoadAssetAsync(assetInfo);
		_handle.Completed += Handle_Completed;
	}
	public void OnDestroy()
	{
		if (_handle != null)
		{
			_handle.Release();
			_handle = null;
		}
	}

	private void Handle_Completed(AssetOperationHandle handle)
	{
		if (handle.Status == EOperationStatus.Succeed)
		{
			handle.InstantiateSync(this.transform);
		}
	}
}