using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CapturePoint : NetworkBehaviour
{
	public static NetworkVariable<float> scoreTeam1 = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	public static NetworkVariable<float> scoreTeam2 = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	
	private List<PlayerController> team1MembersOnTop = new List<PlayerController>();
	private List<PlayerController> team2MembersOnTop = new List<PlayerController>();
	
	void Start()
	{
		if(!IsServer) this.enabled = false;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(!IsServer) return;
		
		PlayerBody playerBody = other.GetComponent<PlayerBody>();
		if (playerBody == null || other.tag != "Player") return;
		
		if(playerBody.player.team.Value == 1 && !team1MembersOnTop.Contains(playerBody.player))
			team1MembersOnTop.Add(playerBody.player);
			
		if(playerBody.player.team.Value == 2 && !team2MembersOnTop.Contains(playerBody.player))
			team2MembersOnTop.Add(playerBody.player);
	}
	
	void OnTriggerExit(Collider other)
	{
		if(!IsServer) return;
		
		PlayerBody playerBody = other.GetComponent<PlayerBody>();
		if (playerBody == null) return;
		
		if(playerBody.player.team.Value == 1 && team1MembersOnTop.Contains(playerBody.player))
			team1MembersOnTop.Remove(playerBody.player);
			
		if(playerBody.player.team.Value == 2 && team2MembersOnTop.Contains(playerBody.player))
			team2MembersOnTop.Remove(playerBody.player);
	}
	
	void Update()
	{
		if(team1MembersOnTop.Count == 0 && team2MembersOnTop.Count > 0) 
			scoreTeam2.Value += Time.deltaTime * team2MembersOnTop.Count;
		else if(team2MembersOnTop.Count == 0 && team1MembersOnTop.Count > 0) 
			scoreTeam1.Value += Time.deltaTime * team1MembersOnTop.Count;
	}
}
