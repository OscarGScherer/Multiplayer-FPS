using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Dummy : MonoBehaviour
{
	public void Start()
	{
		MatchInfo.players.Add(gameObject);
		GetComponent<PlayerController>().team.Value = gameObject.layer - 5;
	}
}
