using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Pooling;

public class EntityBullet : MonoBehaviour
{
	public float MoveSpeed = 20f;
	public float DelayDestroyTime = 5f;

	private SpawnHandle _handle;
	private Rigidbody _rigidbody;

	public void InitEntity(SpawnHandle handle)
	{
		_handle = handle;
		_rigidbody.velocity = this.transform.forward * MoveSpeed;
	}

	void Awake()
	{
		_rigidbody = this.transform.GetComponent<Rigidbody>();
	}
	void OnTriggerEnter(Collider other)
	{
		var name = other.gameObject.name;
		if (name.StartsWith("Boundary"))
			return;

		var goName = this.gameObject.name;
		if (goName.StartsWith("enemy_bullet"))
		{
			if (name.StartsWith("enemy") == false)
			{
				_handle.Restore();
				_handle = null;
			}
		}

		if (goName.StartsWith("player_bullet"))
		{
			if (name.StartsWith("player") == false)
			{
				_handle.Restore();
				_handle = null;
			}
		}
	}
	void OnTriggerExit(Collider other)
	{
		var name = other.gameObject.name;
		if (name.StartsWith("Boundary"))
		{
			_handle.Restore();
			_handle = null;
		}
	}
}