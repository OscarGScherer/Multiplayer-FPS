using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Lobby : NetworkBehaviour
{
	public GameObject playerNamePrefab;
	public RectTransform[] playerLists = new RectTransform[3];
	public GameObject startMatchButton;
	public Camera lobbyCamera;
	public TextMeshProUGUI winningTeamText;
	
	[ClientRpc]
	public void ClearPlayerList_ClientRPC(int list)
	{
		foreach(Transform transform in playerLists[list])
			Destroy(transform.gameObject);
	}
	
	public void Quit()
	{
		NetworkManager.Shutdown();
	}
	
	public void StartMatch()
	{
		if(!IsServer) return;
		
		//if(playerLists[1].transform.childCount == 0 && playerLists[2].transform.childCount == 0) return;
		
		foreach(NetworkClient client in NetworkManager.ConnectedClientsList)
		{
			Player player = client.PlayerObject.GetComponent<Player>();
			Debug.Log(player.team.Value);
			if(player.team.Value != Player.Team.Spectator) player.SpawnPlayer_ServerRPC(0);
		}
		CloseLobby_ClientRPC();
	}
	
	public void EndMatch(Player.Team winningTeam)
	{
		foreach(NetworkClient client in NetworkManager.ConnectedClientsList)
		{
			Player player = client.PlayerObject.GetComponent<Player>();
			player.DestroyCharacter_ClientRPC();
		}
		OpenLobby_ClientRPC(winningTeam);
	}
	
	[ClientRpc]
	public void CloseLobby_ClientRPC()
	{
		winningTeamText.text = "Match has already started";
		
		if(MatchInfo.yourPlayer.team.Value != Player.Team.Spectator) lobbyCamera.enabled = false;
		transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
	}
	
	[ClientRpc] void OpenLobby_ClientRPC(Player.Team winningTeam)
	{
		winningTeamText.text = (winningTeam == Player.Team.White) ? "TEAM WHITE WON" : "TEAM BLACK WON";
		winningTeamText.color = (winningTeam == Player.Team.White) ? Color.white : Color.black;
		
		lobbyCamera.enabled = true;
		transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
	}
	
	[ClientRpc]
	public void AddPlayerToList_ClientRPC(int list, string name)
	{
		TextMeshProUGUI tmp = Instantiate(playerNamePrefab, playerLists[list].transform).GetComponent<TextMeshProUGUI>();
		tmp.gameObject.SetActive(true);
		tmp.text = name;
	}
	
	public override void OnNetworkSpawn()
	{
		if(IsServer)
		{
			NetworkManager.OnClientConnectedCallback += UpdateLobbyUI;
			NetworkManager.OnClientDisconnectCallback += UpdateLobbyUI;
			UpdateLobbyUI(0);
			startMatchButton.SetActive(true);
		}
	}
	
	public void ChangePlayerTeam(int newTeam)
	{
		ChangePlayerTeam_ServerRPC(newTeam, MatchInfo.yourClientID);
	}
	
	[ServerRpc(RequireOwnership = false)]
	public void ChangePlayerTeam_ServerRPC(int newTeam, ulong clientId)
	{
		foreach(NetworkClient client in NetworkManager.ConnectedClientsList)
		{
			Player player = client.PlayerObject.GetComponent<Player>();
			if(player.OwnerClientId == clientId)
			{
				player.team.Value = (Player.Team) newTeam;
				break;
			}
		}
	}
	
	public void UpdateLobbyUI(Player.Team prev, Player.Team curr) => UpdateLobbyUI(0);
	public void UpdateLobbyUI(ulong OwnerClientId)
	{
		ClearPlayerList_ClientRPC(0);
		ClearPlayerList_ClientRPC(1);
		ClearPlayerList_ClientRPC(2);
		
		foreach(NetworkClient client in NetworkManager.ConnectedClientsList)
		{
			Player player = client.PlayerObject.GetComponent<Player>();
			
			player.team.OnValueChanged -= UpdateLobbyUI;
			player.team.OnValueChanged += UpdateLobbyUI;
			
			AddPlayerToList_ClientRPC((int)player.team.Value, player.name);	
		}
				
	}
	
}
