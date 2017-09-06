using UnityEngine;
using System.Collections;

public class MotionControllerResetFadeNet : MonoBehaviour {

	public constantProfSetting ConstantProfileSetting = constantProfSetting.SetBySpeedAndPeriod;
	public float constProfSpeed = 1.4f; //meters per second
	public float period = 7.0f; //secs
	public float pathLength = 9.8f; //meters
	public float N = 4.0f; //polynomial degree
	public int numberOfTrials = 12; 
	public float pause = 1.5f; //secs


	bool inMotion = false;
	float displacement = 0.0f;
	float elapsedTime = 0.0f;
	float m = 1.0f;
	int curTrial = 1;
	Vector3 initPos;
	bool enterOnce = true;


	public enum constantProfSetting { SetByPeriodAndPathLength, SetBySpeedAndPeriod};
	enum typesOfProfile { constant, ramp, NdegreePoly, none };
	typesOfProfile selectedProfile;


	private positionFade posFad;	//fade in/out script
	private pythonTalker pyTalker;  //python talker script - sends start recording signal

	//start
	void Start (){
		posFad = GetComponent<positionFade> ();
		pyTalker = GetComponent<pythonTalker> ();		
	}


	// Update is called once per frame
	void Update () {

		if (inMotion == false) { //There is not camera motion

			if (Input.GetKeyDown (KeyCode.C)) {	//Start constant speed profile motion

				inMotion = true; //We activate motion
				computeConstProfParameters (out m, period, pathLength); //We compute the constant profile parameters - 'm' in v = m;
				selectedProfile = typesOfProfile.constant;					

			} else if (Input.GetKeyDown (KeyCode.R)) {

				inMotion = true; //We activate motion
				computeRampParameters (out m, period, pathLength); //We compute the ramp parameters - 'm' in v = m*T;
				selectedProfile = typesOfProfile.ramp;

			} else if (Input.GetKeyDown (KeyCode.P)) {

				inMotion = true; //We activate motion
				computeNpolyParameters (out m, period, pathLength); //We compute the N degree polynomial parameters - 'm' in v = m*(T^N);
				selectedProfile = typesOfProfile.NdegreePoly;

			}//else if

			if (inMotion == true) //motion has just been activated
				pyTalker.startWiiBoardRecording (); //send start recording signal

			//Initialization
			elapsedTime = 0.0f;
			initPos = transform.position; //stores the initial position

		} else { //The camera is in moving stage

			//We update the elapsed time since the motion started
			elapsedTime += Time.deltaTime;			

			//The elapsed time is less than the initial pause plus period
			if (elapsedTime <= (pause + period)) {

				//The initial pause has passed so we apply displacement
				if (elapsedTime > pause) {
				
					//We compute the displacement to be applied
					switch (selectedProfile) {

					case typesOfProfile.constant:	//constant speed profile	
						displacement = computeDisplacementFromConstantSpeed (elapsedTime, m);
						break;

					case typesOfProfile.ramp:		//ramp speed profile	
						displacement = computeDisplacementFromRamp (elapsedTime - pause, m, period);
						break;

					case typesOfProfile.NdegreePoly:		//ramp speed profile	
						displacement = computeDisplacementFromNdegreePol (elapsedTime - pause, m, period);
						break;

					default:
						break;

					}//switch
		
					//We apply the computed displacement
					transform.Translate (0.0f, 0.0f, displacement);

				}//if

			//We start fade at the end of the trial
			} else if ((pause + period) < elapsedTime  && elapsedTime < (pause + period + pause) ) {

				//we apply the fade
				if (enterOnce == true) {					
//					positionFade posFad = GetComponent<positionFade> ();
					posFad.beginFade (2.0f*pause);
					enterOnce = false;
				}//if

			//We check the stop condition after the pause at the end of the trial
			} else if (elapsedTime >= (pause + period + pause) ) { 


				//We reached the limit number of trials
				if (curTrial == numberOfTrials) {
						inMotion = false; //We stop motion	
					curTrial = 0;
				}//if

				//We update the trial number and reset elapsed time and enterOnce
				curTrial++;
				elapsedTime = 0.0f;
				enterOnce = true;

				//We reset the position
				transform.position = initPos;
					
			}//else if
					
		}//else

	}

////////////////////////////////////////////////////////////////////////////////////
///////////////////Functions for computing speed profiles///////////////////
////////////////////////////////////////////////////////////////////////////////////

	//This function computes the constant profile parameters according to a period 
	//	in seconds and desired distance to travel s
	void computeConstProfParameters(out float M, float period, float s){
		M = 0;
		if (ConstantProfileSetting == constantProfSetting.SetByPeriodAndPathLength) {
			M = s / period;
		} else if (ConstantProfileSetting == constantProfSetting.SetBySpeedAndPeriod) {
			M = constProfSpeed;
		}//else if	
	}
		
	//This function computes the displacement according to a given constant speed profile
	float computeDisplacementFromConstantSpeed(float t, float speed){
		//Let P be the period of the signal in seconds
		//Assumed speed profile
		//
		// v(t) = m

		Debug.Log("Translational Speed: " + speed.ToString() + " At t: " + t);

		//We compute the displacement and return it
		float dis = speed * Time.deltaTime;
		return dis; 	
	}


	//This function computes the ramp parameters according to a period in seconds and 
	//	desired distance to travel s
	void computeRampParameters(out float M, float period, float s){
		float half_period = period / 2.0f;
		float half_s = s / 2.0f;
		M = (2.0f * half_s) / ( half_period * half_period);		
	}
		
	//This function computes the displacement according to a given ramp speed profile
	float computeDisplacementFromRamp(float t, float M, float period){
		//Let P be the period of the signal in seconds
		//Assumed speed profile
		//
		// v(t) = m*t 				, 	t <= (period/2)  
		// v(t) = m*[period-t] 		, 	t > (period/2)  

		float speed = 0.0f;
		float dis = 0.0f; 

		if (t <= (period / 2.0f)) {	//first half of the profile
			speed = M * t;
		} else {					//second half of the profile
			speed = M * (period - t);					
		}

		Debug.Log("Translational Speed: " + speed.ToString() + " At t: " + t );

		//We compute the displacement and return it
		dis = speed * Time.deltaTime;
		return dis; 	
	}
		

	//This function computes the N degree polynomial parameters according to a period in seconds and 
	//	desired distance to travel s
	void computeNpolyParameters(out float M, float period, float s){
		float half_period = period / 2.0f;
		float half_s = s / 2.0f;

		// M  = (N+1)*S/(T^(N+1))
		M = (N+1.0f) * half_s / Mathf.Pow (half_period, N+1.0f);	
	}

	//This function computes the displacement according to a given N degree polynomial speed profile
	float computeDisplacementFromNdegreePol(float t, float M, float period){
		//Let P be the period of the signal in seconds
		//Assumed speed profile
		//
		// v(t) = m*t^N 				, 	t <= (period/2)  
		// v(t) = m*[period-t]^N 		, 	t > (period/2)  

		float speed = 0.0f;
		float dis = 0.0f; 

		if (t <= (period / 2.0f)) {	//first half of the profile
			speed = M * Mathf.Pow (t, N);
		} else {					//second half of the profile
			speed = M * Mathf.Pow (period - t, N);					
		}

		Debug.Log("Translational Speed: " + speed.ToString() + " At t: " + t );

		//We compute the displacement and return it
		dis = speed * Time.deltaTime;
		return dis; 	
	}
}
