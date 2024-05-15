using System;
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
	public GameObject currentCharacter;
	
	public enum Team
	{
		Spectator = 0,
		White = 1,
		Black = 2
	}
	public NetworkVariable<Team> team = new NetworkVariable<Team>(Team.Spectator, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	
	[ServerRpc(RequireOwnership = false)]
	public void SpawnPlayer_ServerRPC(int characterIndex)
	{
		Transform spawn = GameObject.FindGameObjectWithTag("Team " + team.Value + " Spawn").transform;
		currentCharacter = NetworkManager.Instantiate(characterPrefabs[characterIndex], spawn.position, spawn.rotation);
		currentCharacter.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
		currentCharacter.GetComponent<PlayerController>().Respawn_ClientRPC(spawn.position, spawn.rotation);
	}
	
	[ClientRpc]
	public void DestroyCharacter_ClientRPC()
	{
		Destroy(currentCharacter);
	}
	
	[ServerRpc]
	public void SwitchCharacter_ServerRPC(int characterIndex)
	{
		Destroy(currentCharacter);
		SpawnPlayer_ServerRPC(characterIndex);
	}
	
	[ServerRpc]
	private void SetClientName_ServerRPC(string name)
	{
		this.name = name;
		Rename_ClientRPC(name);
	}
	[ClientRpc]
	public void Rename_ClientRPC(string name) => this.name = name;
	
	public override void OnNetworkSpawn()
	{
		if(IsOwner)
		{
			MatchInfo.yourClientID = OwnerClientId;
			MatchInfo.yourPlayer = this;
			SetClientName_ServerRPC(NetworkManagerHud.username);
		}
		
		MatchInfo.playerClients.Add(this);
		
		// teamLayer = 6 + MatchInfo.playerCount.Value % 2;
		// if(IsOwner) 
		// {
		// 	MatchInfo.AddPlayer_ServerRPC();
		// 	SceneManager.sceneLoaded -= SpawnPlayer;
		// 	SceneManager.sceneLoaded += SpawnPlayer;
		// }
	}
}