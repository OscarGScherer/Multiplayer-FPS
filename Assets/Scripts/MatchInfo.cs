using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class MatchInfo
{
	public static NetworkVariable<int> playerCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	public static NetworkVariable<bool> matchStarted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	public static List<GameObject> playersCharacters = new List<GameObject>();
	public static List<Player> playerClients = new List<Player>();
	
	public static Player yourPlayer;
	public static ulong yourClientID;
	
	[ServerRpc]
	public static void AddPlayer_ServerRPC()
	{
		playerCount.Value++;
	}
}
