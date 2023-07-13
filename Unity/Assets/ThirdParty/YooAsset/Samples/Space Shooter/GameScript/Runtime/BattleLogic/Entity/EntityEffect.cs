using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Pooling;

public class EntityEffect : MonoBehaviour
{
	public float DelayDestroyTime = 1f;

	private SpawnHandle _handle;

	public void InitEntity(SpawnHandle handle)
	{
		_handle = handle;

		Invoke(nameof(DelayDestroy), DelayDestroyTime);
	}
	private void DelayDestroy()
	{
		_handle.Restore();
		_handle = null;
	}
}