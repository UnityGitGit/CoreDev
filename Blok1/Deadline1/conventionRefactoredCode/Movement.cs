using UnityEngine;
using System.Collections;

//dit script zorgt er mbv de navmesh voor dat de tank naar een bepaald punt toeloopt.
//deze opdracht kan ofwel door Formation.cs of door Select.cs gegeven worden
public class Movement : MonoBehaviour {
	
	private Unit unitScript;

	public MoveState moveState = MoveState.stay;//stay, move, of moveSlow
	public float movementSpeed = 10;
	const float SLOW_MOVE_FACTOR = 0.34f;

	private UnityEngine.AI.NavMeshAgent agent;
	private float MinDistance{
		get{ 
			return agent.stoppingDistance;
		}
	}

	private int formationAssignmentID;
	public int FormationAssignmentID{
		get{ 
			return formationAssignmentID;
		}
		set{ 
			formationAssignmentID = value;
		}
	}

	void Start () {
		unitScript = GetComponent<Unit> ();
		agent = GetComponent<UnityEngine.AI.NavMeshAgent> ();
		agent.speed = movementSpeed;
	}
		
	void Update () {
		if (moveState != MoveState.stay) {
			if(agent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete){
				print (transform.name + " pathstatus: " + agent.pathStatus);
			}
			if (agent.velocity.magnitude < 0.1f && agent.remainingDistance < MinDistance) {
				moveState = MoveState.stay;
			}
		} 
	}

	public void MakeNewTargetLoc(Vector3 loc, MoveState newMoveState = MoveState.move, bool isFormationAssignment = false){
		if (!isFormationAssignment) {
			FormationAssignmentID = 0;
		}
		if ((isFormationAssignment && FormationAssignmentID != 0) || !isFormationAssignment) {
			float dist = Vector3.Distance (transform.position, loc);
			if (dist > MinDistance) {
				unitScript.target = null;
				agent.destination = loc;
				moveState = newMoveState;
			} 
			else {
				print (dist + " is smaller than " + MinDistance + " for: " + transform.name);
				moveState = MoveState.stay;
				agent.destination = transform.position;
			}

			if (moveState == MoveState.moveSlow) {
				agent.speed = movementSpeed * SLOW_MOVE_FACTOR;
			}
			else {
				agent.speed = movementSpeed;
			}
		}
	}

	public void ForceStopMoving(){
		FormationAssignmentID = 0;
		moveState = MoveState.stay;
		agent.destination = transform.position;
	}
}

public enum MoveState{
	stay,
	move,
	moveSlow
}