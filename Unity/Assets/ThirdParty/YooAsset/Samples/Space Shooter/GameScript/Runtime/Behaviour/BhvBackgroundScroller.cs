using System;
using UnityEngine;

public class BhvBackgroundScroller : MonoBehaviour
{
	public float ScrollSpeed;
	public float TileSizeZ;

	private Vector3 _startPosition;

	void Start ()
	{
		_startPosition = transform.position;
	}

	void Update ()
	{
		float newPosition = Mathf.Repeat(Time.time * ScrollSpeed, TileSizeZ);
		this.transform.position = _startPosition + Vector3.forward * newPosition;
	}
}