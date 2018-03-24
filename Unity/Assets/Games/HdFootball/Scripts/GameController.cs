using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	/// <summary>
	/// Main game controller class.
	/// This class controls the game time (passed and remaining) and also manages game status
	/// like win, lose or draw.
	/// This class resets the game elements after each goal events.
	/// </summary>

	public static int gameTime = 90;				//gameplay time for each round
	private int remainingTime;						//time left to finish the game
	public static int playerGoals;					//total goals by player
	public static int cpuGoals;						//total goals by cpu
	public static float startDelay = 3.0f;			//cooldown timer before starting the game

	public static bool gameIsFinished = false;		//global flag to finish the game
	private bool gameFinishFlag = false;			//private flag

	public AudioClip startWistle;					//Audioclip
	public AudioClip endWistle;						//Audioclip
	public AudioClip[] goalReceived;				//Audioclip
	public AudioClip[] goalLanded;					//Audioclip

	//Reference to important game objects used inside the game
	public GameObject counter;						
	public GameObject hudPlayerGoals;
	public GameObject hudCpuGoals;
	public GameObject hudGameTime;
	public GameObject ball;
	public GameObject goalPlane;
	public GameObject GameFinishPlane;
	public GameObject gameFinishStatusImage;

	public Texture2D[] statusImages;				//available images for end game status

	private bool isFirstRun = true;					//flag to initialize the game
	public static bool isGoalTransition = false;	//flag to manage after goal events


	void Awake () {
		gameIsFinished = false;
		gameFinishFlag = false;
		playerGoals = 0;
		cpuGoals = 0;
		StartCoroutine(init());

		if(GameFinishPlane)
			GameFinishPlane.SetActive(false); 
	}


	//setup the game for the first run
	IEnumerator init() {
		//if this is the first run, init and stop the physics for a bit
		if(isFirstRun) {

			//freeze the ball
			ball.GetComponent<Rigidbody>().useGravity = false;
			ball.GetComponent<Rigidbody>().isKinematic = true;

			//show the countdown timer
			GameObject Counter = Instantiate(counter, new Vector3(0, 1.8f, -1), Quaternion.Euler(0, 180, 0)) as GameObject;
			Counter.name = "Starting-Time-Counter";
			yield return new WaitForSeconds(startDelay);

			//start the game
			isFirstRun = false;
			playSfx(startWistle);
			yield return new WaitForSeconds(startWistle.length);

			//unfreeze the ball
			ball.GetComponent<Rigidbody>().useGravity = true;
			ball.GetComponent<Rigidbody>().isKinematic = false;
		}
	}


	void Start () {
		remainingTime = gameTime;
	}

	
	void Update () {

		//show game timer
		manageGameTime();

		if(!gameIsFinished)
			manageGoals();

		//if time is up, setup game finish events
		if(gameIsFinished && !gameFinishFlag) {
			gameFinishFlag = true;

			//play end wistle
			playSfx(endWistle);

			//declare the winner
			if(playerGoals > cpuGoals) {
				print ("Player Wins");
				gameFinishStatusImage.GetComponent<Renderer>().material.mainTexture = statusImages[0];
			} else if(playerGoals < cpuGoals) {
				print ("CPU Wins");
				gameFinishStatusImage.GetComponent<Renderer>().material.mainTexture = statusImages[1];
			} else if(playerGoals == cpuGoals) {
				print ("Draw!");
				gameFinishStatusImage.GetComponent<Renderer>().material.mainTexture = statusImages[2];
			}

			//show gamefinish plane
			GameFinishPlane.SetActive(true); 
		}

	}


	/// <summary>
	/// Update game data after a goal happens.
	/// Arg Parameters:
	/// 0 = received a goal
	/// 1 = scored a goal
	/// </summary>
	public IEnumerator manageGoalEvent(int _side) {

		//fake pause the game for a while
		isGoalTransition = true;

		//blow the wistle && add the score
		if(_side == 0) {
			//player received a goal
			playSfx(goalReceived[Random.Range(0, goalReceived.Length)]);
			cpuGoals++;
		} else {
			//cpu received a goal
			playSfx(goalLanded[Random.Range(0, goalLanded.Length)]);
			playerGoals++;
		}

		//activate the goal event plane
		GameObject gp = null;
		if(_side == 1) {
			gp = Instantiate (goalPlane, new Vector3(15, 2, -1), Quaternion.Euler(0, 180, 0)) as GameObject;
			float t = 0;
			float speed = 2.0f;
			while(t < 1) {
				t += Time.deltaTime * speed;
				gp.transform.position = new Vector3(Mathf.SmoothStep(15, 0, t), 2, -1);
				yield return 0;
			}

			yield return new WaitForSeconds(0.75f);

			float t2 = 0;
			while(t2 < 1) {
				t2 += Time.deltaTime * speed;
				gp.transform.position = new Vector3(Mathf.SmoothStep(0, -15, t2), 2, -1);
				yield return 0;
			}
		}

		yield return new WaitForSeconds(1.5f);
		Destroy(gp);
		isGoalTransition = false;

		//do not continue the game if the time is up
		if(gameIsFinished)
			yield break;

		//continue the game
		playSfx(startWistle);
		yield return new WaitForSeconds(startWistle.length);
		ball.GetComponent<Rigidbody>().isKinematic = false;
		ball.GetComponent<Rigidbody>().AddForce(new Vector3(0, 1, 0), ForceMode.Impulse);

	}


	//show goals on the screen
	void manageGoals() {
		hudCpuGoals.GetComponent<TextMesh>().text = cpuGoals.ToString();
		hudPlayerGoals.GetComponent<TextMesh>().text = playerGoals.ToString();
	}


	//show game timer on the screen
	void manageGameTime() {
		remainingTime = (int)(gameTime - Time.timeSinceLevelLoad);

		//finish the game if time is up
		if(remainingTime <= 0) {
			gameIsFinished = true;
			remainingTime = 0;
		}

		hudGameTime.GetComponent<TextMesh>().text = remainingTime.ToString();
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