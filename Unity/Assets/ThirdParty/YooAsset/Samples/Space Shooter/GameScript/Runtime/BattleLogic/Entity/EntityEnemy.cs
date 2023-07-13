using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Pooling;
using Random = UnityEngine.Random;

public class EntityEnemy : MonoBehaviour
{
	private const float Dodge = 5f;
	private const float Smoothing = 7.5f;

	public RoomBoundary Boundary;
	public float MoveSpeed = 20f;
	public float FireInterval = 2f;

	public Vector2 StartWait = new Vector2(0.5f, 1f);
	public Vector2 ManeuverTime = new Vector2(1, 2);
	public Vector2 ManeuverWait = new Vector2(1, 2);

	private SpawnHandle _handle;
	private Transform _shotSpawn;
	private Rigidbody _rigidbody;
	private AudioSource _audioSource;
	private float _lastFireTime = 0f;
	private float _currentSpeed;
	private float _targetManeuver;


	public void InitEntity(SpawnHandle handle)
	{
		_handle = handle;

		_rigidbody.velocity = this.transform.forward * -5f;
		_lastFireTime = Time.time;
		_currentSpeed = _rigidbody.velocity.z;
		_targetManeuver = 0f;
		StartCoroutine(Evade());
	}

	void Awake()
	{
		_rigidbody = this.gameObject.GetComponent<Rigidbody>();
		_audioSource = this.gameObject.GetComponent<AudioSource>();
		_shotSpawn = this.transform.Find("shot_spawn");
	}
	void Update()
	{
		if (Time.time - _lastFireTime >= FireInterval)
		{
			_lastFireTime = Time.time;
			_audioSource.Play();
			BattleEventDefine.EnemyFireBullet.SendEventMessage(_shotSpawn.position, _shotSpawn.rotation);
		}
	}
	void FixedUpdate()
	{
		float newManeuver = Mathf.MoveTowards(_rigidbody.velocity.x, _targetManeuver, Smoothing * Time.deltaTime);
		_rigidbody.velocity = new Vector3(newManeuver, 0.0f, _currentSpeed);
		_rigidbody.position = new Vector3
		(
			Mathf.Clamp(_rigidbody.position.x, Boundary.xMin, Boundary.xMax),
			0.0f,
			Mathf.Clamp(_rigidbody.position.z, Boundary.zMin, Boundary.zMax)
		);

		float tilt = 10f;
		_rigidbody.rotation = Quaternion.Euler(0, 0, _rigidbody.velocity.x * -tilt);
	}
	void OnTriggerEnter(Collider other)
	{
		var name = other.gameObject.name;
		if (name.StartsWith("player"))
		{
			BattleEventDefine.EnemyDead.SendEventMessage(this.transform.position, this.transform.rotation);
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

	IEnumerator Evade()
	{
		yield return new WaitForSeconds(Random.Range(StartWait.x, StartWait.y));
		while (true)
		{
			_targetManeuver = Random.Range(1, Dodge) * -Mathf.Sign(transform.position.x);
			yield return new WaitForSeconds(Random.Range(ManeuverTime.x, ManeuverTime.y));
			_targetManeuver = 0;
			yield return new WaitForSeconds(Random.Range(ManeuverWait.x, ManeuverWait.y));
		}
	}
}