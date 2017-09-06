using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;



public class pythonTalker : MonoBehaviour {
    
	private bool alreadyEntered = false;

	//client socket
	System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();

	//server ip address and port
	public string hostName = "localhost";
	public int hostPort = 8088;


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

		//Connects to host
		clientSocket.Connect(hostName, hostPort);

		//Sends the message "start"
		NetworkStream serverStream = clientSocket.GetStream();
		byte[] outStream = System.Text.Encoding.ASCII.GetBytes("start");
		serverStream.Write(outStream, 0, outStream.Length);
		serverStream.Flush();
		Debug.Log("Sent 'start recording' command");

		//Closes connection
		serverStream.Close ();
		clientSocket.Close ();

	}

}
