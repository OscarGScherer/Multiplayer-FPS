using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Dummy : MonoBehaviour
{
	public int team = 1;
	private PlayerController playerController;
	
	public void Start()
	{
		MatchInfo.players.Add(gameObject);
		playerController = GetComponent<PlayerController>();
		playerController.team.Value = team;
	}
	
	public void Update()
	{
		playerController.FireGun_ServerRPC();
	}
}
