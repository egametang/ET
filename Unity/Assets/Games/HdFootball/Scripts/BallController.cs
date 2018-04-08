using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour {

	/// <summary>
	/// This is the main ball controller. It handles all ball status including force management,
	/// movements, collisions, sounds, visual effects, etc.
	/// </summary>

	private GameObject gc;						//Reference to GameController Object
	private float ballMaxSpeed = 10.0f;			//Ball should not move faster than this value

	public AudioClip ballHitGround;				//Audio when ball hits floor
	public AudioClip ballHitMiddlePole;			//Audio when ball hits the pole
	public AudioClip[] ballHitHead;				//Audio when ball hits the heads

	public Vector3[] ballStartingPosition;		//initial position of the ball

	private GameObject player;					//Reference to Player Object
	private GameObject cpu;						//Reference to Cpu Object

	public GameObject ballSpeedDebug;			//debug object to show ball's speed at all times
	public GameObject hitEffect;				//visual effect for contact points
	public GameObject ballShadow;				//shadow object that follows the ball



	void Awake () {
		//find and cache references to important gameobjects
		player			 = GameObject.FindGameObjectWithTag("PlayerHead");
		cpu				 = GameObject.FindGameObjectWithTag("CpuHead");
		gc				 = GameObject.FindGameObjectWithTag("GameController");
	}


	void Start () {
		//set ball starting position
		transform.position = ballStartingPosition[0];
	}
	

	void FixedUpdate () {

		//never allow the ball to get stuck at the two ends of the screen.
		escapeLimits();

		//move ball's shadow object
		manageBallShadow();

		//debug - show ball's speed
		if(ballSpeedDebug)
			ballSpeedDebug.GetComponent<TextMesh>().text = "Speed: " + GetComponent<Rigidbody>().velocity.magnitude.ToString();

		//limit ball's maximum spped
		if(GetComponent<Rigidbody>().velocity.magnitude > ballMaxSpeed)
			GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * ballMaxSpeed;
	}


	/// <summary>
	/// Make shadow object follow ball's movements
	/// </summary>
	void manageBallShadow() {
		if(!ballShadow)
			return;

		ballShadow.transform.position = new Vector3(transform.position.x, -1, 0);
		ballShadow.transform.localScale = new Vector3(1.5f, 0.75f, 0.001f);
	}


	/// <summary>
	/// never allow the ball to get stuck at the two ends of the screen.
	/// we do this by applying a small force to move the ball in another direction.
	/// Screen boundaries are hardcoded and must be updated to match your own design.
	/// </summary>
	void escapeLimits() {
		if(transform.position.x <= -8.4f)
			GetComponent<Rigidbody>().AddForce(new Vector3(3, 0, 0), ForceMode.Impulse);
		if(transform.position.x >= 8.4f)
			GetComponent<Rigidbody>().AddForce(new Vector3(-3, 0, 0), ForceMode.Impulse);
	}


	/// <summary>
	/// Manages collition events
	/// </summary>
	void OnCollisionEnter (Collision other) {
		
		if(other.gameObject.tag == "Field") {
			playSfx(ballHitGround);
			createHitGfx();
			checkGoal();
		}

		if(other.gameObject.tag == "MiddlePole") {
			playSfx(ballHitMiddlePole);
			//if the ball is going to get stuck on the pole, move it by applying a small force
			if(transform.position.x <= 0.12f || transform.position.x >= -0.12f)
				GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1.0f, 1.0f), 0, 0), ForceMode.Impulse);
		}

		if(other.gameObject.tag == "PlayerHead") {
			playSfx(ballHitHead[Random.Range(0, ballHitHead.Length)]);
			StartCoroutine (other.gameObject.GetComponent<PlayerController>().changeFaceStatus());
			createHitGfx();
		}

		if(other.gameObject.tag == "CpuHead") {
			playSfx(ballHitHead[Random.Range(0, ballHitHead.Length)]);
			StartCoroutine (other.gameObject.GetComponent<CpuController>().changeFaceStatus());
			createHitGfx();
		}
	}


	/// <summary>
	/// Creates a small visual object to show the contact point between ball and other objects
	/// </summary>
	void createHitGfx() {
		GameObject hitGfx = Instantiate(hitEffect, 
					                    transform.position + new Vector3(0, -0.4f, -1), 
					                    Quaternion.Euler(0, 180, 0)) as GameObject;
		hitGfx.name = "hitGfx";
	}


	/// <summary>
	/// Check if a goal happened.
	/// </summary>
	void checkGoal() {

		//it the time is up, this goal is not accepted.
		if(GameController.gameIsFinished)
			return;

		if(transform.position.x < 0) {
			//ball has landed in the left field. we received a goal :(
			StartCoroutine(resetBallPosition(0));
			StartCoroutine(gc.GetComponent<GameController>().manageGoalEvent(0));

		} else if(transform.position.x > 0) {
			//ball has landed in the right field. we scored a goal :)
			StartCoroutine(resetBallPosition(1));
			StartCoroutine(gc.GetComponent<GameController>().manageGoalEvent(1));
		}

		player.GetComponent<PlayerController>().resetPosition();				//reset player position
		StartCoroutine(cpu.GetComponent<CpuController>().resetPosition());		//reset cpu position
	}


	/// <summary>
	/// Move the ball to the starting position after a goal happened
	/// </summary>
	public IEnumerator resetBallPosition(int _posIndex) {

		//give the ball to the player or the cpu if they have received a goal
		if(_posIndex == 1) {
			transform.position = ballStartingPosition[1];
		} else {
			transform.position = ballStartingPosition[0];
		}

		//freeze the ball for a while
		GetComponent<Rigidbody>().Sleep();
		GetComponent<Rigidbody>().isKinematic = true;

		yield return new WaitForSeconds(1.0f);

		//unfreeze the ball
		//GetComponent<Rigidbody>().isKinematic = false;
	}


	//*****************************************************************************
	// Play sound clips
	//*****************************************************************************
	void playSfx ( AudioClip _clip  ){
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().Play();
		}
	}
}