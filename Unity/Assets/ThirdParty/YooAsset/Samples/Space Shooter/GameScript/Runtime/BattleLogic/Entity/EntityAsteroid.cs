using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Pooling;

public class EntityAsteroid : MonoBehaviour
{
	public float MoveSpeed = -5f;
	public float Tumble = 5f;

	private SpawnHandle _handle;
	private Rigidbody _rigidbody;

	public void InitEntity(SpawnHandle handle)
	{
		_handle = handle;

		_rigidbody.velocity = this.transform.forward * MoveSpeed;
		_rigidbody.angularVelocity = Random.insideUnitSphere * Tumble;
	}

	void Awake()
	{
		_rigidbody = this.transform.GetComponent<Rigidbody>();
	}
	void OnTriggerEnter(Collider other)
	{
		var name = other.gameObject.name;
		if (name.StartsWith("player"))
		{
			BattleEventDefine.AsteroidExplosion.SendEventMessage(this.transform.position, this.transform.rotation);
			_handle.Restore();
			_handle = null;
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