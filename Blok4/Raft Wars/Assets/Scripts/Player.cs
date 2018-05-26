﻿using UnityEngine;
using UnityEngine.Networking;

//this script is used to get the structure the action handling of the player 
//whether the player is allowed to do an action is communicated from the ActionManager
[RequireComponent(typeof(RaftMovement))]
public class Player : NetworkBehaviour {

	public PlayerInfo myInfo; 
	private bool canDoAction;

	private RaftActionPerformer[] raftActions;

	#region initialization handling
	private void Start(){
		raftActions = GetComponents<RaftActionPerformer> ();
		if (isServer) {
			TurnManager.instance.InitializeServerPlayer (this);
		}
	} 

	public override void OnStartLocalPlayer () {
		myInfo.playerName = GameManager.instance.GetNickname ();
		//TurnManager.instance.CmdInitializePlayer (myInfo);
	}

	[ClientRpc]//client initialization:
	public void RpcInitializeRaft(int raftID){ 
		bool playerIsServer = (raftID == 0);
		transform.name = playerIsServer ? "Raft_Server" : "Raft_Client";
		//myInfo.playerName = playerIsServer ? "ServerRaft" : "ClientRaft";
		myInfo.playerColor = playerIsServer ? Color.blue : Color.red; 
		GetComponent<SpriteRenderer> ().color = Color.Lerp(myInfo.playerColor, Color.white, 0.9f);
		transform.Translate (transform.right * raftID);

		Debug.Log ("initialize raft with id: " + raftID);
	}
	#endregion

	private void Update(){
		if (!isLocalPlayer || !canDoAction)
			return;

		//check what action we will do
		foreach (RaftActionPerformer actionPerformer in raftActions) {
			bool actionStarted = false;
			actionPerformer.EvaluateInput (out actionStarted);
			if (actionStarted) {
				canDoAction = false;
				break;
			}
		}
	}

	[ClientRpc]
	public void RpcGrantActionPermission(){
		GrantActionPermission ();
	}

	//allow the player to do an action
	public void GrantActionPermission(){
		canDoAction = true;
	}

	public void OnActionStarted(){
		canDoAction = false;
	}
}

[System.Serializable]
public struct PlayerInfo{
	public int raftID;//is only updated on the server
	public NetworkInstanceId netID;//is only updated on the server
	public string playerName;
	public Color playerColor;
}
