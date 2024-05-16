using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CapturePoint : NetworkBehaviour
{
	public static NetworkVariable<float> scoreWhite = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	public static NetworkVariable<float> scoreBlack = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	
	private static List<PlayerController> whitePlayersOnTop = new List<PlayerController>();
	private static List<PlayerController> blackPlayersOnTop = new List<PlayerController>();
	
	public Lobby lobby; 
	
	public static void Reset()
	{
		whitePlayersOnTop.Clear();
		blackPlayersOnTop.Clear();
		scoreWhite.Value = 0;
		scoreBlack.Value = 0;
	}
	
	void Start()
	{
		if(!IsServer) this.enabled = false;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(!IsServer) return;
		
		PlayerBody playerBody = other.GetComponent<PlayerBody>();
		if (playerBody == null || other.tag != "Player") return;
		
		if(playerBody.player.team == Player.Team.White && !whitePlayersOnTop.Contains(playerBody.player))
			whitePlayersOnTop.Add(playerBody.player);
			
		if(playerBody.player.team == Player.Team.Black && !blackPlayersOnTop.Contains(playerBody.player))
			blackPlayersOnTop.Add(playerBody.player);
	}
	
	void OnTriggerExit(Collider other)
	{
		if(!IsServer) return;
		
		PlayerBody playerBody = other.GetComponent<PlayerBody>();
		if (playerBody == null) return;
		
		if(playerBody.player.team == Player.Team.White && whitePlayersOnTop.Contains(playerBody.player))
			whitePlayersOnTop.Remove(playerBody.player);
			
		if(playerBody.player.team == Player.Team.Black && blackPlayersOnTop.Contains(playerBody.player))
			blackPlayersOnTop.Remove(playerBody.player);
	}
	
	void Update()
	{
		if(whitePlayersOnTop.Count == 0 && blackPlayersOnTop.Count > 0) 
			scoreBlack.Value += Time.deltaTime;
		else if(blackPlayersOnTop.Count == 0 && whitePlayersOnTop.Count > 0) 
			scoreWhite.Value += Time.deltaTime;
		
		if(scoreWhite.Value >= 100) 
		{
			lobby.EndMatch(Player.Team.White);
		}
		else if(scoreBlack.Value >= 100) 
		{
			lobby.EndMatch(Player.Team.Black);
		}
	}
}
