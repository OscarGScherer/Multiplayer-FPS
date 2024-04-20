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
	
	private InputInfo clientInput = new InputInfo(Vector2.zero, Vector2.zero, false, false);
	private PlayerController playerController;
	
	void Awake()
	{
		playerController = GetComponent<PlayerController>();
		//Cursor.visible = false;
	}
	
	void SetLayerAllChildren(Transform root, int layer)
	{
		var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (var child in children)
		{
			child.gameObject.layer = layer;
		}
	}
	
	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		
		GetComponent<PlayerController>().enabled = false;
		transform.position += new Vector3(Random.Range(-2,2), 3, Random.Range(-2,2));
		GetComponent<PlayerController>().enabled = true;
		if(!IsOwner)
		{
			playerController.DisableCamera();
		}
	}
	
	public void Start()
	{
		if(MatchInfo.playerCount.Value == 1) SetLayerAllChildren(transform, 7);
		if(IsOwner) IncrementPlayerCount_ServerRPC();
	}

	[ServerRpc] public void IncrementPlayerCount_ServerRPC(){ MatchInfo.playerCount.Value++; }
	[ServerRpc] public void SetHorizontalVelocity_ServerRPC(Vector2 velocity) { playerController.SetHorizontalVelocity(velocity); }
	
	[ServerRpc] public void FireGun_ServerRPC() 
	{ 
		playerController.FireGun();
	}
	
	[ServerRpc] public void LookAt_ServerRPC(Vector2 mouseRotation) { playerController.LookAt(mouseRotation); }

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
			playerController.SetHorizontalVelocity(clientInput.wasdVector);
			playerController.LookAt(clientInput.mouseVector);
			if(clientInput.spacebar) playerController.Jump(5f);
			if(clientInput.leftClick) FireGun_ServerRPC();
		}
		//SetHorizontalVelocity_ServerRPC(clientInput.wasdVector);
		//LookAt_ServerRPC(clientInput.mouseVector);
		
	}
}
