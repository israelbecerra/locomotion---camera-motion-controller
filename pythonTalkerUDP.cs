using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;



public class pythonTalkerUDP : MonoBehaviour {
    
	private bool alreadyEntered = false;

	//server ip address and port
	public string hostName = "localhost";
	public int hostPort = 25005;

	//udp client
	UdpClient udpClient = new UdpClient ();


    // Update is called once per frame
    void Update () {

		//Press 's' key and it is allowed to only enter once
		if (Input.GetKey (KeyCode.S) && alreadyEntered==false) {

			startWiiBoardRecording ();
			alreadyEntered = true;

		}//if

    }


	// Sends the start recording flag to the wii board script
	public void startWiiBoardRecording (){

		//Sends the message "start"
		byte[] sendBytes = Encoding.ASCII.GetBytes("Start");
		udpClient.Send(sendBytes, sendBytes.Length, hostName, hostPort);
		Debug.Log("Sent 'start recording' command");

		//Closes connection
		udpClient.Close();
	}

}
