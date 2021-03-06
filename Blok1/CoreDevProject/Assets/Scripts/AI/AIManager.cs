using UnityEngine;
using System.Collections.Generic;

public class AIManager : MonoBehaviour {

	private static AIManager instance;
	public static AIManager Instance{
		get{ 
			if (instance == null) {
				instance = new GameObject ("EnemyManager").AddComponent<AIManager>();
			}
			return instance;
		}
		set{ 
			instance = value;
		}
	}

	private Transform player;

	private List<AI> myAIs;
	public delegate void OnAIChanged(AI target);
	public event OnAIChanged onAIChanged;//als een ai dood gaat of als een ai getrapped is

	private void Awake(){
		Instance = this;	
		myAIs = new List<AI> (10);
		player = GameObject.FindGameObjectWithTag ("Player").transform;
		GameManager.Instance.InitAIManager ();
	}

	private void Update(){
		for(int i = 0; i < myAIs.Count; i ++){
			myAIs[i].UpdateFSM ();
		}
	}

	public void SubscribeAI(AI newAI, out Transform _player){
		myAIs.Add (newAI);
		_player = player;
		newAI.onDeath += MyAIDies;
		GameManager.Instance.AddEvilAI ();
	}

	public void ChangeAIState(AI disabledAI){//of ai wordt bevriend, of ai gaat dood
		onAIChanged (disabledAI);//GameManager gebruikt dit voor score enzo

		if (disabledAI.myFaction == Faction.Good) {
			myAIs.Remove (disabledAI);
			disabledAI.onDeath -= MyAIDies;
		}
		else {
			disabledAI.myFaction = Faction.Good;
		}
	}
	
	private void MyAIDies(Character sender){
		AI deadAI = (AI)sender;
		ChangeAIState (deadAI);
	}
}
