using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkConnection : MonoBehaviour
{
	public void StartServer() { GetComponent<NetworkManager>().StartServer(); }
	public void StartClient() { GetComponent<NetworkManager>().StartClient(); }
	public void StartHost() 
	{ 
		GetComponent<NetworkManager>().SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
		GetComponent<NetworkManager>().StartHost();
	}
}
