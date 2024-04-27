using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
	public GameObject[] characterPrefabs;
	public int teamLayer;
	private GameObject currentCharacter;
	
	public void SpawnPlayer(Scene scene, LoadSceneMode mode)
	{
		SpawnPlayer_ServerRPC(0);
	}
	
	[ServerRpc]
	private void SpawnPlayer_ServerRPC(int characterIndex)
	{
		Transform spawn = GameObject.FindGameObjectWithTag("Team " + teamLayer % 5 + " Spawn").transform;
		currentCharacter = NetworkManager.Instantiate(characterPrefabs[characterIndex], spawn.position, spawn.rotation);
		SetLayerAllChildren(currentCharacter.transform, teamLayer);
		currentCharacter.GetComponent<NetworkObject>().SpawnAsPlayerObject(OwnerClientId);
		currentCharacter.GetComponent<PlayerInput>().player = this;
		currentCharacter.GetComponent<PlayerController>().spawnPoint = spawn;
		currentCharacter.GetComponent<PlayerController>().team.Value = teamLayer - 5;
	}
	
	[ServerRpc]
	public void SwitchCharacter_ServerRPC(int characterIndex)
	{
		Destroy(currentCharacter);
		SpawnPlayer_ServerRPC(characterIndex);
	}
	
	void SetLayerAllChildren(Transform root, int layer)
	{
		var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (var child in children)
		{
			child.gameObject.layer = layer;
		}
	}
	
	void Start()
	{
		if(SceneManager.GetActiveScene().name == "Map" && IsOwner) SpawnPlayer_ServerRPC(0);
	}
	
	public override void OnNetworkSpawn()
	{
		teamLayer = 6 + MatchInfo.playerCount.Value % 2;
		if(IsOwner) 
		{
			MatchInfo.AddPlayer_ServerRPC();
			SceneManager.sceneLoaded -= SpawnPlayer;
			SceneManager.sceneLoaded += SpawnPlayer;
		}
	}
}