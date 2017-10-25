﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	private static GameManager instance;
	public static GameManager Instance{
		get{ 
			if(instance == null){
				GameObject gm = new GameObject ("Game Manager");
				gm.AddComponent <GameManager> ();
			}
			return instance;
		}
		set{ 
			instance = value;
		}
	}
		
	private GameState myGameState = GameState.menu;
	public bool IsPaused{//used by GameMenu
		get{ 
			if (myGameState == GameState.paused) {
				return true;
			} 
			else {
				return false;
			}
		}
	}

	private Scene currentActiveScene;
	private int currentLevel = 1;

	const string levelSceneName = "Level";
	const string menuSceneName = "Menu";
	private string CurrentSceneName {
		get { 
			return levelSceneName + currentLevel.ToString ();
		}
	}

	[SerializeField]private int winTotemCount = 3;
	private int goodTotemCount = 0;

	private GameMenu menuScript;

	void Awake(){
		Instance = this;

		//check if the active scene is correct (== not the base scene)
		Scene activeScene = SceneManager.GetActiveScene ();
		if (activeScene == gameObject.scene) {
			SetCorrectActiveScene ();	
		} 
		else {
			currentActiveScene = activeScene;
		}

		if (currentActiveScene.name.StartsWith (levelSceneName)) {
			myGameState = GameState.playing;
		}
	}

	public void InitMenuScript(GameMenu subscriber){
		menuScript = subscriber;
	}

	//find the other scene and set it to active
	void SetCorrectActiveScene(){
		for(int i = 0; i < SceneManager.sceneCount; i ++){
			Scene iScene = SceneManager.GetSceneAt (i);
			string iSceneName = iScene.name;

			if(iSceneName == menuSceneName || iSceneName == CurrentSceneName){
				SetNewActiveScene (iScene);
			}
		}
	}

	public void StartGame(){
		myGameState = GameState.playing;
		StartCoroutine(LoadNewScene(CurrentSceneName));
	}

	public void QuitGame(){
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit ();
		#endif
	}

	public void TogglePause(){
		if(IsPaused){
			myGameState = GameState.playing;
			Time.timeScale = 1f;
		}
		else{
			myGameState = GameState.paused;
			Time.timeScale = 0f;
		}
	}

	public void ReturnToMenu(){
		myGameState = GameState.menu;
		StartCoroutine(LoadNewScene(menuSceneName));
	}

	private IEnumerator LoadNewScene(string newSceneName){
		SceneManager.UnloadSceneAsync (currentActiveScene);
		AsyncOperation loadInstruction = SceneManager.LoadSceneAsync (newSceneName, LoadSceneMode.Additive);
		yield return loadInstruction;

		Scene loadedScene = SceneManager.GetSceneByName (newSceneName);
		SetNewActiveScene (loadedScene);
	}

	private void SetNewActiveScene(Scene targetScene){
		SceneManager.SetActiveScene (targetScene);
		currentActiveScene = targetScene;
	}

	public void CaptureTotem(Faction newTotemFaction){
		if (newTotemFaction == Faction.Good) {
			goodTotemCount++;
			if (goodTotemCount == winTotemCount) {
				menuScript.ShowVictoryPanel ();
				TogglePause ();
			}
		}
		else {
			goodTotemCount--;
		}
	}
}

public enum GameState{
	menu, playing, paused
}
