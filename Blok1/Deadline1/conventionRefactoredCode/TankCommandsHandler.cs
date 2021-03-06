using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//dit script zorgt ervoor dat je als speler tanks kan selecteren door deze in een list op te slaan 
//ook kan dit script mbv een Formation script aan de geselecteerde tanks een opdracht geven om ergens heen te lopen
public class TankCommandsHandler : MonoBehaviour {
	
	private List<Unit> selectedUnits = new List<Unit>();

	private float doubleClickInterval = 0.5f;
	private float lastClickTime = -1;

	public List<Formation> formationScripts = new List<Formation>();//twijfelachtig systeem

	private string myCivTag;
	private LayerMask clickLM;

	void Start(){
		myCivTag = TankManager.Instance.myCivTag;
		clickLM = LayerMask.GetMask ("Unit", "Terrain");
	}

	void Update(){
		if(Input.GetMouseButtonUp(0)){
			Clicked ();
		}
		else if(Input.GetMouseButtonUp(1) && selectedUnits.Count > 0){
			RightClicked ();
		}
	}

	void Clicked(){
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, 100, clickLM)) {
			Transform hitParent = hit.transform.root;
			if (hitParent != null && hitParent.tag == myCivTag) {
				if ((Time.time - lastClickTime) < doubleClickInterval) {
					SelectUnitsOfType (getUnitType (hitParent.name));
				} else {
					SelectOneUnit (hitParent.name);
				}
				lastClickTime = Time.time;
			} 
			else {
				int amountOfSelectedUnits = selectedUnits.Count;
				for(int i = 0; i < amountOfSelectedUnits; i ++){
					if(selectedUnits[i] != null)	
						selectedUnits[i].SelectMe (false);
				}
				selectedUnits.Clear ();
			}
		}
	}
	void RightClicked(){
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, 100, clickLM)){
			if (hit.collider.tag == "floor") {
				foreach(Unit u in selectedUnits){
					u.SetTarget (null);
				}

				if (selectedUnits.Count == 1) {
					Movement unitMovement = selectedUnits [0].GetComponent<Movement> ();
					unitMovement.MakeNewTargetLoc (hit.point);
				} 
				else {
					Formation newFormation = new Formation (this);
					formationScripts.Add (newFormation);
					StartCoroutine (newFormation.SendUnits (selectedUnits.ToArray (), hit.point));
				}
			} else {
				Transform clickedUnit = hit.transform.root;
				if(clickedUnit.tag != myCivTag){
					if (selectedUnits.Count == 1) {
						selectedUnits[0].SetTarget (clickedUnit);
					} 
					else {
						foreach(Unit u in selectedUnits){
							u.SetTarget (clickedUnit);
						}
					}
				}
			}
		}
	}

	void SelectUnitsOfType(string unitType){
		GameObject[] allUnits = GameObject.FindGameObjectsWithTag (myCivTag);
		foreach(GameObject unit in allUnits){
			Unit unitScript = unit.GetComponent<Unit> ();
			if (getUnitType (unit.name) == unitType && !selectedUnits.Contains(unitScript)) {
				selectedUnits.Add (unitScript);
				unitScript.SelectMe (true);
			} 
			else if(getUnitType(unit.name) != unitType && selectedUnits.Contains(unitScript)) {
				selectedUnits.Remove (unitScript);
				unitScript.SelectMe (false);
			}
		}
	}

	void SelectOneUnit(string unitName){
		GameObject[] myCivUnits = GameObject.FindGameObjectsWithTag (myCivTag);
		foreach(GameObject unit in myCivUnits){
			Unit unitScript = unit.GetComponent<Unit> ();
			if (unit.name == unitName && !selectedUnits.Contains(unitScript)) {
				selectedUnits.Add (unitScript);
				unitScript.SelectMe (true);
			} 
			else if(unit.name != unitName && selectedUnits.Contains(unitScript)) {
				selectedUnits.Remove (unitScript);
				unitScript.SelectMe (false);
			}
		}
	}

	string getUnitType(string unitName) {
		string[] splittedStr = unitName.Split ("|"[0]);
		return splittedStr[0];
	}

	public void CancelUnitMovement(Unit u){//call when a unit has died
		if(selectedUnits.Contains (u)){
			selectedUnits.Remove (u);
		}
	}
}
