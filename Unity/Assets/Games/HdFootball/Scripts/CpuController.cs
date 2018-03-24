using UnityEngine;
using System.Collections;

public class CpuController : MonoBehaviour {

	/// <summary>
	/// Main CPU controller. 
	/// This class ia a compact AI manager which moves the cpu opponent and manage its decisions.
	/// </summary>

	//A dropdown option to select the AI level
	//the different AI levels uses different parameters for the speed and accuracy of the cpu.
	public enum cpuLevels {easy, normal, hard}
	public cpuLevels cpuLevel = cpuLevels.easy;

	public int selectedCpuAvatar = 0;			//cpu avatar can be picked from available textures [in this demo, it can be 0 to 3]
	public StatusImage[] availableAvatars;		//different avatar images 

	//reference to child game objects
	public GameObject myHead;
	public GameObject myShoe;


	private JointSpring js;
	public bool canTurnHead = true;
	public bool canTurnShoe = true;
	private float maxShoeTurn = 20.0f; //in degrees


	//starting position of cpu
	[Range(3.0f, 7.0f)]
	public float startingX = 5.0f;
	public Vector3 startingPosition;
	private bool canMove = true;

	//private AI parameters
	private float moveSpeed;
	private float jumpSpeed;
	private float minJumpDistance;
	private bool canJump = true;
	private bool preventDoubleJump = true;
	private float jumpDelay;
	private float accuracy;
	private float adjustingPosition;

	private Vector2 cpuFieldLimits = new Vector2(0.1f, 8.5f);
	private GameObject ball;


	void Awake () {
		//set cpu at starting position
		startingPosition = new Vector3(startingX, 0.5f, 0);
		//find and cache ball object
		ball = GameObject.FindGameObjectWithTag("Ball");
		StartCoroutine(resetPosition());
		setCpuLevel();

		//set cpu avatar
		myHead.GetComponent<Renderer>().material.mainTexture = availableAvatars[selectedCpuAvatar].normal;
	}


	//change the AI configuration based on the enum selection
	void setCpuLevel() {
		switch(cpuLevel) {

		case cpuLevels.easy:
			moveSpeed = 13.0f;
			jumpSpeed = 250.0f;
			minJumpDistance = 3.0f;
			accuracy = 0.5f;
			jumpDelay = 0.5f;
			break;

		case cpuLevels.normal:
			moveSpeed = 16.0f;
			jumpSpeed = 275.0f;
			minJumpDistance = 3.4f;
			accuracy = 0.4f;
			jumpDelay = 0.2f;
			break;

		case cpuLevels.hard:
			moveSpeed = 19.0f;
			jumpSpeed = 300.0f;
			minJumpDistance = 3.75f;
			accuracy = 0.35f;
			jumpDelay = 0.1f;
			break;
		}
	}
	

	void FixedUpdate () {
		if(canMove && !GameController.isGoalTransition && !GameController.gameIsFinished)
			playSoccer();
	}


	/// <summary>
	/// Main cpu play routines
	/// </summary>
	void playSoccer() {

		if(ball.transform.position.x < cpuFieldLimits.x) {

			//freePlay();

			//swing the head to the default rotation when not playing
			if(canTurnHead) {
				js = myHead.GetComponent<HingeJoint>().spring;
				js.targetPosition = 0;
				myHead.GetComponent<HingeJoint>().spring = js;
			}

			//reset shoe rotation when not playing
			if(canTurnShoe)
				myShoe.transform.localEulerAngles = new Vector3(0, 0, 180);

		} else if(ball.transform.position.x > cpuFieldLimits.x && ball.transform.position.x < cpuFieldLimits.y) {

			//move the cpu towards the ball
			transform.position = new Vector3(Mathf.SmoothStep(transform.position.x, ball.transform.position.x + adjustingPosition, Time.deltaTime * moveSpeed),
			                                 transform.position.y,
			                                 transform.position.z);

			if(transform.position.x <= ball.transform.position.x) {

				//swing cpu's head
				rotateHead(-1);

				//rotate cpu's shoe
				if(canTurnShoe)
					myShoe.transform.localEulerAngles = new Vector3(0, maxShoeTurn, 180);

			} else {

				rotateHead(1);
				if(canTurnShoe)
					myShoe.transform.localEulerAngles = new Vector3(0, maxShoeTurn * -1, 180);
			}


			//if cpu is close enough to the ball, make it jump
			if(Vector3.Distance(transform.position, ball.transform.position) < minJumpDistance && canJump) {
				canJump = false;
				Vector3 jumpPower = new Vector3(0, jumpSpeed - Random.Range(0, 50), 0);
				GetComponent<Rigidbody>().AddForce(jumpPower, ForceMode.Impulse);
			}
		}
	}


	/// <summary>
	/// Swing the head of the cpu avatar
	/// </summary>
	void rotateHead(int _dir) {

		if(!canTurnHead)
			return;

		if(myHead) {
			js = myHead.GetComponent<HingeJoint>().spring;
			js.targetPosition = 15 * _dir;
			myHead.GetComponent<HingeJoint>().spring = js;
		}
	}


	//take the object to its initial position
	public IEnumerator resetPosition() {
		canMove = false;
		transform.position = startingPosition;
		GetComponent<Rigidbody>().sleepThreshold = 0.005f;
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().angularVelocity = Vector3.zero; 
		adjustingPosition = Random.Range(0.1f, accuracy);

		yield return new WaitForSeconds(0.75f);
		canMove = true;
	}


	//Optional silly moves by cpu when ball is not in his field
	void freePlay() {

	}


	/// <summary>
	/// Change status image for avatar face
	/// </summary>
	public IEnumerator changeFaceStatus() {
		myHead.GetComponent<Renderer>().material.mainTexture = availableAvatars[selectedCpuAvatar].hit;
		yield return new WaitForSeconds(0.3f);
		myHead.GetComponent<Renderer>().material.mainTexture = availableAvatars[selectedCpuAvatar].normal;
	}


	/// <summary>
	/// Shoot the ball upon collision
	/// </summary>
	void OnCollisionEnter (Collision other) {
		
		Vector3 bounceDir = other.gameObject.transform.position - gameObject.transform.position;
		Vector3 shootForce;
		bounceDir.Normalize();
		shootForce = bounceDir * 0.5f;
		other.gameObject.GetComponent<Rigidbody>().AddForce(shootForce, ForceMode.Impulse);
		
		if(other.gameObject.tag == "Field" && preventDoubleJump)
			StartCoroutine(jumpActivation());
	}


	/// <summary>
	/// enable jump ability again
	/// </summary>
	IEnumerator jumpActivation() {
		preventDoubleJump = false;
		yield return new WaitForSeconds(jumpDelay);
		//print ("Cpu jump activated at: " + Time.timeSinceLevelLoad);
		canJump = true;
		preventDoubleJump = true;
	}
}
