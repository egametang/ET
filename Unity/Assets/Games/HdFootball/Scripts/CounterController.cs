using UnityEngine;
using System.Collections;

public class CounterController : MonoBehaviour {

	/// <summary>
	/// The counter prefab.
	/// It is used in the first run of the game. counts from 3 to 1 and then dissapears.
	/// </summary>

	public Texture2D[] number;			//available number images
	private bool runOnce = false;		//flag for playing the countdown just once

	private Vector3 startingScale = new Vector3(2, 2.65f, 0.001f);
	private Vector3 targetScale = new Vector3(3, 4, 0.001f);


	void Awake () {
		//init
		GetComponent<Renderer>().material.mainTexture = number[0];
		transform.localScale = startingScale;
	}
	

	void Update () {
		if(!runOnce)
			StartCoroutine(countdown());
	}


	/// <summary>
	/// Countdown from 3 to 1.
	/// </summary>
	IEnumerator countdown() {
		runOnce = true;

		StartCoroutine(animate());
		yield return new WaitForSeconds(GameController.startDelay / 3);

		transform.localScale = startingScale;
		StartCoroutine(animate());
		GetComponent<Renderer>().material.mainTexture = number[1];
		yield return new WaitForSeconds(GameController.startDelay / 3);

		transform.localScale = startingScale;
		StartCoroutine(animate());
		GetComponent<Renderer>().material.mainTexture = number[2];
		yield return new WaitForSeconds(GameController.startDelay / 3);

		//start the game
		Destroy (gameObject);
	}


	/// <summary>
	/// changes the scale of the given object over time
	/// </summary>
	IEnumerator animate() {
		float t = 0;
		while(t < 1) {
			t += Time.deltaTime * 1;
			transform.localScale = new Vector3(Mathf.SmoothStep(startingScale.x, targetScale.x, t),
			                                   Mathf.SmoothStep(startingScale.y, targetScale.y, t),
			                                   0.001f);
			yield return 0;
		}
	}
}
