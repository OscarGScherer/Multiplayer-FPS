using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class MatchInfo
{
	public static NetworkVariable<int> playerCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
}
