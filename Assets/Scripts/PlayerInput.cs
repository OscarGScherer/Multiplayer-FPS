using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
	
	public struct InputInfo
	{
		public Vector2 wasdVector, mouseVector;
		public bool leftClick, spacebar;
		
		public InputInfo(Vector2 wasdVector, Vector2 mouseVector, bool leftClick, bool spacebar)
		{
			this.wasdVector = wasdVector;
			this.mouseVector = mouseVector;
			this.leftClick = leftClick;
			this.spacebar = spacebar;
		}
	}
	private bool canSwapCharacters = false;
	public Player player;
	private InputInfo clientInput = new InputInfo(Vector2.zero, Vector2.zero, false, false);
	private PlayerController playerController;
	private Canvas ui;
	private RectTransform target;
	private Camera camera;
	private Transform projectileOrigin;
	
	private Ability shift, q, e;
	
	void OnTriggerEnter(Collider other)
	{
		if(other.name == "Team " + playerController.team.Value + " Spawn Room") 
		{
			canSwapCharacters = true;
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if(other.name == "Team " + playerController.team.Value + " Spawn Room") 
			canSwapCharacters = false;
	}
	
	Transform FindWithTag(Transform root, string tag)
	{
		foreach (Transform t in root.GetComponentsInChildren<Transform>())
		{
			if (t.CompareTag(tag)) return t;
		}
		return null;
	}
	
	void Awake()
	{
		playerController = GetComponent<PlayerController>();
		ui = transform.GetComponentInChildren<Canvas>();
		camera = transform.GetComponentInChildren<Camera>();
		target = ui.transform.GetChild(0).GetComponent<RectTransform>();
		projectileOrigin = camera.transform.GetChild(1);
		
		Transform abilityParent = FindWithTag(transform, "Abilities");
		shift = abilityParent.GetChild(0).GetComponent<Ability>();
		q = abilityParent.GetChild(1).GetComponent<Ability>();
		e = abilityParent.GetChild(2).GetComponent<Ability>();
		//Cursor.visible = false;
	}
	
	public override void OnNetworkSpawn()
	{	
		if(IsOwner) playerController.EnableCamera();
		MatchInfo.players.Add(gameObject);
		for(int i = 0; i < MatchInfo.players.Count; i++)
			if(MatchInfo.players[i] == null)
			{
				MatchInfo.players.RemoveAt(i);
				i--;
			} 
	}

	public override void OnNetworkDespawn()
	{
		MatchInfo.players.Remove(gameObject);
	}

	public PlayerController GetTargetedTeammate()
	{
		PlayerController targetedTeammate = null;
		float closestAngle = 20f;
		foreach(GameObject p in MatchInfo.players)
		{
			if(p == null) continue;
			PlayerController otherPlayer = p.GetComponent<PlayerController>();
			if(otherPlayer == null || otherPlayer == playerController) continue;
			if(otherPlayer.team.Value != playerController.team.Value) continue;
			
			Vector3 dir = otherPlayer.transform.GetChild(0).position - transform.GetChild(0).position;
			float angle = Vector3.Angle(playerController.facingDirection.Value, dir);
			
			if(angle < closestAngle) targetedTeammate = otherPlayer;
		}
		return targetedTeammate;
	}
	
	public void SetTargetToWorldObject(Transform worldObject)
	{
		var screen = camera.WorldToScreenPoint(worldObject.transform.position);
		//screen.z = (canvas.transform.position - ui.transform.position).magnitude;
		//var position = ui.ScreenToWorldPoint(screen);
		//element.position = position; // element is the Text show in the UI.
		target.anchoredPosition = screen;
	}

	void Update()
	{
		if(!IsOwner) return;
		//Debug.Log(OwnerClientId + ": " + MatchInfo.playerCount.Value);
		clientInput.wasdVector.x = Input.GetAxis("Horizontal");
		clientInput.wasdVector.y = Input.GetAxis("Vertical");
		
		clientInput.mouseVector.x = Input.GetAxis("Mouse X");
		clientInput.mouseVector.y = Input.GetAxis("Mouse Y");
		
		clientInput.leftClick = Input.GetKey(KeyCode.Mouse0);
		clientInput.spacebar = Input.GetKey(KeyCode.Space);
		
		if(playerController.canMove)
		{
			PlayerController targettedTeammate = GetTargetedTeammate();
			if(targettedTeammate != null)
			{
				target.gameObject.SetActive(true);
				SetTargetToWorldObject(targettedTeammate.transform);
			}
			else target.gameObject.SetActive(false);
			
			if(Input.GetKey(KeyCode.Q)) q.TryCast(playerController, targettedTeammate, projectileOrigin, playerController.facingDirection.Value);
			if(Input.GetKey(KeyCode.LeftShift)) shift.TryCast(playerController, targettedTeammate, projectileOrigin, playerController.facingDirection.Value);
			if(Input.GetKey(KeyCode.E)) e.TryCast(playerController, targettedTeammate, projectileOrigin, playerController.facingDirection.Value);
			
			if(canSwapCharacters && Input.GetKeyDown(KeyCode.Alpha1)) player.SwitchCharacter_ServerRPC(0);
			else if(canSwapCharacters && Input.GetKeyDown(KeyCode.Alpha2)) player.SwitchCharacter_ServerRPC(1);
			
			//Movement stuff, client side
			//if(Input.GetKeyDown(KeyCode.Escape)) playerController.Teleport(Vector3.zero);
			playerController.Move(clientInput.wasdVector);
			playerController.LookAt(clientInput.mouseVector);
			if(clientInput.spacebar) playerController.Jump(7f);
			
			//Shooting, calling RPCs
			if(clientInput.leftClick) playerController.FireGun_ServerRPC();
		}
		//SetHorizontalVelocity_ServerRPC(clientInput.wasdVector);
		//LookAt_ServerRPC(clientInput.mouseVector);
		
	}
}
