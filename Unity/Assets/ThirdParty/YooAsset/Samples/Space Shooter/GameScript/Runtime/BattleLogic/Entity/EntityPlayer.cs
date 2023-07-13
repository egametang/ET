using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Pooling;

public class EntityPlayer : MonoBehaviour
{	
	public RoomBoundary Boundary;
	public float MoveSpeed = 10f;
	public float FireRate = 0.25f;

	private SpawnHandle _handle;
	private float _nextFireTime = 0f;
	private Transform _shotSpawn;
	private Rigidbody _rigidbody;
	private AudioSource _audioSource;

	public void InitEntity(SpawnHandle handle)
	{
		_handle = handle;
	}

	void Awake()
	{
		_rigidbody = this.gameObject.GetComponent<Rigidbody>();
		_audioSource = this.gameObject.GetComponent<AudioSource>();
		_shotSpawn = this.transform.Find("shot_spawn");
	}
	void Update()
	{
		if (Input.GetButton("Fire1") && Time.time > _nextFireTime)
		{
			_nextFireTime = Time.time + FireRate;
			_audioSource.Play();
			BattleEventDefine.PlayerFireBullet.SendEventMessage(_shotSpawn.position, _shotSpawn.rotation);
		}
	}
	void FixedUpdate()
	{
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");

		Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
		_rigidbody.velocity = movement * MoveSpeed;
		_rigidbody.position = new Vector3
		(
			Mathf.Clamp(GetComponent<Rigidbody>().position.x, Boundary.xMin, Boundary.xMax),
			0.0f,
			Mathf.Clamp(GetComponent<Rigidbody>().position.z, Boundary.zMin, Boundary.zMax)
		);

		float tilt = 5f;
		_rigidbody.rotation = Quaternion.Euler(0.0f, 0.0f, _rigidbody.velocity.x * -tilt);
	}
	void OnTriggerEnter(Collider other)
	{
		var name = other.gameObject.name;
		if (name.StartsWith("enemy") || name.StartsWith("asteroid"))
		{
			BattleEventDefine.PlayerDead.SendEventMessage(this.transform.position, this.transform.rotation);
			_handle.Restore();
			_handle = null;
		}
	}
}