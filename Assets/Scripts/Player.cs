using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
	public GameObject playerCharacterPrefab;
	public int teamLayer;
	
	public void SpawnPlayer(Scene scene, LoadSceneMode mode)
	{
		SpawnPlayer_ServerRPC(OwnerClientId);
	}
	
	[ServerRpc]
	private void SpawnPlayer_ServerRPC(ulong ownerClientId)
	{
		Transform spawn = GameObject.FindGameObjectWithTag("Team " + teamLayer % 5 + " Spawn").transform;
		GameObject go = NetworkManager.Instantiate(playerCharacterPrefab, spawn.position, spawn.rotation);
		SetLayerAllChildren(go.transform, teamLayer);
		go.GetComponent<NetworkObject>().SpawnAsPlayerObject(ownerClientId);
		go.GetComponent<PlayerController>().spawnPoint = spawn;
		go.GetComponent<PlayerController>().team.Value = teamLayer - 5;
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
		if(SceneManager.GetActiveScene().name == "Map" && IsOwner) SpawnPlayer_ServerRPC(OwnerClientId);
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