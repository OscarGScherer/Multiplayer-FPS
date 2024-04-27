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
	
	void Awake()
	{
		playerController = GetComponent<PlayerController>();
		//Cursor.visible = false;
	}
	
	public override void OnNetworkSpawn()
	{	
		if(IsOwner) playerController.EnableCamera();
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
