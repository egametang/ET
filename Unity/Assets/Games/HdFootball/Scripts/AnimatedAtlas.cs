using UnityEngine;
using System.Collections;

public class AnimatedAtlas : MonoBehaviour {
		
	///*************************************************************************///
	/// This class animates an atlas texture.
	///*************************************************************************///

	public int tileX; 						//tiles of animation in X direction
	public int tileY; 						//tiles of animation in Y direction
	public float framesPerSecond = 16.0f;	//animation speed

	private int index;
	private Vector2 size;
	private int uIndex;
	private int vIndex;
	private Vector2 offset;

	private float startTime;

	void Awake() {
		index = 0;
		startTime = Time.time;
	}

	void Update (){
		index = (int)(( (Time.time - startTime) * framesPerSecond) % (tileX * tileY));
	    size = new Vector2(1.0f / tileX, 1.0f / tileY);
	    uIndex = index % tileX;
	    vIndex = index / tileX;
	    offset = new Vector2(uIndex * size.x, 1.0f - size.y - vIndex * size.y);
	   	GetComponent<Renderer>().material.SetTextureOffset ("_MainTex", offset);
	    GetComponent<Renderer>().material.SetTextureScale ("_MainTex", size);

		if(index == (tileX * tileY) - 1 )
			Destroy (gameObject);
	}

}