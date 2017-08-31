using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class positionFade : MonoBehaviour {

	public Texture texture;

	private enum fadeMode {fadeNot, fadeOut, fadePause, fadeIn}; //fade Mode
	private fadeMode fadeMod = fadeMode.fadeOut;				//default fade Mode, it starts with fadeout
	private float alpha = 0.0f;									//alpha value of fade texture
	private float fadeSpeed = 1.0f;								//fade speed
	private float timer = 0.0f;									//auxiliary timer
	private bool fadeActive = false;							//active when fade is taking place
	private float totFadDur = 0.0f;								//total fade duration


	public void beginFade(float totalFadeDuration) {
		fadeActive = true;					//activates fade
		totFadDur = totalFadeDuration;		//total fade duration
		fadeSpeed = 1.0f/(totFadDur/3.0f); //We compute speed = max_alpha/time
		timer = 0.0f;				       //resets timer
	}

	void Update () {
		timer += Time.deltaTime; //we update the timer just once per frame (initialized in beginFade(...))
	}
		
	void OnGUI () {	

		if (fadeActive == true) //calls fade() only if fadeActive was turned on in beginFade(...)
			fade ();
	}

	//Fade function
	void fade() {

		//Performs acording to the current fade mode
		switch (fadeMod)	{

		case fadeMode.fadeOut: //fade out

			alpha = fadeSpeed * timer; //computes alpha = speed * elapsed_time
			alpha = Mathf.Clamp01 (alpha);

			if (timer >= totFadDur/3.0f)	//checks if first third of total time has passed
				fadeMod = fadeMode.fadePause;
			break;

		case fadeMode.fadePause:
			
			if (timer >= totFadDur*2.0f/3.0f)//checks if two thirds of total time have passed
				fadeMod = fadeMode.fadeIn;
			break;	

		case fadeMode.fadeIn: //fade in

			alpha = 1.0f - fadeSpeed * (timer - totFadDur*2.0f/3.0f); ////computes alpha = speed * elapsed_time
			alpha = Mathf.Clamp01 (alpha);

			if (timer >= totFadDur) //checks if total time has passed
				fadeMod = fadeMode.fadeNot;
			break;

		case fadeMode.fadeNot: //deactivates fading and resets settings
			fadeActive = false;
			fadeMod = fadeMode.fadeOut;
			break;

		}//switch
					
		GUI.depth = -1000; //huge negative value to paint on top of everything
		GUI.color = new Color (GUI.color.r,GUI.color.g,GUI.color.b,alpha); //updates alpha value
		GUI.DrawTexture (new Rect (0,0,Screen.width,Screen.height), texture);  //prints texture

	}//fade ()

}

