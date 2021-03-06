using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//in dit script wordt een formatie gemaakt.
//deze formatie wordt gemaakt op de units die als parameters meegegeven worden
public class Formation {

	private TankCommandsHandler selectScript;

	private Transform[] units;
	private Movement[] allMoveScripts;//ordered movementarray
	private Vector3[] unitEndDestinations;
	private Vector3 endDestination;

	private Transform middleIndicator;

	public int troopSize;
	public float unitWidth = 0.39f;

	private Vector3 endDirection;
	const float MAX_REL_FORMATION_DELTA_DIST = 0.05f;//hoeveel verder mag een tank van een andere tank zijn voordat de tank op volle speed gaat rijden

	private static int uniqueID;
	private int formationAssignmentID = 0;

	private List<Transform> unitsList = new List<Transform>();

	public Formation(TankCommandsHandler s){
		selectScript = s;
		uniqueID ++;
		formationAssignmentID = uniqueID;
		middleIndicator = new GameObject("middle Indicator: " + formationAssignmentID).transform;
	}

	public IEnumerator SendUnits(Unit[] unitScripts, Vector3 endDest){
		List<Transform> unitsTr = new List<Transform> ();
		foreach(Unit u in unitScripts){
			unitsTr.Add (u.transform);
		}
		units = unitsTr.ToArray ();
		endDestination = endDest;
		troopSize = units.Length;
		allMoveScripts = new Movement[troopSize];
		unitEndDestinations = new Vector3[troopSize];

		UnitSetup ();
		SetCenterObject ();
		GroupUnits ();

		yield return new WaitUntil (() => UnitsInFormation());
		yield return new WaitUntil (() => UnitsAligned());

		selectScript.formationScripts.Remove (this);
	}

	void UnitSetup(){//stop attacking and give formationid
		for (int i = 0; i < troopSize; i++) {
			Unit unitScript = units [i].GetComponent<Unit> ();
			unitScript.target = null;
			Movement mov = units [i].GetComponent<Movement> ();
			mov.ForceStopMoving ();
			mov.FormationAssignmentID = formationAssignmentID;
		}
	}

	void SetCenterObject(){
		float closestDist = -1;

		for(int i = 0; i < troopSize; i ++){//find the unit that is the closest to the end
			float dist = Vector3.Distance (units [i].position, endDestination);
			if(dist < closestDist || closestDist == -1){
				closestDist = dist;
				middleIndicator.position = units [i].position;
			}
		}
		middleIndicator.LookAt (endDestination);

		float leftX = 0, rightX = 0;
		for(int i = 0; i < troopSize; i ++){
			if(units[i].position != middleIndicator.position){
				float relX = middleIndicator.InverseTransformPoint (units[i].position).x;
				if(relX < leftX){
					leftX = relX;
				}
				else if(relX > rightX){
					rightX = relX;
				}
			}
		}

		float relXMiddlePoint = (rightX + leftX) / 2;
		Vector3 middlePoint = middleIndicator.position + middleIndicator.right * relXMiddlePoint;
		middleIndicator.position = middlePoint;
		middleIndicator.LookAt (endDestination);
	}

	void GroupUnits(){
		Transform[] leftToRightUnits = new Transform[troopSize];
		float[] relXPosses = new float[troopSize];

		for(int i = 0; i < troopSize; i ++){
			float relX = middleIndicator.InverseTransformPoint (units[i].position).x;
			relXPosses [i] = relX;
		}

		float[] orderedXPosses = relXPosses.OrderBy (pos => pos).ToArray();
		for(int i = 0; i < troopSize; i ++){
			for(int j = 0; j < troopSize; j ++){
				if (orderedXPosses [i] == relXPosses [j]) {
					leftToRightUnits [i] = units [j];
					j = troopSize;
				}
			}
		}

		endDirection = Vector3.Distance (middleIndicator.position, endDestination) * middleIndicator.forward;
		Vector3 unitPosition = middleIndicator.position - (middleIndicator.right * unitWidth * troopSize / 2);
		for(int i = 0; i < troopSize; i ++){
			Movement unitMovement = leftToRightUnits [i].GetComponent<Movement> ();
			unitMovement.MakeNewTargetLoc(unitPosition, MoveState.move, true);
			unitPosition += middleIndicator.right * unitWidth;

			allMoveScripts[i] = unitMovement;
			unitEndDestinations [i] = unitPosition + middleIndicator.forward + endDirection;
		}

		units = leftToRightUnits;
	}

	bool UnitsInFormation(){
		bool unitsAreInFormation = true;
		for(int i = 0; i < troopSize; i ++){
			Movement unitMovement = allMoveScripts [i];
			if(unitIsInThisFormation(unitMovement)){
				if (unitMovement.moveState == MoveState.stay) {
					unitMovement.MakeNewTargetLoc(units[i].position + endDirection, MoveState.moveSlow, true);
				} 
				else if(unitMovement.moveState == MoveState.move) {
					unitsAreInFormation = false;
				}
			}
		}

		return unitsAreInFormation;
	}

	bool UnitsAligned(){
		bool aligned = true;
		for(int i = 0; i < troopSize; i ++){
			Movement unitMovement = allMoveScripts [i];
			if (unitMovement.moveState == MoveState.moveSlow && unitIsInThisFormation(unitMovement)){
				bool canMoveNormal = true;

				for (int j = 0; j < troopSize; j++) {
					if (i != j) {
						Vector3 relPos = units [i].InverseTransformPoint (units [j].position);
						if (relPos.z < -MAX_REL_FORMATION_DELTA_DIST) {
							aligned = false;
							canMoveNormal = false;
						} 
					}
				}

				if (canMoveNormal) {
					allMoveScripts [i].MakeNewTargetLoc (unitEndDestinations [i], MoveState.move, true);
				}
			}
		}

		return aligned;
	}

	bool unitIsInThisFormation(Movement moveScript){
		if (moveScript.FormationAssignmentID == formationAssignmentID) {
			return true;
		} 
		else {
			return false;
		}
	}
}
