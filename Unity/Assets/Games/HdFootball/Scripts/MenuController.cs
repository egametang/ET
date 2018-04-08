using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {
		
	///*************************************************************************///
	/// Main Menu Controller.
	/// This class handles all touch/click events on buttons.
	/// This class also let us choose a character to play with.
	///*************************************************************************///

	public GameObject PlayerAvatar;				//reference to gameObject

	private float buttonAnimationSpeed = 9;		//speed on animation effect when tapped on button
	private bool canTap = true;					//flag to prevent double tap
	public AudioClip tapSfx;					//tap sound for buttons click

	public Texture2D[] availableAvatars;		//available character images
	private int selectedAvatarIndex;			//index of the selected avatar


	void Awake (){
		//reset time scale variables
		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.02f;

		selectedAvatarIndex = 0;
		PlayerPrefs.SetInt("selectedAvatar", selectedAvatarIndex);
	}

	
	void Update (){	

		if(canTap) {
			StartCoroutine(tapManager());
		}

		//set the selected avatar on the gameObject
		PlayerAvatar.GetComponent<Renderer>().material.mainTexture = availableAvatars[selectedAvatarIndex];
	}


	//*****************************************************************************
	// This function monitors player touches on menu buttons.
	// detects both touch and clicks and can be used with editor, handheld device and 
	// every other platforms at once.
	//*****************************************************************************
	private RaycastHit hitInfo;
	private Ray ray;
	IEnumerator tapManager (){

		//Mouse of touch?
		if(	Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended)  
			ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
		else if(Input.GetMouseButtonUp(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			yield break;
			
		if (Physics.Raycast(ray, out hitInfo)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			switch(objectHit.name) {
			
				case "Button-Next":
					playSfx(tapSfx);							//play touch sound
					canTap = false;								//prevent double touch
					changeAvatar(1);							//change the avatar
					StartCoroutine(animateButton(objectHit));	//touch animation effect
					yield return new WaitForSeconds(0.25f);		//Wait for the animation to end
					canTap = true;
					break;

				case "Button-Prev":
					playSfx(tapSfx);							
					canTap = false;
					changeAvatar(-1);
					StartCoroutine(animateButton(objectHit));	
					yield return new WaitForSeconds(0.25f);		
					canTap = true;
					break;
					
				case "Button-Start":
					playSfx(tapSfx);
					canTap = false;
					StartCoroutine(animateButton(objectHit));
					yield return new WaitForSeconds(1.0f);
					Application.LoadLevel("Game");
					break;

				case "Button-Options":
					playSfx(tapSfx);
					StartCoroutine(animateButton(objectHit));
					yield return new WaitForSeconds(1.0f);
					//Application.LoadLevel("Options");
					break;	
			}	
		}
	}


	/// <summary>
	/// Cycle through all available avatar images
	/// </summary>
	void changeAvatar(int _inc) {

		//get the selected avatar index
		selectedAvatarIndex += _inc;

		//limit the index to the size of array containing all available avatars
		if(selectedAvatarIndex == availableAvatars.Length)
			selectedAvatarIndex = 0;
		if(selectedAvatarIndex < 0)
			selectedAvatarIndex = availableAvatars.Length - 1;

		//save the selected index
		PlayerPrefs.SetInt("selectedAvatar", selectedAvatarIndex);
	}


	//*****************************************************************************
	// This function animates a button by modifying it's scales on x-y plane.
	// can be used on any element to simulate the tap effect.
	//*****************************************************************************
	IEnumerator animateButton ( GameObject _btn  ){
		canTap = false;
		Vector3 startingScale = _btn.transform.localScale;	//initial scale	
		Vector3 destinationScale = startingScale * 1.1f;	//target scale
		
		//Scale up
		float t = 0.0f; 
		while (t <= 1.0f) {
			t += Time.deltaTime * buttonAnimationSpeed;
			_btn.transform.localScale = new Vector3( Mathf.SmoothStep(startingScale.x, destinationScale.x, t),
			                                        Mathf.SmoothStep(startingScale.y, destinationScale.y, t),
			                                        _btn.transform.localScale.z);
			yield return 0;
		}
		
		//Scale down
		float r = 0.0f; 
		if(_btn.transform.localScale.x >= destinationScale.x) {
			while (r <= 1.0f) {
				r += Time.deltaTime * buttonAnimationSpeed;
				_btn.transform.localScale = new Vector3( Mathf.SmoothStep(destinationScale.x, startingScale.x, r),
				                                        Mathf.SmoothStep(destinationScale.y, startingScale.y, r),
				                                        _btn.transform.localScale.z);
				yield return 0;
			}
		}
		
		if(r >= 1)
			canTap = true;
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